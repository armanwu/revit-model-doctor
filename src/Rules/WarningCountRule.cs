using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to inspect active Revit warnings count in the document.
    /// Industry Thresholds: Pass &lt;= 50, Warning 51-200, Fail &gt; 200.
    /// </summary>
    public class WarningCountRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Active Warnings";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            IList<FailureMessage> warnings = doc.GetWarnings();
            var offendingList = new List<OffendingElementInfo>();

            foreach (var warning in warnings)
            {
                string desc = warning.GetDescriptionText();
                var failingIds = warning.GetFailingElements();

                if (failingIds != null && failingIds.Count > 0)
                {
                    foreach (var id in failingIds)
                    {
                        offendingList.Add(new OffendingElementInfo
                        {
                            ElementId = id,
                            IssueDescription = $"Warning: '{desc}' (Failing Element ID: {id.Value})"
                        });
                    }
                }
            }

            int warningCount = warnings.Count;

            HealthStatus EvaluateStatus(int count)
            {
                if (count <= 50) return HealthStatus.Pass;
                if (count <= 200) return HealthStatus.Warning;
                return HealthStatus.Fail;
            }

            HealthStatus status = EvaluateStatus(warningCount);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Model Performance",
                    Description = warningCount == 0
                        ? "No warnings present in the model."
                        : $"Found {warningCount} active warning(s). Industry Standard: Pass <= 50, Warning 51-200, Fail > 200.",
                    Status = status,
                    Count = warningCount,
                    OffendingElements = offendingList,
                    StatusEvaluator = _ => EvaluateStatus(warnings.Count)
                }
            };
        }
    }
}
