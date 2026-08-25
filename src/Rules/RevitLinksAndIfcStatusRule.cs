using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to inspect Revit/IFC Link status (Loaded, Unloaded, Not Found) and attachment type (Overlay vs Attachment).
    /// Industry Thresholds: Pass = 0 issues, Warning = 1 Unloaded, Fail >= 1 Not Found / Unresolved.
    /// </summary>
    public class RevitLinksAndIfcStatusRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Broken Links & IFC Status";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var offendingList = new List<OffendingElementInfo>();

            var linkTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkType))
                .Cast<RevitLinkType>()
                .ToList();

            foreach (var lt in linkTypes)
            {
                ExternalFileReference extRef = lt.GetExternalFileReference();
                LinkedFileStatus status = extRef != null ? extRef.GetLinkedFileStatus() : LinkedFileStatus.NotFound;
                AttachmentType attachType = lt.AttachmentType;

                if (status == LinkedFileStatus.NotFound)
                {
                    offendingList.Add(new OffendingElementInfo
                    {
                        ElementId = lt.Id,
                        IssueDescription = $"Broken Link (Not Found): '{lt.Name}' (ID: {lt.Id.Value}). File reference missing from path!"
                    });
                }
                else if (status == LinkedFileStatus.Unloaded)
                {
                    offendingList.Add(new OffendingElementInfo
                    {
                        ElementId = lt.Id,
                        IssueDescription = $"Unloaded Link: '{lt.Name}' (ID: {lt.Id.Value}). Link is currently unloaded."
                    });
                }

                if (attachType == AttachmentType.Attachment)
                {
                    offendingList.Add(new OffendingElementInfo
                    {
                        ElementId = lt.Id,
                        IssueDescription = $"Link Attachment Type = Attachment: '{lt.Name}' (ID: {lt.Id.Value}). Best practice: Use 'Overlay' to prevent circular nesting."
                    });
                }
            }

            HealthStatus EvaluateStatus(IEnumerable<OffendingElementInfo> elements)
            {
                int cnt = elements.Count();
                if (cnt == 0) return HealthStatus.Pass;
                bool hasNotFound = elements.Any(e => e.IssueDescription.Contains("Not Found"));
                int unloadedCount = elements.Count(e => e.IssueDescription.Contains("Unloaded"));
                if (hasNotFound || unloadedCount > 1) return HealthStatus.Fail;
                return HealthStatus.Warning;
            }

            int count = offendingList.Count;
            HealthStatus ruleStatus = EvaluateStatus(offendingList);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Data & Deliverable Integrity",
                    Description = count == 0
                        ? "All Revit and IFC links are properly loaded with Overlay attachment type."
                        : $"Found {count} link issue(s). Industry Standard: Pass = 0, Warning = 1 Unloaded, Fail >= 1 Not Found / Unresolved.",
                    Status = ruleStatus,
                    Count = count,
                    OffendingElements = offendingList,
                    StatusEvaluator = EvaluateStatus
                }
            };
        }
    }
}
