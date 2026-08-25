using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect printable working model views that are not placed on any drawing sheet.
    /// Industry Thresholds: Pass &lt; 20% of total views, Warning 20%-40%, Fail &gt; 40%.
    /// </summary>
    public class UnplacedViewsRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Views Not on Sheets";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var placedViewIds = new HashSet<ElementId>(
                new FilteredElementCollector(doc)
                    .OfClass(typeof(Viewport))
                    .Cast<Viewport>()
                    .Select(vp => vp.ViewId)
            );

            var placedScheduleIds = new FilteredElementCollector(doc)
                .OfClass(typeof(ScheduleSheetInstance))
                .Cast<ScheduleSheetInstance>()
                .Select(ssi => ssi.ScheduleId);

            foreach (var id in placedScheduleIds)
            {
                placedViewIds.Add(id);
            }

            var allPrintableViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.CanBePrinted &&
                            v.ViewType != ViewType.Internal &&
                            v.ViewType != ViewType.ProjectBrowser &&
                            v.ViewType != ViewType.SystemBrowser &&
                            v.ViewType != ViewType.DrawingSheet &&
                            v.ViewType != ViewType.Schedule &&
                            v.ViewType != ViewType.Legend)
                .ToList();

            var unplacedViews = allPrintableViews.Where(v => !placedViewIds.Contains(v.Id)).ToList();

            var offendingList = unplacedViews
                .Select(v => new OffendingElementInfo
                {
                    ElementId = v.Id,
                    IssueDescription = $"View Not on Sheet: '{v.Name}' (Type: {v.ViewType}, ID: {v.Id.Value}). Working or temporary view not placed on any sheet."
                })
                .ToList();

            int totalViewCount = allPrintableViews.Count;
            int count = offendingList.Count;

            HealthStatus EvaluateStatus(int unplacedCnt)
            {
                double unplacedPct = totalViewCount > 0 ? ((double)unplacedCnt / totalViewCount) * 100.0 : 0.0;
                if (unplacedPct < 20.0) return HealthStatus.Pass;
                if (unplacedPct <= 40.0) return HealthStatus.Warning;
                return HealthStatus.Fail;
            }

            HealthStatus status = EvaluateStatus(count);
            double unplacedPercentage = totalViewCount > 0 ? ((double)count / totalViewCount) * 100.0 : 0.0;

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Data & Deliverable Integrity",
                    Description = count == 0
                        ? "All printable model views are placed on drawing sheets."
                        : $"Found {count} of {totalViewCount} model view(s) ({Math.Round(unplacedPercentage, 1)}%) not placed on sheets. Industry Standard: Pass < 20%, Warning 20%-40%, Fail > 40%.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
