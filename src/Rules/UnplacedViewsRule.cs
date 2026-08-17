using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect printable model views that are not placed on any sheet.
    /// </summary>
    public class UnplacedViewsRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Unplaced Model Views";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            // Collect all viewports to get placed View IDs
            var placedViewIds = new HashSet<ElementId>(
                new FilteredElementCollector(doc)
                    .OfClass(typeof(Viewport))
                    .Cast<Viewport>()
                    .Select(vp => vp.ViewId)
            );

            // Also include schedule sheet instances
            var placedScheduleIds = new FilteredElementCollector(doc)
                .OfClass(typeof(ScheduleSheetInstance))
                .Cast<ScheduleSheetInstance>()
                .Select(ssi => ssi.ScheduleId);

            foreach (var id in placedScheduleIds)
            {
                placedViewIds.Add(id);
            }

            // Collect printable model views
            var unplacedViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.CanBePrinted &&
                            v.ViewType != ViewType.Internal &&
                            v.ViewType != ViewType.ProjectBrowser &&
                            v.ViewType != ViewType.SystemBrowser &&
                            v.ViewType != ViewType.DrawingSheet &&
                            !placedViewIds.Contains(v.Id))
                .ToList();

            var offendingList = unplacedViews
                .Select(v => new OffendingElementInfo
                {
                    ElementId = v.Id,
                    IssueDescription = $"Unplaced View: '{v.Name}' (Type: {v.ViewType}, ID: {v.Id.Value}). This view is not placed on any drawing sheet. Unplaced views clutter the Project Browser and increase file size."
                })
                .ToList();

            int count = offendingList.Count;
            HealthStatus status = count == 0 ? HealthStatus.Pass : (count < 15 ? HealthStatus.Warning : HealthStatus.Fail);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Views & Sheets",
                    Description = count == 0
                        ? "All printable model views are placed on drawing sheets."
                        : $"Found {count} unplaced model view(s). Select an Element ID to view details or locate in Revit.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList
                }
            };
        }
    }
}
