using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect unpinned datum references (Grids, Levels) and Revit Link instances.
    /// Industry Thresholds: Pass = 100% Pinned, Warning = 90%-99% Pinned, Fail < 90% Pinned.
    /// </summary>
    public class UnpinnedGridsAndLevelsRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Unpinned Grids & Levels";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            var allGrids = new FilteredElementCollector(doc).OfClass(typeof(Grid)).Cast<Grid>().ToList();
            var allLevels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            var allLinks = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();

            int totalDatums = allGrids.Count + allLevels.Count + allLinks.Count;

            var unpinnedGrids = allGrids.Where(e => !e.Pinned).ToList();
            foreach (var g in unpinnedGrids)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = g.Id,
                    IssueDescription = $"Unpinned Grid: '{g.Name}' (ID: {g.Id.Value}). Grid lines should be pinned to prevent displacement."
                });
            }

            var unpinnedLevels = allLevels.Where(e => !e.Pinned).ToList();
            foreach (var l in unpinnedLevels)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = l.Id,
                    IssueDescription = $"Unpinned Level: '{l.Name}' (ID: {l.Id.Value}). Level datums should be pinned to prevent vertical coordinate shifts."
                });
            }

            var unpinnedLinks = allLinks.Where(e => !e.Pinned).ToList();
            foreach (var rlink in unpinnedLinks)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = rlink.Id,
                    IssueDescription = $"Unpinned Revit Link Instance: '{rlink.Name}' (ID: {rlink.Id.Value}). Linked models should be pinned."
                });
            }

            int unpinnedCount = offendingList.Count;

            HealthStatus EvaluateStatus(int unpinnedCnt)
            {
                if (unpinnedCnt == 0) return HealthStatus.Pass;
                double pinnedPct = totalDatums > 0 ? (((double)(totalDatums - unpinnedCnt)) / totalDatums) * 100.0 : 100.0;
                if (pinnedPct >= 90.0) return HealthStatus.Warning;
                return HealthStatus.Fail;
            }

            HealthStatus status = EvaluateStatus(unpinnedCount);
            double pinnedPercentage = totalDatums > 0 ? (((double)(totalDatums - unpinnedCount)) / totalDatums) * 100.0 : 100.0;

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Spatial & Model Safety",
                    Description = unpinnedCount == 0
                        ? "All Grids, Levels, and Revit Links are properly pinned (100% Pinned)."
                        : $"Found {unpinnedCount} of {totalDatums} datum/link element(s) unpinned ({Math.Round(pinnedPercentage, 1)}% Pinned). Industry Standard: Pass = 100% Pinned, Warning = 90%-99%, Fail < 90%.",
                    Status = status,
                    Count = unpinnedCount,
                    OffendingElements = offendingList,
                    StatusEvaluator = elems => EvaluateStatus(elems.Count())
                }
            };
        }
    }
}
