namespace Sala7ly.BLL.Services
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

        public static string RequestRefinementFollowUpUser(
            string rawDescription,
            string categories,
            string previousQA,
            int answersCount)
        {
            return $$"""
                أنت مساعد متخصص في خدمات الصيانة المنزلية في مصر.
                الوصف الحالي: "{rawDescription}"
                الفئات المتاحة: {categories}

                الإجابات السابقة ({answersCount} من أصل 3 كحد أقصى):
                {previousQA}

                المطلوب:
                - إذا كان الوصف واضحًا بما يكفي، أعد: {"question": "", "isComplete": true}
                - إذا احتجت مزيداً من المعلومات، اسأل سؤالاً واحداً فقط بالعربية.
                - لا تكرر سؤالاً سبق الإجابة عليه.

                أعد JSON فقط:
                {
                  "question": "<question>",
                  "isComplete": <true|false>
                }
                """;
        }

        public static string RequestRefinementUser(
            string rawDescription,
            string categories)
            => $$"""
                أنت مساعد متخصص في تحسين طلبات الصيانة المنزلية.
                الوصف الأصلي: "{rawDescription}"
                الفئات المتاحة: {categories}

                المطلوب:
                - أعد كتابة الوصف بشكل أوضح وأقصر بالعربية.
                - اختر الفئة الأنسب من القائمة.
                - قدرت مستوى الاستعجال: low / medium / high.
                - قدم ملخصًا قصيرًا للطلب.

                أعد JSON فقط:
                {
                  "refinedDescription": "<وصف محسّن>",
                  "aiSummary": "<ملخص>",
                  "suggestedCategoryId": <رقم الفئة>,
                  "suggestedUrgency": "<low|medium|high>"
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

        public static string SmartMatchingUser(
            string title,
            string description,
            int customerChosenCategoryId,
            string categoryList)
            => $$"""
                أنت مساعد متخصص في تصنيف طلبات الصيانة المنزلية في مصر.

                الطلب:
                العنوان: "{title}"
                الوصف: "{description}"
                الفئة التي اختارها العميل: {customerChosenCategoryId}

                الفئات المتاحة (id:الاسم):
                {categoryList}

                المطلوب:
                - حدد الفئة الأنسب من القائمة أعلاه.
                - إذا كانت فئة العميل صحيحة، أعد نفس الرقم.
                - أعد JSON فقط.

                {
                  "suggestedCategoryId": <رقم صحيح>
                }
                """;

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

        public static string SupportChatSystem()
        {
            return "أنت مساعد دعم للعملاء في منصة Sala7ly. " +
                   "أجب بصيغة واضحة ومباشرة بالعربية. " +
                   "استخدم JSON فقط للاستجابة دون شرح إضافي.";
        }

        public static string InsightsUser(
            int totalRequests,
            int completedRequests,
            int activeRequests,
            int totalTechnicians,
            int approvedTechnicians,
            int totalReviews)
        {
            return $$"""
                أنت محلل أداء منصة خدمات منزلية.
                استعرض هذه الأرقام وأنشئ ملخصاً وارداً من 3 إلى 5 رؤى عملية بالعربية:

                إجمالي الطلبات: {{totalRequests}}
                الطلبات المكتملة: {{completedRequests}}
                الطلبات النشطة: {{activeRequests}}
                إجمالي الفنيين: {{totalTechnicians}}
                الفنيين المعتمدين: {{approvedTechnicians}}
                إجمالي التقييمات: {{totalReviews}}

                أعد نصاً موجزاً من ثلاثة إلى خمسة نقاط واضحة، ولا ترد JSON.
                """;
        }
    }
}