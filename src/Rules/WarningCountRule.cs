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
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            // Retrieve all warning messages from the document
            IList<FailureMessage> warnings = doc.GetWarnings();

            if (warnings.Count == 0)
            {
                return new[]
                {
                    new HealthRuleResult
                    {
                        RuleName = "Model Warnings Check",
                        Category = "Warnings - Integrity",
                        Description = "No warnings present in the model.",
                        Status = HealthStatus.Pass,
                        Count = 0,
                        OffendingElements = new List<OffendingElementInfo>()
                    }
                };
            }

            // Temporary storage grouped by (Category, RuleName)
            var groupedResults = new Dictionary<(string Category, string RuleName), List<OffendingElementInfo>>();

            foreach (var w in warnings)
            {
                string warningText = w.GetDescriptionText();
                var (category, ruleName) = WarningClassifier.Classify(warningText);

                var key = (category, ruleName);
                if (!groupedResults.TryGetValue(key, out var list))
                {
                    list = new List<OffendingElementInfo>();
                    groupedResults[key] = list;
                }

                var failingIds = w.GetFailingElements();
                if (failingIds != null && failingIds.Count > 0)
                {
                    foreach (var id in failingIds)
                    {
                        list.Add(new OffendingElementInfo
                        {
                            ElementId = id,
                            IssueDescription = string.IsNullOrWhiteSpace(warningText)
                                ? $"Revit Warning affecting Element ID: {id.Value}"
                                : warningText
                        });
                    }
                }
                else
                {
                    list.Add(new OffendingElementInfo
                    {
                        ElementId = ElementId.InvalidElementId,
                        IssueDescription = string.IsNullOrWhiteSpace(warningText)
                            ? "General Revit Warning"
                            : warningText
                    });
                }
            }

            var results = new List<HealthRuleResult>();

            foreach (var kvp in groupedResults)
            {
                int count = kvp.Value.Count;
                HealthStatus status = count < 20 ? HealthStatus.Warning : HealthStatus.Fail;

                results.Add(new HealthRuleResult
                {
                    RuleName = kvp.Key.RuleName,
                    Category = kvp.Key.Category,
                    Description = $"Category contains {count} warning item(s) affecting model elements. Select an Element ID to view details or locate in Revit.",
                    Status = status,
                    Count = count,
                    OffendingElements = kvp.Value
                });
            }

            return results;
        }
    }
}
