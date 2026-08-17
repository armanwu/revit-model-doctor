using System;
using System.Reflection;
using System.Windows.Media.Imaging;
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

            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyPath = assembly.Location;

            // Create push button for CmdRunHealthCheck
            var buttonData = new PushButtonData(
                "btnRunHealthCheck",
                "Run Health\nCheck",
                assemblyPath,
                typeof(CmdRunHealthCheck).FullName
            )
            {
                ToolTip = "Audit Revit model health for CAD imports, warning count, and offending elements.",
                LargeImage = LoadImage(assembly, "Icon/icon32.png"),
                Image = LoadImage(assembly, "Icon/icon16.png")
            };

            ribbonPanel.AddItem(buttonData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private static BitmapImage? LoadImage(Assembly assembly, string relativePath)
        {
            try
            {
                string packUri = $"pack://application:,,,/{assembly.GetName().Name};component/{relativePath}";
                return new BitmapImage(new Uri(packUri, UriKind.Absolute));
            }
            catch
            {
                return null;
            }
        }
    }
}
