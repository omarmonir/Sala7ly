using System.Net.Http.Json;
using System.Text.Json;
using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class RequestRefinerService : IRequestRefinerService
    {
        private readonly HttpClient _http;
        private readonly string _model = "gpt-4o-mini";  // GitHub Models
        private readonly string _endpoint = "https://models.inference.ai.azure.com/chat/completions";

        public RequestRefinerService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("GitHubModels");
        }

        // ── Step 1: ask one follow-up question ───────────────────
        public async Task<FollowUpDto> AskFollowUpAsync(FollowUpRequestDto dto)
        {
            var answersCount = dto.PreviousAnswers.Count;
            var previousQA = answersCount > 0
                ? string.Join("\n", dto.PreviousAnswers.Select((a, i) => $"إجابة {i + 1}: {a}"))
                : "لا توجد إجابات سابقة.";
            if (dto.PreviousAnswers.Count >= 3)
                return new FollowUpDto { Question = "", IsComplete = true };

            var prompt = $$"""
    أنت مساعد متخصص في خدمات الصيانة المنزلية في مصر.
    العميل كتب: "{{dto.RawDescription}}"
    الفئات المتاحة: {{string.Join("، ", dto.Categories)}}

    الإجابات السابقة ({{answersCount}} من أصل 3 كحد أقصى):
    {{previousQA}}

    قواعد صارمة:
    - إذا وصل عدد الإجابات إلى 3 أو أكثر، أعد: {"question": "", "isComplete": true}
    - إذا كان الوصف واضحاً بما يكفي، أعد: {"question": "", "isComplete": true}
    - لا تكرر سؤالاً سبق الإجابة عليه
    - إذا احتجت سؤالاً، اسأل سؤالاً واحداً جديداً فقط بالعربية

    رد بـ JSON فقط: {"question": "...", "isComplete": false}
    """;
            var result = await CallModelAsync(prompt);

            try
            {
                var parsed = JsonSerializer.Deserialize<FollowUpDto>(result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return parsed ?? new FollowUpDto { IsComplete = true };
            }
            catch
            {
                return new FollowUpDto { IsComplete = true };
            }
        }

        // ── Step 2: generate refined description + suggestions ───
        public async Task<RefineResultDto> RefineAsync(RefineRequestDto dto, List<string> allAnswers)
        {
            var answersBlock = allAnswers.Count > 0
                ? string.Join("\n", allAnswers.Select((a, i) => $"إجابة {i + 1}: {a}"))
                : "";

            var prompt = $$"""
                        أنت مساعد متخصص في خدمات الصيانة المنزلية في مصر.
                        وصف العميل الأصلي: "{{dto.RawDescription}}"
                        {{answersBlock}}
                        الفئات المتاحة (id:name): {{string.Join("، ", dto.Categories)}}

                        المطلوب:
                        1. أعد كتابة الوصف بشكل احترافي وواضح بالعربية (3-5 جمل).
                        2. اختر الفئة الأنسب من القائمة.
                        3. حدد مستوى الاستعجال: low / medium / high.

                        رد بـ JSON فقط:
                        {
                          "refinedDescription": "...",
                          "aiSummary": "جملة واحدة ملخص",
                          "suggestedCategoryId": 0,
                          "suggestedUrgency": "medium"
                        }
                        """;

            var raw = await CallModelAsync(prompt);

            // store Q&A history as JSON
            var refinementJson = JsonSerializer.Serialize(new
            {
                original = dto.RawDescription,
                answers = allAnswers,
                generatedAt = DateTime.UtcNow
            }, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                var categoryIdRaw = root.GetProperty("suggestedCategoryId").GetInt32();
                var matchedCategory = dto.Categories
                    .FirstOrDefault(c => c.StartsWith($"{categoryIdRaw}:"));

                return new RefineResultDto
                {
                    RefinedDescription = root.GetProperty("refinedDescription").GetString()!,
                    AiSummary = root.GetProperty("aiSummary").GetString()!,
                    SuggestedCategoryId = categoryIdRaw > 0 ? categoryIdRaw : null,
                    SuggestedCategoryName = matchedCategory?.Split(':').ElementAtOrDefault(1),
                    SuggestedUrgency = root.GetProperty("suggestedUrgency").GetString()!,
                    RefinementJson = refinementJson
                };
            }
            catch
            {
                return new RefineResultDto
                {
                    RefinedDescription = dto.RawDescription,
                    AiSummary = dto.RawDescription,
                    SuggestedUrgency = "medium",
                    RefinementJson = refinementJson
                };
            }
        }

        // ── Internal: call GitHub Models ─────────────────────────
        private async Task<string> CallModelAsync(string userPrompt)
        {
            var body = new
            {
                model = _model,
                messages = new[]
                {
                new { role = "user", content = userPrompt }
            },
                temperature = 0.3,
                max_tokens = 500
            };

            var response = await _http.PostAsJsonAsync(_endpoint, body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }
    }
}
