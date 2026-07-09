namespace Sala7ly.BLL.DTOs.AiDTOs
{
    /// <summary>
    /// Returned by ISmartMatchingService after AI-driven category resolution
    /// and technician notification dispatch.
    /// </summary>
    public class SmartMatchingResultDto
    {
        /// <summary>The category ID that was actually used for matching.</summary>
        public int ResolvedCategoryId { get; set; }

        /// <summary>
        /// True when the AI suggested a different category than the customer picked
        /// and the override was applied.
        /// </summary>
        public bool CategoryWasOverridden { get; set; }

        /// <summary>
        /// The original CategoryId sent by the customer (before any AI override).
        /// </summary>
        public int OriginalCategoryId { get; set; }

        /// <summary>
        /// AI-generated one-line summary describing why this category was chosen.
        /// </summary>
        public string? AiMatchReason { get; set; }

        /// <summary>Number of technicians who received the new-request notification.</summary>
        public int NotifiedTechnicianCount { get; set; }
    }
}