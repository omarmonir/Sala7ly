namespace Sala7ly.BLL.Services.Implementation
{
    /// <summary>
    /// Central place for all AI prompt strings.
    /// Keeps services thin and makes prompt tuning easy.
    /// </summary>
    public static class PromptBuilder
    {
        // ── Price Estimation ──────────────────────────────────────────────────

        public static string PriceEstimationUser(
            string categoryAr,
            string description,
            string urgency,
            string district,
            decimal avgPrice,
            decimal minPrice,
            decimal maxPrice)
        {
            return $$"""
                أنت خبير تسعير خدمات منزلية في مصر.
                قدّر نطاق السعر لهذا الطلب وأعد JSON فقط بدون أي نص إضافي.

                الفئة: {{categoryAr}}
                الوصف: {{description}}
                الإلحاح: {{urgency}}
                المنطقة: {{district}}

                أسعار تاريخية مقبولة (جنيه مصري):
                  متوسط: {{avgPrice}}، أدنى: {{minPrice}}، أعلى: {{maxPrice}}

                الصيغة المطلوبة:
                {
                  "min_price": <number>,
                  "max_price": <number>,
                  "fair_price": <number>,
                  "price_factors": ["<factor1>", "<factor2>"],
                  "confidence": <0.0-1.0>
                }
                """;
        }

        // ── Image Analysis ────────────────────────────────────────────────────

        public static string ImageAnalysisUser()
        {
            return """
                أنت مساعد خبير في تشخيص مشاكل المنازل.
                حلّل هذه الصورة وأعد JSON فقط بدون أي نص إضافي.

                الصيغة المطلوبة:
                {
                  "detected_problem": "<وصف المشكلة>",
                  "category": "<plumbing|electrical|ac|painting|carpentry|other>",
                  "severity": "<low|moderate|high|critical>",
                  "affected_components": ["<component1>", "<component2>"],
                  "suggested_description": "<وصف مقترح للعميل>",
                  "confidence": <0.0-1.0>
                }
                """;
        }

        // ── Request Refinement ────────────────────────────────────────────────

        public static string RequestRefinementSystem() =>
            "أنت مساعد ذكي متخصص في خدمات الصيانة المنزلية في مصر. " +
            "مهمتك تحليل طلبات العملاء وتحسينها وطرح أسئلة توضيحية عند الحاجة. " +
            "أجب دائماً بصيغة JSON نقية بدون أي نص إضافي.";

        public static string RequestRefinementUser(string title, string description) =>
            $$"""
            حسّن هذا الطلب وأعد JSON فقط:
            العنوان: {{title}}
            الوصف: {{description}}

            {
              "refined_description": "<وصف محسّن>",
              "suggested_category": "<category>",
              "urgency": "<low|medium|high>",
              "follow_up_questions": ["<q1>", "<q2>"],
              "estimated_duration": "<duration>"
            }
            """;

        // ── Review Summarisation ──────────────────────────────────────────────

        public static string ReviewSummaryUser(IEnumerable<string> reviews)
        {
            var joined = string.Join("\n- ", reviews);
            return $$"""
                لخّص تقييمات هذا الفني في JSON فقط:
                - {{joined}}

                {
                  "summary": "<ملخص عام>",
                  "strengths": ["<strength1>"],
                  "complaints": ["<complaint1>"],
                  "sentiment_score": <0.0-1.0>
                }
                """;
        }

        // ── Dispute Analysis ──────────────────────────────────────────────────

        public static string DisputeAnalysisUser(
            string customerClaim,
            string technicianClaim,
            string requestDetails)
        {
            return $$"""
                حلّل هذا النزاع وأعد JSON فقط:

                تفاصيل الطلب: {{requestDetails}}
                ادعاء العميل: {{customerClaim}}
                ادعاء الفني: {{technicianClaim}}

                {
                  "case_summary": "<ملخص>",
                  "timeline": ["<event1>"],
                  "customer_position_strength": <0.0-1.0>,
                  "technician_position_strength": <0.0-1.0>,
                  "recommendation": "<refund_full|refund_partial|no_refund>",
                  "recommended_amount": <number or null>,
                  "reasoning": "<التبرير>"
                }
                """;
        }
    }
}