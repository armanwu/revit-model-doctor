using System.Collections.Generic;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Represents the result output generated after running a health check rule.
    /// </summary>
    public class HealthRuleResult
    {
        /// <summary>
        /// The display name of the rule evaluated.
        /// </summary>
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// The category of the health rule (e.g., "Imports & Links", "Model Integrity").
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Detailed summary description of the health evaluation rule.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Overall health status (Pass, Warning, or Fail).
        /// </summary>
        public HealthStatus Status { get; set; } = HealthStatus.Pass;

        /// <summary>
        /// Quantitative count of offending elements or issues detected.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Collection of offending elements along with their specific error/warning details.
        /// </summary>
        public ICollection<OffendingElementInfo> OffendingElements { get; set; } = new List<OffendingElementInfo>();
    }
}
