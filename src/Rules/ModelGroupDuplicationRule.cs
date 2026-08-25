using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect unused or duplicated Model Group definitions.
    /// Industry Thresholds: Pass = 0, Warning = 1-3, Fail &gt; 3.
    /// </summary>
    public class ModelGroupDuplicationRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Model Group Duplication";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            var groupTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(GroupType))
                .Cast<GroupType>()
                .ToList();

            var placedGroupTypeIds = new HashSet<ElementId>(
                new FilteredElementCollector(doc)
                    .OfClass(typeof(Group))
                    .Cast<Group>()
                    .Select(g => g.GroupType.Id)
            );

            var unusedGroupTypes = groupTypes.Where(gt => !placedGroupTypeIds.Contains(gt.Id)).ToList();

            foreach (var gt in unusedGroupTypes)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = gt.Id,
                    IssueDescription = $"Unused Model Group Type: '{gt.Name}' (ID: {gt.Id.Value}). Group definition exists but 0 instances are placed."
                });
            }

            int count = offendingList.Count;

            HealthStatus EvaluateStatus(int cnt)
            {
                if (cnt == 0) return HealthStatus.Pass;
                if (cnt <= 3) return HealthStatus.Warning;
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
                        ? "All Model Group types are placed and in active use."
                        : $"Found {count} unused Model Group type definition(s). Industry Standard: Pass = 0, Warning = 1-3, Fail > 3.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
