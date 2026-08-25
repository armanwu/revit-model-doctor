using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to inspect critical elements (Grids, Levels, Links) placed on default user worksets.
    /// Industry Thresholds: Pass = 0, Warning = 1-5, Fail &gt; 5.
    /// </summary>
    public class WorksetAllocationRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Workset Allocation";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            if (!doc.IsWorkshared)
            {
                return new[]
                {
                    new HealthRuleResult
                    {
                        RuleName = Name,
                        Category = "Data & Deliverable Integrity",
                        Description = "Document is non-workshared (Standalone .rvt). Workset allocation check skipped.",
                        Status = HealthStatus.Pass,
                        Count = 0,
                        OffendingElements = offendingList
                    }
                };
            }

            var criticalElements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementMulticlassFilter(new[] { typeof(Grid), typeof(Level), typeof(RevitLinkInstance) }))
                .ToList();

            WorksetTable worktable = doc.GetWorksetTable();

            foreach (var elem in criticalElements)
            {
                WorksetId wsId = elem.WorksetId;
                Workset ws = worktable.GetWorkset(wsId);

                if (ws != null && (ws.Name.Equals("Workset1", StringComparison.OrdinalIgnoreCase) || ws.Kind == WorksetKind.UserWorkset && ws.Name.StartsWith("User", StringComparison.OrdinalIgnoreCase)))
                {
                    string elemType = elem is Grid ? "Grid" : (elem is Level ? "Level" : "Revit Link");
                    offendingList.Add(new OffendingElementInfo
                    {
                        ElementId = elem.Id,
                        IssueDescription = $"Workset Misallocation: {elemType} '{elem.Name}' (ID: {elem.Id.Value}) is on default '{ws.Name}'. Should be allocated to dedicated Shared Levels/Grids workset."
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
                        ? "All critical datum and link elements are allocated to dedicated worksets."
                        : $"Found {count} critical element(s) placed on default user worksets. Industry Standard: Pass = 0, Warning = 1-5, Fail > 5.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
