using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect directly imported CAD files in the model.
    /// Industry Thresholds: Pass = 0, Warning = 1-2 (2D view-specific only), Fail &gt; 2 or any CAD in 3D Model Layer.
    /// </summary>
    public class CadImportRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Imported CAD Files";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var allImports = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>()
                .ToList();

            var unlinkedCadImports = allImports.Where(inst => !inst.IsLinked).ToList();

            var offendingList = new List<OffendingElementInfo>();

            foreach (var inst in unlinkedCadImports)
            {
                string placementType = inst.ViewSpecific ? "2D View-Specific" : "3D Model Layer";
                string warningDetail = inst.ViewSpecific
                    ? "Directly imported CAD in a 2D View. (Recommend linking instead)."
                    : "Directly imported CAD in 3D Model Layer! (CRITICAL - degrades graphics performance).";

                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = inst.Id,
                    IssueDescription = $"Imported CAD ('{inst.Name}', ID: {inst.Id.Value}) [{placementType}]: {warningDetail}"
                });
            }

            HealthStatus EvaluateStatus(IEnumerable<OffendingElementInfo> elements)
            {
                int cnt = elements.Count();
                if (cnt == 0) return HealthStatus.Pass;
                bool hasModelLayer = unlinkedCadImports.Any(i => !i.ViewSpecific && elements.Any(e => e.ElementId == i.Id));
                if (cnt <= 2 && !hasModelLayer) return HealthStatus.Warning;
                return HealthStatus.Fail;
            }

            int count = offendingList.Count;
            HealthStatus status = EvaluateStatus(offendingList);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Model Performance",
                    Description = count == 0
                        ? "No directly imported CAD files found in the model."
                        : $"Found {count} directly imported CAD file(s). Industry Standard: Pass = 0, Warning = 1-2 (2D View only), Fail > 2 or any in 3D Model Layer.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = EvaluateStatus
                }
            };
        }
    }
}
