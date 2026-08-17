using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect In-Place Family instances in the model.
    /// </summary>
    public class InPlaceFamilyRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "In-Place Families Check";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var inPlaceInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(inst => inst.Symbol?.Family?.IsInPlace == true)
                .ToList();

            var offendingList = inPlaceInstances
                .Select(inst => new OffendingElementInfo
                {
                    ElementId = inst.Id,
                    IssueDescription = $"In-Place Family: '{inst.Name}' (Category: {inst.Category?.Name ?? "General"}, ID: {inst.Id.Value}). In-place families increase model file size, degrade performance, and bypass standard family library management. Recommend replacing with loadable families."
                })
                .ToList();

            int count = offendingList.Count;
            HealthStatus status = count == 0 ? HealthStatus.Pass : (count < 5 ? HealthStatus.Warning : HealthStatus.Fail);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Model Hygiene",
                    Description = count == 0
                        ? "No In-Place families found in the model."
                        : $"Found {count} In-Place family instance(s). Select an Element ID to view details or locate in Revit.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList
                }
            };
        }
    }
}
