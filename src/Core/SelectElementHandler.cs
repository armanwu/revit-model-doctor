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
            if (uiDoc == null) return;

            try
            {
                var idList = new List<ElementId> { ElementIdToSelect };
                uiDoc.Selection.SetElementIds(idList);
                uiDoc.ShowElements(ElementIdToSelect);
            }
            catch
            {
                // Silently swallow if element is deleted or non-viewable in active view context
            }
        }

        /// <inheritdoc />
        public string GetName() => "ModelDoctorSelectElementHandler";
    }
}
