using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect unused Schedule definitions and unplaced Legends in the model.
    /// Industry Thresholds: Pass = 0, Warning = 1-5, Fail &gt; 5.
    /// </summary>
    public class UnusedSchedulesAndLegendsRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Unused Schedules & Legends";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            var placedScheduleIds = new HashSet<ElementId>(
                new FilteredElementCollector(doc)
                    .OfClass(typeof(ScheduleSheetInstance))
                    .Cast<ScheduleSheetInstance>()
                    .Select(ssi => ssi.ScheduleId)
            );

            var placedViewIds = new HashSet<ElementId>(
                new FilteredElementCollector(doc)
                    .OfClass(typeof(Viewport))
                    .Cast<Viewport>()
                    .Select(vp => vp.ViewId)
            );

            var allSchedules = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .Where(vs => !vs.IsTemplate && !vs.IsTitleblockRevisionSchedule && vs.ViewType == ViewType.Schedule)
                .ToList();

            var unplacedSchedules = allSchedules.Where(s => !placedScheduleIds.Contains(s.Id)).ToList();
            foreach (var s in unplacedSchedules)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = s.Id,
                    IssueDescription = $"Unplaced Schedule: '{s.Name}' (ID: {s.Id.Value}). Schedule is not placed on any drawing sheet."
                });
            }

            var allLegends = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.ViewType == ViewType.Legend)
                .ToList();

            var unplacedLegends = allLegends.Where(l => !placedViewIds.Contains(l.Id)).ToList();
            foreach (var l in unplacedLegends)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = l.Id,
                    IssueDescription = $"Unplaced Legend: '{l.Name}' (ID: {l.Id.Value}). Legend view is not placed on any drawing sheet."
                });
            }

            int count = offendingList.Count;

            HealthStatus EvaluateStatus(int cnt)
            {
                if (cnt == 0) return HealthStatus.Pass;
                if (cnt <= 5) return HealthStatus.Warning;
                return HealthStatus.Fail;
            }

            HealthStatus status = EvaluateStatus(count);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Data & Deliverable Integrity",
                    Description = count == 0
                        ? "All View Schedules and Legends are placed on drawing sheets."
                        : $"Found {unplacedSchedules.Count} unplaced schedule(s) and {unplacedLegends.Count} unplaced legend(s). Industry Standard: Pass = 0, Warning = 1-5, Fail > 5.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
