using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModelDoctor.Core;

namespace ModelDoctor.Rules
{
    /// <summary>
    /// Health rule to detect printable model views that do not have a View Template assigned.
    /// </summary>
    public class ViewTemplateRule : IHealthCheckRule
    {
        /// <inheritdoc />
        public string Name => "Views Without View Template";

        /// <inheritdoc />
        public IEnumerable<HealthRuleResult> Execute(Document doc)
        {
            ArgumentNullException.ThrowIfNull(doc);

            var viewsWithoutTemplate = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.CanBePrinted &&
                            v.ViewType != ViewType.Internal &&
                            v.ViewType != ViewType.ProjectBrowser &&
                            v.ViewType != ViewType.SystemBrowser &&
                            v.ViewType != ViewType.DrawingSheet &&
                            v.ViewType != ViewType.Legend &&
                            v.ViewTemplateId == ElementId.InvalidElementId)
                .ToList();

            var offendingList = viewsWithoutTemplate
                .Select(v => new OffendingElementInfo
                {
                    ElementId = v.Id,
                    IssueDescription = $"View Without View Template: '{v.Name}' (Type: {v.ViewType}, ID: {v.Id.Value}). Assigning View Templates ensures graphic consistency across drawings and project standards compliance."
                })
                .ToList();

            int count = offendingList.Count;
            HealthStatus status = count == 0 ? HealthStatus.Pass : (count < 10 ? HealthStatus.Warning : HealthStatus.Fail);

            return new[]
            {
                new HealthRuleResult
                {
                    RuleName = Name,
                    Category = "Views & Sheets",
                    Description = count == 0
                        ? "All printable model views have assigned View Templates."
                        : $"Found {count} view(s) without a View Template. Select an Element ID to view details or locate in Revit.",
                    Status = status,
                    Count = count,
                    OffendingElements = offendingList
                }
            };
        }
    }
}
