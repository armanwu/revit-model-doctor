using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to evaluate native Revit model warnings grouped by native warning message descriptions.
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
                        RuleName = "No Model Warnings",
                        Category = "Revit Warnings",
                        Description = "No warnings present in the model.",
                        Status = HealthStatus.Pass,
                        Count = 0,
                        OffendingElements = new List<OffendingElementInfo>()
                    }
                };
            }

            // Group native warnings by exact description text
            var groupedWarnings = new Dictionary<string, List<OffendingElementInfo>>();

            foreach (var w in warnings)
            {
                string warningText = w.GetDescriptionText();
                if (string.IsNullOrWhiteSpace(warningText))
                {
                    warningText = "General Revit Warning";
                }

                if (!groupedWarnings.TryGetValue(warningText, out var list))
                {
                    list = new List<OffendingElementInfo>();
                    groupedWarnings[warningText] = list;
                }

                var failingIds = w.GetFailingElements();
                if (failingIds != null && failingIds.Count > 0)
                {
                    foreach (var id in failingIds)
                    {
                        list.Add(new OffendingElementInfo
                        {
                            ElementId = id,
                            IssueDescription = warningText
                        });
                    }
                }
                else
                {
                    list.Add(new OffendingElementInfo
                    {
                        ElementId = ElementId.InvalidElementId,
                        IssueDescription = warningText
                    });
                }
            }

            var results = new List<HealthRuleResult>();

            foreach (var kvp in groupedWarnings)
            {
                string fullDescription = kvp.Key;
                string shortTitle = ShortenWarningTitle(fullDescription);
                int count = kvp.Value.Count;
                HealthStatus status = count < 15 ? HealthStatus.Warning : HealthStatus.Fail;

                results.Add(new HealthRuleResult
                {
                    RuleName = shortTitle,
                    Category = "Revit Warnings",
                    Description = fullDescription,
                    Status = status,
                    Count = count,
                    OffendingElements = kvp.Value
                });
            }

            return results;
        }

        private static string ShortenWarningTitle(string fullText)
        {
            if (string.IsNullOrWhiteSpace(fullText))
                return "General Revit Warning";

            string text = fullText.Trim();

            // Extract first sentence if period exists
            int periodIdx = text.IndexOf('.');
            if (periodIdx > 10)
            {
                text = text.Substring(0, periodIdx).Trim();
            }

            // Truncate cleanly at word boundary if > 60 chars
            if (text.Length > 60)
            {
                int spaceIdx = text.LastIndexOf(' ', 57);
                if (spaceIdx > 15)
                {
                    text = text.Substring(0, spaceIdx) + "...";
                }
                else
                {
                    text = text.Substring(0, 57) + "...";
                }
            }

            return text;
        }
    }
}
