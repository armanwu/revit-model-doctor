using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Revit ExternalEvent handler to safely select and show elements from a modeless UI.
    /// </summary>
    public class SelectElementHandler : IExternalEventHandler
    {
        /// <summary>
        /// Element ID to select and zoom to in Revit.
        /// </summary>
        public ElementId? ElementIdToSelect { get; set; }

        /// <inheritdoc />
        public void Execute(UIApplication app)
        {
            if (ElementIdToSelect == null || ElementIdToSelect == ElementId.InvalidElementId)
                return;

            UIDocument? uiDoc = app?.ActiveUIDocument;
            Document? doc = uiDoc?.Document;
            if (uiDoc == null || doc == null) return;

            try
            {
                Element targetElem = doc.GetElement(ElementIdToSelect);
                if (targetElem == null) return;

                if (targetElem is View targetView)
                {
                    // If the target element is a View, activate/open it directly in Revit
                    if (!targetView.IsTemplate &&
                        targetView.ViewType != ViewType.Internal &&
                        targetView.ViewType != ViewType.ProjectBrowser &&
                        targetView.ViewType != ViewType.SystemBrowser)
                    {
                        uiDoc.ActiveView = targetView;
                    }
                }
                else
                {
                    // Model / 2D element: select and zoom
                    var idList = new List<ElementId> { ElementIdToSelect };
                    uiDoc.Selection.SetElementIds(idList);
                    uiDoc.ShowElements(ElementIdToSelect);
                }
            }
            catch
            {
                // Silently swallow if element is deleted or cannot be opened in active context
            }
        }

        /// <inheritdoc />
        public string GetName() => "ModelDoctorSelectElementHandler";
    }
}
