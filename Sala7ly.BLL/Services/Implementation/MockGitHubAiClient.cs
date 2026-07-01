using Sala7ly.BLL.DTOs.AiDTOs;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    /// <summary>
    /// Deterministic stand-in for <see cref="GitHubAiClient"/>, enabled via
    /// AI:GitHub:UseMock. Every prompt built by <see cref="PromptBuilder"/>
    /// starts with a "[TaskType: X]" marker; this mock routes off that
    /// marker instead of guessing from natural-language content (the
    /// previous implementation matched on Arabic substrings, which broke
    /// the moment a prompt was reworded).
    /// </summary>
    public class MockGitHubAiClient : IGitHubAiClient
    {
        public Task<string> CompleteAsync(
            string model,
            string userPrompt,
            string? systemPrompt = null,
            int maxTokens = 500,
            List<ChatMsg>? history = null)
        {
            var response = ExtractTaskType(userPrompt) switch
            {
                "FollowUpQuestion" => """
                    { "question": "", "isComplete": true }
                    """,

                "RefineRequest" => """
                    {
                      "refinedDescription": "تسريب مياه من أسفل حوض المطبخ يحتاج إلى إصلاح عاجل",
                      "aiSummary": "تسريب مياه يحتاج إصلاح عاجل",
                      "suggestedCategoryId": 1,
                      "suggestedUrgency": "high"
                    }
                    """,

                "CategoryVerification" => """
                    { "suggestedCategoryId": 1 }
                    """,

                "PriceEstimation" => """
                    {
                      "min_price": 200,
                      "max_price": 500,
                      "fair_price": 350,
                      "price_factors": ["قطع الغيار", "صعوبة الوصول"],
                      "confidence": 0.85
                    }
                    """,

                "TechnicianRerank" => """
                    [{"index":0,"score":0.85},{"index":1,"score":0.5}]
                    """,

                "DisputeAnalysis" => """
                    {
                      "case_summary": "العميل يدعي أن العمل لم يكتمل والفني يؤكد الإتمام",
                      "timeline": ["تقديم الطلب", "قبول العرض", "بدء العمل", "رفع النزاع"],
                      "customer_position_strength": 0.6,
                      "technician_position_strength": 0.4,
                      "recommendation": "refund_partial",
                      "recommended_amount": 150,
                      "reasoning": "الأدلة تشير إلى اكتمال جزئي للعمل"
                    }
                    """,

                "ReviewSummary" => """
                    {
                      "summary": "فني محترف وملتزم بالمواعيد مع خبرة جيدة في التكييف",
                      "strengths": ["الالتزام بالمواعيد", "الاحترافية", "جودة العمل"],
                      "complaints": ["السعر مرتفع أحياناً"],
                      "sentiment_score": 0.88
                    }
                    """,

                _ => "مرحباً! يمكنني مساعدتك. هل تحتاج إلى معلومات إضافية؟"
            };

            return Task.FromResult(response);
        }

        public Task<string> CompleteWithImageAsync(
            string model,
            string userPrompt,
            string base64Image,
            string mediaType,
            int maxTokens = 500)
        {
            return Task.FromResult("""
                {
                  "detected_problem": "تسريب مياه من الأنابيب",
                  "category": "plumbing",
                  "severity": "moderate",
                  "affected_components": ["أنبوب المياه", "الوصلات"],
                  "suggested_description": "يوجد تسريب في أنبوب المياه الرئيسي يحتاج إلى إصلاح",
                  "confidence": 0.82
                }
                """);
        }

        private static string ExtractTaskType(string prompt)
        {
            const string marker = "[TaskType: ";
            var start = prompt.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return string.Empty;

            start += marker.Length;
            var end = prompt.IndexOf(']', start);
            return end < 0 ? string.Empty : prompt[start..end];
        }
    }
}