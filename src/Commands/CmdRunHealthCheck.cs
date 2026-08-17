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

            // Register modular rules to execute
            var rules = new List<IHealthCheckRule>
            {
                new CadImportRule(),
                new WarningCountRule()
            };

            var results = new List<HealthRuleResult>();

            // Execute each rule with error handling
            foreach (var rule in rules)
            {
                try
                {
                    HealthRuleResult result = rule.Execute(doc);
                    results.Add(result);
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

                var viewModel = new HealthCheckDashboardViewModel(uiDoc, results, selectEvent, selectHandler);
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
