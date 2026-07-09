using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.DTOs.AiSupportDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class AiSupportService : IAiSupportService
    {
        private readonly IServiceRequestRepository _requestRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IConfiguration _config;

        public AiSupportService(
            IServiceRequestRepository requestRepo,
            ICustomerRepository customerRepo,
            IConfiguration config)
        {
            _requestRepo = requestRepo;
            _customerRepo = customerRepo;
            _config = config;
        }

        public async Task<AiSupportResponseDto> AskAsync(string userId, AiSupportRequestDto dto)
        {
            // 1) ground the AI with the user's own recent requests
            var context = await BuildUserContextAsync(userId);

            // 2) build the message list: system + optional history + current question
            var messages = new List<object>
            {
                new { role = "system", content = SystemPrompt(context) }
            };

            if (dto.History is not null)
                foreach (var turn in dto.History.TakeLast(6))
                    messages.Add(new { role = turn.Role, content = turn.Content });

            messages.Add(new { role = "user", content = dto.Message });

            // 3) read config (correct paths from appsettings AI:GitHub)
            var token = _config["AI:GitHub:Token"];
            var endpoint = _config["AI:GitHub:Endpoint"];   // https://models.inference.ai.azure.com
            var model = _config["AI:GitHub:HaikuModel"];    // gpt-4o-mini

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var payload = new
            {
                model,
                messages,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                $"{endpoint}/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return new AiSupportResponseDto
                {
                    Reply = $"خطأ [{(int)response.StatusCode}]: {errorBody}"
                };
            }

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return new AiSupportResponseDto
            {
                Reply = reply ?? "عذراً، لم أفهم سؤالك. هل يمكنك إعادة صياغته؟"
            };
        }

        private async Task<string> BuildUserContextAsync(string userId)
        {
            var customer = await _customerRepo.GetByUserIdAsync(userId);
            if (customer is null) return "المستخدم ليس لديه ملف عميل.";

            var requests = await _requestRepo.GetByCustomerIdAsync(customer.Id);
            if (requests is null || !requests.Any())
                return "العميل ليس لديه أي طلبات حتى الآن.";

            var sb = new StringBuilder();
            sb.AppendLine("طلبات العميل الحالية:");
            foreach (var r in requests.Take(10))
                sb.AppendLine($"- طلب #{r.Id}: {r.Title} — الحالة: {r.Status}");

            return sb.ToString();
        }

        private static string SystemPrompt(string userContext) => $@"
أنت مساعد دعم العملاء لمنصة ""صلّحلي"" لخدمات الصيانة المنزلية.

معلومات عن كيفية عمل المنصة:
- العميل ينشئ طلب خدمة ويحدد التفاصيل والعنوان والفئة.
- الفنيون يقدمون عروضهم (أسعارهم) على الطلب.
- العميل يراجع العروض ويقبل العرض المناسب، فيصبح الطلب ""معيّن"".
- الفني يبدأ العمل فيصبح الطلب ""قيد التنفيذ"".
- عند الانتهاء، العميل يؤكد الإكمال فيصبح الطلب ""مكتمل"".
- بعد الإكمال، يمكن للعميل تقييم الفني.
- المدفوعات تتم عبر المحفظة، ويمكن شحنها عبر Stripe.
- الفنيون يجب أن يوثّقوا حساباتهم بالمستندات قبل استقبال الطلبات.

مهمتك مساعدة العملاء في أمور تتعلق بطلباتهم، العروض المقدمة من الفنيين، وحالة الخدمة.
كن ودوداً ومختصراً وأجب بالعربية دائماً.
إذا سُئلت عن أمر خارج نطاق المنصة، اعتذر بلطف ووجّه المستخدم للدعم البشري.
لا تختلق معلومات؛ إذا لم تكن تعرف، قل ذلك.

معلومات عن حساب المستخدم الحالي:
{userContext}
";
    }
}