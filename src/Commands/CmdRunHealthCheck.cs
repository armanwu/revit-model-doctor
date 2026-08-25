using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ModelDoctor.Core;
using ModelDoctor.Rules;
using ModelDoctor.ViewModels;
using ModelDoctor.Views;

namespace ModelDoctor.Commands
{
    /// <summary>
    /// Revit External Command to run the Model Doctor health check rules and open the WPF Dashboard view.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    public class CmdRunHealthCheck : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Null validation for ActiveUIDocument and Document
            UIApplication? uiApp = commandData?.Application;
            UIDocument? uiDoc = uiApp?.ActiveUIDocument;
            Document? doc = uiDoc?.Document;

            if (doc == null)
            {
                TaskDialog.Show("Model Doctor Error", "No active Revit document found. Please open a Revit model first.");
                return Result.Failed;
            }

            // Register modular rules structured into 3 refined English categories
            var rules = new List<IHealthCheckRule>
            {
                // 1. Model Performance
                new WarningCountRule(),
                new CadImportRule(),
                new InPlaceFamilyRule(),
                new PurgeableElementsRule(),
                new UnusedViewFiltersAndTemplatesRule(),

                // 2. Data & Deliverable Integrity
                new UnboundedRoomsAndSpacesRule(),
                new UnplacedViewsRule(),
                new UnusedSchedulesAndLegendsRule(),
                new RevitLinksAndIfcStatusRule(),
                new DuplicateElementsRule(),
                new ModelGroupDuplicationRule(),
                new WorksetAllocationRule(),

                // 3. Spatial & Model Safety
                new SurveyAndBasePointDistanceRule(),
                new UnpinnedGridsAndLevelsRule(),
                new ViewClippingAndExtentsRule()
            };

            var results = new List<HealthRuleResult>();

            // Retrieve stored ignored element IDs from ExtensibleStorage
            HashSet<long> ignoredElementIds = IgnoreStorageService.GetIgnoredElementIds(doc);

            // Execute each rule with error handling
            foreach (var rule in rules)
            {
                try
                {
                    var ruleResults = rule.Execute(doc);
                    foreach (var res in ruleResults)
                    {
                        if (res.OffendingElements != null)
                        {
                            foreach (var elem in res.OffendingElements)
                            {
                                if (elem.ElementId != null && ignoredElementIds.Contains(elem.ElementId.Value))
                                {
                                    elem.IsIgnored = true;
                                }
                            }
                        }
                    }
                    results.AddRange(ruleResults);
                }
                catch (Exception ex)
                {
                    results.Add(new HealthRuleResult
                    {
                        RuleName = rule.Name,
                        Category = "System Error",
                        Description = $"Error executing rule: {ex.Message}",
                        Status = HealthStatus.Fail,
                        Count = 0
                    });
                }
            }

            // Open interactive WPF Dashboard Window (Modeless)
            try
            {
                var selectHandler = new SelectElementHandler();
                var selectEvent = ExternalEvent.Create(selectHandler);

                var ignoreHandler = new IgnoreElementHandler();
                var ignoreEvent = ExternalEvent.Create(ignoreHandler);

                var viewModel = new HealthCheckDashboardViewModel(
                    uiDoc, 
                    results, 
                    selectEvent, 
                    selectHandler,
                    ignoreEvent,
                    ignoreHandler);

                var view = new HealthCheckDashboardView(viewModel);

                if (uiApp != null && uiApp.MainWindowHandle != IntPtr.Zero)
                {
                    var helper = new System.Windows.Interop.WindowInteropHelper(view);
                    helper.Owner = uiApp.MainWindowHandle;
                }

                view.Show();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Model Doctor UI Error", $"Failed to launch dashboard window: {ex.Message}");
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
