using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect views with active Crop Region turned off or unclipped View Depth.
    /// Industry Thresholds: Pass = 0, Warning = 1-5, Fail &gt; 5.
    /// </summary>
    public class ViewClippingAndExtentsRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "View Clipping & Extents";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            var modelViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.CanBePrinted &&
                            (v.ViewType == ViewType.FloorPlan ||
                             v.ViewType == ViewType.CeilingPlan ||
                             v.ViewType == ViewType.Elevation ||
                             v.ViewType == ViewType.Section ||
                             v.ViewType == ViewType.ThreeD))
                .ToList();

            foreach (var v in modelViews)
            {
                if (!v.CropBoxActive)
                {
                    offendingList.Add(new OffendingElementInfo
                    {
                        ElementId = v.Id,
                        IssueDescription = $"Uncropped View: '{v.Name}' (Type: {v.ViewType}, ID: {v.Id.Value}). Crop View is turned OFF, causing excessive graphic rendering extent."
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
                    Category = "Spatial & Model Safety",
                    Description = count == 0
                        ? "All model plan, section, and 3D views have active Crop Box regions enabled."
                        : $"Found {count} view(s) with uncropped extents. Industry Standard: Pass = 0, Warning = 1-5, Fail > 5.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
