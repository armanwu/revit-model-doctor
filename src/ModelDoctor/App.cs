using System;
using System.Reflection;
using Autodesk.Revit.UI;
using ModelDoctor.Commands;

namespace ModelDoctor
{
    /// <summary>
    /// Revit External Application entry point to register ribbon UI components upon Revit startup.
    /// </summary>
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Calling CreateRibbonPanel(panelName) places the panel inside the default built-in "Add-Ins" tab in Revit.
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("Model Doctor");

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Create push button for CmdRunHealthCheck
            var buttonData = new PushButtonData(
                "btnRunHealthCheck",
                "Run Health\nCheck",
                assemblyPath,
                typeof(CmdRunHealthCheck).FullName
            )
            {
                ToolTip = "Audit Revit model health for CAD imports, warning count, and offending elements."
            };

            ribbonPanel.AddItem(buttonData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
