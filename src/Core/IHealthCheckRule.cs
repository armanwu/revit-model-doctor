using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Contract for all modular model health check rules.
    /// </summary>
    public interface IHealthCheckRule
    {
        /// <summary>
        /// Gets the human-readable name of the health check rule.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Executes the rule against the specified Revit Document.
        /// </summary>
        /// <param name="doc">The active Revit document to audit.</param>
        /// <returns>A collection of <see cref="HealthRuleResult"/> containing status and offending element details.</returns>
        IEnumerable<HealthRuleResult> Execute(Document doc);
    }
}
