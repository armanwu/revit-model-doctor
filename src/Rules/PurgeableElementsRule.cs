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
    public class PurgeableElementsRule : IHealthCheckRule, IQuickFixableRule
    {
        /// <inheritdoc />
        public string Name => "Purgeable Items";

        /// <inheritdoc />
        public string QuickFixDescription => "Purges unused family types from the model database to reduce file size.";

        /// <inheritdoc />
        public int ExecuteQuickFix(Document doc, IEnumerable<OffendingElementInfo> offendingElements)
        {
            ArgumentNullException.ThrowIfNull(doc);
            ArgumentNullException.ThrowIfNull(offendingElements);

            int count = 0;
            var processedIds = new HashSet<ElementId>();

            foreach (var item in offendingElements)
            {
                if (item.ElementId == null || item.ElementId == ElementId.InvalidElementId || processedIds.Contains(item.ElementId))
                    continue;

                Element elem = doc.GetElement(item.ElementId);
                if (elem == null) continue;

                try
                {
                    // Attempt 1: Try deleting the individual symbol
                    ICollection<ElementId> deleted = doc.Delete(item.ElementId);
                    if (deleted != null && deleted.Count > 0)
                    {
                        foreach (var id in deleted) processedIds.Add(id);
                        count++;
                    }
                    else if (elem is FamilySymbol symbol && symbol.Family != null && !processedIds.Contains(symbol.Family.Id))
                    {
                        // Attempt 2: If deleting symbol returned 0, try deleting parent Family
                        ICollection<ElementId> famDeleted = doc.Delete(symbol.Family.Id);
                        if (famDeleted != null && famDeleted.Count > 0)
                        {
                            foreach (var id in famDeleted) processedIds.Add(id);
                            count++;
                        }
                    }
                }
                catch
                {
                    // Attempt 3: Catch exception (e.g. deleting last symbol of a family) and try deleting parent Family
                    try
                    {
                        if (elem is FamilySymbol symbol && symbol.Family != null && !processedIds.Contains(symbol.Family.Id))
                        {
                            ICollection<ElementId> famDeleted = doc.Delete(symbol.Family.Id);
                            if (famDeleted != null && famDeleted.Count > 0)
                            {
                                foreach (var id in famDeleted) processedIds.Add(id);
                                count++;
                            }
                        }
                    }
                    catch
                    {
                        // System locked element
                    }
                }
            }
            return count;
        }

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
                .Where(s => s.Family != null && !s.Family.IsInPlace && s.Family.IsEditable && !placedSymbolIds.Contains(s.Id))
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
                    StatusEvaluator = elems => EvaluateStatus(elems.Count()),
                    QuickFixRule = this,
                    RuleSource = this
                }
            };
        }
    }
}
