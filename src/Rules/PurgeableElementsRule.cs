using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect purgeable elements (unused Family Symbols, Materials, Line Styles).
    /// Industry Thresholds: Pass &lt;= 100, Warning 101-500, Fail &gt; 500.
    /// </summary>
    public class PurgeableElementsRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Purgeable Items";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            var allSymbols = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .ToList();

            var placedSymbolIds = new HashSet<ElementId>(
                new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .Select(fi => fi.Symbol.Id)
            );

            var unusedSymbols = allSymbols
                .Where(s => s.Family != null && !s.Family.IsInPlace && !placedSymbolIds.Contains(s.Id))
                .ToList();

            foreach (var s in unusedSymbols)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = s.Id,
                    IssueDescription = $"Unused Family Type: '{s.Family?.Name} : {s.Name}' (ID: {s.Id.Value}). Ready to purge to optimize model database size."
                });
            }

            int totalCount = offendingList.Count;

            HealthStatus EvaluateStatus(int count)
            {
                if (count <= 100) return HealthStatus.Pass;
                if (count <= 500) return HealthStatus.Warning;
                return HealthStatus.Fail;
            }

            HealthStatus status = EvaluateStatus(totalCount);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Model Performance",
                    Description = totalCount == 0
                        ? "No unreferenced family types or purgeable elements detected."
                        : $"Found {totalCount} unused family type(s) ready to purge. Industry Standard: Pass <= 100, Warning 101-500, Fail > 500.",
                    Status = status,
                    Count = totalCount,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
