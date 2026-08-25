using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect Unplaced, Not Enclosed, or Redundant (Zero Area) Rooms and Spaces.
    /// Industry Thresholds: Pass = 0, Warning = 1-5, Fail &gt; 5.
    /// </summary>
    public class UnboundedRoomsAndSpacesRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Unplaced & Unenclosed Rooms";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            var spatialElements = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .Cast<SpatialElement>()
                .ToList();

            foreach (var elem in spatialElements)
            {
                string elemType = elem is Autodesk.Revit.DB.Architecture.Room ? "Room" : "Space";

                if (elem.Location == null)
                {
                    offendingList.Add(new OffendingElementInfo
                    {
                        ElementId = elem.Id,
                        IssueDescription = $"Unplaced {elemType}: '{elem.Name}' (ID: {elem.Id.Value}). Element exists in project database but is unplaced in model space."
                    });
                }
                else if (elem.Area <= 0.0001)
                {
                    offendingList.Add(new OffendingElementInfo
                    {
                        ElementId = elem.Id,
                        IssueDescription = $"Not Enclosed / Redundant {elemType}: '{elem.Name}' (ID: {elem.Id.Value}). Area is 0 sq ft / m². Check boundary enclosure elements."
                    });
                }
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
                        ? "All Rooms and Spaces are properly placed and fully enclosed."
                        : $"Found {count} unplaced, not enclosed, or zero-area Room/Space element(s). Industry Standard: Pass = 0, Warning = 1-5, Fail > 5.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
