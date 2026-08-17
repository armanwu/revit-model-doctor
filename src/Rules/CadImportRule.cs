using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect directly imported CAD files in the Revit model (which should ideally be linked).
    /// </summary>
    public class CadImportRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Direct CAD Imports Check";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            // Collect all ImportInstance elements that are imported directly (not linked)
            var unlinkedCadImports = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>()
                .Where(inst => !inst.IsLinked)
                .ToList();

            var offendingList = unlinkedCadImports
                .Select(inst => new OffendingElementInfo
                {
                    ElementId = inst.Id,
                    IssueDescription = $"Directly Imported CAD Drawing: '{inst.Name}' (ID: {inst.Id.Value}). Importing CAD files directly increases model file size, pollutes line styles/layers, and degrades viewport performance. Recommend deleting and linking instead."
                })
                .ToList();

            int count = offendingList.Count;
            HealthStatus status = count == 0 ? HealthStatus.Pass : HealthStatus.Warning;

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Imports & Links",
                    Description = count == 0
                        ? "No directly imported CAD files found in the model."
                        : $"Found {count} directly imported CAD file(s). Select an Element ID from the list to view its specific details.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList
                }
            };
        }
    }
}
