using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect unused View Filters and unapplied View Templates in the document.
    /// Industry Thresholds: Pass &lt;= 5, Warning 6-20, Fail &gt; 20.
    /// </summary>
    public class UnusedViewFiltersAndTemplatesRule : IHealthCheckRule, IQuickFixableRule
    {
        /// <inheritdoc />
        public string Name => "Unused View Filters & Templates";

        /// <inheritdoc />
        public string QuickFixDescription => "Purges unused View Filters and unassigned View Templates from the document.";

        /// <inheritdoc />
        public int ExecuteQuickFix(Document doc, IEnumerable<OffendingElementInfo> offendingElements)
        {
            ArgumentNullException.ThrowIfNull(doc);
            ArgumentNullException.ThrowIfNull(offendingElements);

            int count = 0;
            foreach (var item in offendingElements)
            {
                if (item.ElementId != null && item.ElementId != ElementId.InvalidElementId)
                {
                    try
                    {
                        doc.Delete(item.ElementId);
                        count++;
                    }
                    catch
                    {
                        // Skip if element cannot be deleted
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

            var allViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .ToList();

            var appliedTemplateIds = new HashSet<ElementId>(
                allViews.Where(v => v.ViewTemplateId != ElementId.InvalidElementId).Select(v => v.ViewTemplateId)
            );

            var unusedTemplates = allViews.Where(v => v.IsTemplate && !appliedTemplateIds.Contains(v.Id)).ToList();
            foreach (var t in unusedTemplates)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = t.Id,
                    IssueDescription = $"Unused View Template: '{t.Name}' (ID: {t.Id.Value}). Template is not assigned to any view."
                });
            }

            var allFilterIds = new FilteredElementCollector(doc)
                .OfClass(typeof(FilterElement))
                .Select(f => f.Id)
                .ToList();

            var usedFilterIds = new HashSet<ElementId>();
            foreach (var v in allViews)
            {
                try
                {
                    ICollection<ElementId> filters = v.GetFilters();
                    if (filters != null)
                    {
                        foreach (var fid in filters)
                        {
                            usedFilterIds.Add(fid);
                        }
                    }
                }
                catch { }
            }

            var unusedFilterIds = allFilterIds.Where(fid => !usedFilterIds.Contains(fid)).ToList();
            foreach (var fid in unusedFilterIds)
            {
                Element f = doc.GetElement(fid);
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = fid,
                    IssueDescription = $"Unused View Filter: '{f?.Name ?? "Filter"}' (ID: {fid.Value}). Filter is not applied to any view or template."
                });
            }

            int count = offendingList.Count;

            HealthStatus EvaluateStatus(int cnt)
            {
                if (cnt <= 5) return HealthStatus.Pass;
                if (cnt <= 20) return HealthStatus.Warning;
                return HealthStatus.Fail;
            }

            HealthStatus status = EvaluateStatus(count);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Model Performance",
                    Description = count == 0
                        ? "All View Filters and View Templates are actively assigned and in use."
                        : $"Found {count} unused View Filter(s) or unapplied View Template(s). Industry Standard: Pass <= 5, Warning 6-20, Fail > 20.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count()),
                    QuickFixRule = this,
                    RuleSource = this
                }
            };
        }
    }
}
