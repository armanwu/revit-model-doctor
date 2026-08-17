using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect unpinned datum references (Grids, Levels) and Revit Link instances.
    /// </summary>
    public class UnpinnedElementsRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Unpinned Datum & Links";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            // 1. Unpinned Grids
            var unpinnedGrids = new FilteredElementCollector(doc)
                .OfClass(typeof(Grid))
                .Where(e => !e.Pinned)
                .ToList();

            foreach (var g in unpinnedGrids)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = g.Id,
                    IssueDescription = $"Unpinned Grid: '{g.Name}' (ID: {g.Id.Value}). Grid lines should be pinned to prevent accidental displacement of building reference axes."
                });
            }

            // 2. Unpinned Levels
            var unpinnedLevels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Where(e => !e.Pinned)
                .ToList();

            foreach (var l in unpinnedLevels)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = l.Id,
                    IssueDescription = $"Unpinned Level: '{l.Name}' (ID: {l.Id.Value}). Level datums should be pinned to prevent accidental vertical coordinate shifts."
                });
            }

            // 3. Unpinned Revit Links
            var unpinnedLinks = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Where(e => !e.Pinned)
                .ToList();

            foreach (var rlink in unpinnedLinks)
            {
                offendingList.Add(new OffendingElementInfo
                {
                    ElementId = rlink.Id,
                    IssueDescription = $"Unpinned Revit Link Instance: '{rlink.Name}' (ID: {rlink.Id.Value}). Linked models should be pinned to maintain shared coordinate positioning."
                });
            }

            int count = offendingList.Count;
            HealthStatus status = count == 0 ? HealthStatus.Pass : HealthStatus.Warning;

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Model Hygiene",
                    Description = count == 0
                        ? "All Grids, Levels, and Revit Links are properly pinned."
                        : $"Found {count} unpinned datum/link element(s). Select an Element ID to view details or locate in Revit.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList
                }
            };
        }
    }
}
