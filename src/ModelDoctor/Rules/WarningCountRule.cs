using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to evaluate total Revit model warnings and gather element-specific warning descriptions.
    /// </summary>
    public class WarningCountRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Model Warnings Check";

        /// <inheritdoc />
        public HealthRuleResult Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            // Retrieve all warning messages from the document
            IList<FailureMessage> warnings = doc.GetWarnings();
            int warningCount = warnings.Count;

            var offendingList = new List<OffendingElementInfo>();

            foreach (var w in warnings)
            {
                string warningText = w.GetDescriptionText();
                var failingIds = w.GetFailingElements();

                if (failingIds != null && failingIds.Count > 0)
                {
                    foreach (var id in failingIds)
                    {
                        offendingList.Add(new OffendingElementInfo
                        {
                            ElementId = id,
                            IssueDescription = string.IsNullOrWhiteSpace(warningText)
                                ? $"Revit Warning affecting Element ID: {id.Value}"
                                : warningText
                        });
                    }
                }
            }

            HealthStatus status;
            if (warningCount == 0)
            {
                status = HealthStatus.Pass;
            }
            else if (warningCount < 100)
            {
                status = HealthStatus.Warning;
            }
            else
            {
                status = HealthStatus.Fail;
            }

            return new HealthRuleResult
            {
                RuleName = Name,
                Category = "Model Integrity",
                Description = warningCount == 0
                    ? "No warnings present in the model."
                    : $"Model contains {warningCount} warning(s) affecting {offendingList.Count} failing element entry(ies). Click an Element ID to view its specific warning explanation.",
                Status = status,
                Count = warningCount,
                OffendingElements = offendingList
            };
        }
    }
}
