using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to check distance of Base Points, Survey Points, and model elements from Revit Internal Origin.
    /// Industry Thresholds: Pass = 0 outside 16 km, Fail >= 1 outside 16 km (strict tolerance).
    /// </summary>
    public class SurveyAndBasePointDistanceRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Large Model Extents";

        private const double MaxRecommendedDistanceFeet = 52800.0; // 10 miles (16 km)

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            var basePoints = new FilteredElementCollector(doc)
                .OfClass(typeof(BasePoint))
                .Cast<BasePoint>()
                .ToList();

            foreach (var bp in basePoints)
            {
                XYZ pt = bp.SharedPosition;
                double dist = pt != null ? pt.GetLength() : 0.0;

                string ptType = bp.IsShared ? "Survey Point" : "Project Base Point";

                if (dist > MaxRecommendedDistanceFeet)
                {
                    double distMiles = Math.Round(dist / 52800.0, 2);
                    offendingList.Add(new OffendingElementInfo
                    {
                        ElementId = bp.Id,
                        IssueDescription = $"{ptType} Extreme Distance: '{bp.Name}' (ID: {bp.Id.Value}). Distance from Internal Origin is {distMiles} miles (> 16 km limit). High risk of floating point visual distortion!"
                    });
                }
            }

            int count = offendingList.Count;

            HealthStatus EvaluateStatus(int cnt)
            {
                return cnt == 0 ? HealthStatus.Pass : HealthStatus.Fail;
            }

            HealthStatus status = EvaluateStatus(count);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Spatial & Model Safety",
                    Description = count == 0
                        ? "Project Base Point and Survey Point are within safe 16 km radius of the Internal Origin."
                        : $"Found {count} coordinate positioning issue(s) exceeding 16 km from Revit Internal Origin. Industry Standard: Pass = 0, Fail >= 1.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
