using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect duplicate elements residing at the exact same spatial location with identical types.
    /// Industry Thresholds: Pass = 0, Warning = 1-5, Fail &gt; 5.
    /// </summary>
    public class DuplicateElementsRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Duplicate Instances";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            var physicalElements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .Where(e => e.Category != null &&
                            e.Category.CategoryType == CategoryType.Model &&
                            e.Location != null &&
                            e.GetTypeId() != ElementId.InvalidElementId)
                .ToList();

            var spatialGroups = new Dictionary<string, List<Element>>();

            foreach (var elem in physicalElements)
            {
                XYZ? pos = GetElementLocationPoint(elem);
                if (pos == null) continue;

                string key = $"{elem.Category.Id.Value}_{elem.GetTypeId().Value}_{Math.Round(pos.X, 3)}_{Math.Round(pos.Y, 3)}_{Math.Round(pos.Z, 3)}";

                if (!spatialGroups.TryGetValue(key, out var list))
                {
                    list = new List<Element>();
                    spatialGroups[key] = list;
                }
                list.Add(elem);
            }

            foreach (var kvp in spatialGroups)
            {
                if (kvp.Value.Count > 1)
                {
                    foreach (var dup in kvp.Value)
                    {
                        offendingList.Add(new OffendingElementInfo
                        {
                            ElementId = dup.Id,
                            IssueDescription = $"Duplicate Element: '{dup.Name}' (Category: {dup.Category?.Name}, ID: {dup.Id.Value}). Overlapping exact same geometry location."
                        });
                    }
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
                        ? "No duplicate overlapping elements detected at identical locations."
                        : $"Found {count} duplicate element instance(s) overlapping at identical coordinates. Industry Standard: Pass = 0, Warning = 1-5, Fail > 5.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }

        private static XYZ? GetElementLocationPoint(Element e)
        {
            if (e.Location is LocationPoint lp)
                return lp.Point;

            if (e.Location is LocationCurve lc)
                return lc.Curve.Evaluate(0.5, true);

            BoundingBoxXYZ box = e.get_BoundingBox(null);
            if (box != null)
                return (box.Min + box.Max) * 0.5;

            return null;
        }
    }
}
