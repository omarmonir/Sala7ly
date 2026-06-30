using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class MockGitHubAiClient : IGitHubAiClient
    {
        public Task<string> CompleteAsync(
            string model,
            string userPrompt,
            string? systemPrompt = null,
            int maxTokens = 500,
            List<ChatMsg>? history = null)
        {
            if (userPrompt.Contains("refined_description") || userPrompt.Contains("حسّن"))
                return Task.FromResult("""
                    {
                      "refined_description": "تسريب مياه من أسفل حوض المطبخ يحتاج إلى إصلاح عاجل",
                      "suggested_category": "plumbing",
                      "urgency": "high",
                      "follow_up_questions": ["هل التسريب مستمر؟", "هل يوجد صوت طقطقة؟"],
                      "estimated_duration": "1-2 ساعة"
                    }
                    """);

            if (userPrompt.Contains("min_price") || userPrompt.Contains("تسعير"))
                return Task.FromResult("""
                    {
                      "min_price": 200,
                      "max_price": 500,
                      "fair_price": 350,
                      "price_factors": ["قطع الغيار", "صعوبة الوصول"],
                      "confidence": 0.85
                    }
                    """);

            if (userPrompt.Contains("case_summary") || userPrompt.Contains("نزاع"))
                return Task.FromResult("""
                    {
                      "case_summary": "العميل يدعي أن العمل لم يكتمل والفني يؤكد الإتمام",
                      "timeline": ["تقديم الطلب", "قبول العرض", "بدء العمل", "رفع النزاع"],
                      "customer_position_strength": 0.6,
                      "technician_position_strength": 0.4,
                      "recommendation": "refund_partial",
                      "recommended_amount": 150,
                      "reasoning": "الأدلة تشير إلى اكتمال جزئي للعمل"
                    }
                    """);

            if (userPrompt.Contains("summary") || userPrompt.Contains("تقييمات"))
                return Task.FromResult("""
                    {
                      "summary": "فني محترف وملتزم بالمواعيد مع خبرة جيدة في التكييف",
                      "strengths": ["الالتزام بالمواعيد", "الاحترافية", "جودة العمل"],
                      "complaints": ["السعر مرتفع أحياناً"],
                      "sentiment_score": 0.88
                    }
                    """);

            return Task.FromResult(
                "مرحباً! يمكنني مساعدتك. هل تحتاج إلى معلومات إضافية؟");
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
    }
}
