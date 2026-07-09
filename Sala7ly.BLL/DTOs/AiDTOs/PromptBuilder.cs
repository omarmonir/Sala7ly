namespace Sala7ly.BLL.Services.Implementation
{
    /// <summary>
    /// Central place for every AI prompt string. Keeps services thin and
    /// makes prompt tuning a one-file change. Every prompt starts with a
    /// "[TaskType: X]" marker used by MockGitHubAiClient for deterministic
    /// routing; it costs the real model nothing and removes the need to
    /// guess intent from wording.
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
                [TaskType: PriceEstimation]
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
                [TaskType: ImageAnalysis]
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

        // ── Request Follow-up (create-request wizard, step 1) ─────────────────

        public static string RequestFollowUpUser(
            string rawDescription,
            IReadOnlyList<string> categories,
            IReadOnlyList<string> previousAnswers)
        {
            var previousQA = previousAnswers.Count > 0
                ? string.Join("\n", previousAnswers.Select((a, i) => $"إجابة {i + 1}: {a}"))
                : "لا توجد إجابات سابقة.";

            return $$"""
                [TaskType: FollowUpQuestion]
                أنت مساعد متخصص في خدمات الصيانة المنزلية في مصر.
                العميل كتب: "{{rawDescription}}"
                الفئات المتاحة: {{string.Join("، ", categories)}}

                الإجابات السابقة ({{previousAnswers.Count}} من أصل 3 كحد أقصى):
                {{previousQA}}

                قواعد صارمة:
                - إذا وصل عدد الإجابات إلى 3 أو أكثر، أعد: {"question": "", "isComplete": true}
                - إذا كان الوصف واضحاً بما يكفي، أعد: {"question": "", "isComplete": true}
                - لا تكرر سؤالاً سبق الإجابة عليه
                - إذا احتجت سؤالاً، اسأل سؤالاً واحداً جديداً فقط بالعربية

                رد بـ JSON فقط: {"question": "...", "isComplete": false}
                """;
        }

        // ── Request Refinement (create-request wizard, step 2) ────────────────

        public static string RequestRefineUser(
            string rawDescription,
            IReadOnlyList<string> categories,
            IReadOnlyList<string> answers)
        {
            var answersBlock = answers.Count > 0
                ? string.Join("\n", answers.Select((a, i) => $"إجابة {i + 1}: {a}"))
                : "";

            return $$"""
                [TaskType: RefineRequest]
                أنت مساعد متخصص في خدمات الصيانة المنزلية في مصر.
                وصف العميل الأصلي: "{{rawDescription}}"
                {{answersBlock}}
                الفئات المتاحة (id:name): {{string.Join("، ", categories)}}

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
        }

        // ── Category Verification (post-creation sanity check) ────────────────

        public static string CategoryVerificationUser(
            string title,
            string description,
            int customerChosenCategoryId,
            IReadOnlyList<string> categories)
        {
            return $$"""
                [TaskType: CategoryVerification]
                أنت مساعد متخصص في تصنيف طلبات الصيانة المنزلية في مصر.

                الطلب:
                العنوان: "{{title}}"
                الوصف: "{{description}}"

                الفئة التي اختارها العميل: {{customerChosenCategoryId}}

                الفئات المتاحة (id:الاسم):
                {{string.Join("، ", categories)}}

                المطلوب:
                - حدد الفئة الأنسب لهذا الطلب من القائمة أعلاه.
                - إذا كانت فئة العميل صحيحة، أعد نفس الرقم.
                - أعد رقم الـ id فقط بدون أي نص إضافي.

                رد بـ JSON فقط:
                {"suggestedCategoryId": <رقم صحيح>}
                """;
        }

        // ── Technician Re-rank (LLM relevance scoring) ─────────────────────────

        public static string TechnicianRerankUser(string requestText, IReadOnlyList<string> technicianTexts)
        {
            var numbered = string.Join("\n", technicianTexts.Select((t, i) => $"[{i}] {t}"));

            return $$"""
                [TaskType: TechnicianRerank]
                أنت نظام مطابقة خدمات. قيّم مدى ملاءمة كل فني للطلب التالي.
                أعد مصفوفة JSON فقط بدون أي نص آخر.
                الصيغة: [{"index":0,"score":0.85}, {"index":1,"score":0.42}, ...]
                النتيجة بين 0 (غير مناسب) و 1 (مناسب جداً).

                الطلب:
                {{requestText}}

                الفنيون:
                {{numbered}}
                """;
        }

        // ── Review Summarisation (prompt ready; no service wired up yet) ───────

        public static string ReviewSummaryUser(IEnumerable<string> reviews)
        {
            var joined = string.Join("\n- ", reviews);
            return $$"""
                [TaskType: ReviewSummary]
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

        // ── Dispute Analysis (prompt ready; no service wired up yet) ───────────

        public static string DisputeAnalysisUser(
            string customerClaim,
            string technicianClaim,
            string requestDetails)
        {
            return $$"""
                [TaskType: DisputeAnalysis]
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