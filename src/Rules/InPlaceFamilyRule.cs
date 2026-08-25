using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect In-Place Family instances in the model.
    /// Industry Thresholds: Pass &lt;= 2, Warning 3-10, Fail &gt; 10.
    /// </summary>
    public class InPlaceFamilyRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "In-Place Families";

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
                    IssueDescription = $"In-Place Family: '{inst.Name}' (Category: {inst.Category?.Name ?? "General"}, ID: {inst.Id.Value}). In-place families degrade graphics performance and increase file size."
                })
                .ToList();

            int count = offendingList.Count;

            HealthStatus EvaluateStatus(int cnt)
            {
                if (cnt <= 2) return HealthStatus.Pass;
                if (cnt <= 10) return HealthStatus.Warning;
                return HealthStatus.Fail;
            }

            HealthStatus status = EvaluateStatus(count);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Model Performance",
                    Description = count == 0
                        ? "No In-Place family instances found in the model."
                        : $"Found {count} In-Place family instance(s). Industry Standard: Pass <= 2, Warning 3-10, Fail > 10.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
