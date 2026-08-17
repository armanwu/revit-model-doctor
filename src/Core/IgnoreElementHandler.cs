using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Specifies the action type for the IgnoreElementHandler.
    /// </summary>
    public enum IgnoreActionType
    {
        Ignore,
        Unignore
    }

    /// <summary>
    /// Revit ExternalEvent handler to safely execute ExtensibleStorage Transactions from modeless WPF UI.
    /// </summary>
    public class IgnoreElementHandler : IExternalEventHandler
    {
        public IgnoreActionType ActionType { get; set; } = IgnoreActionType.Ignore;
        public ElementId? TargetElementId { get; set; }
        public Action? OnCompleted { get; set; }

        /// <inheritdoc />
        public void Execute(UIApplication app)
        {
            if (TargetElementId == null || TargetElementId == ElementId.InvalidElementId)
                return;

            UIDocument? uiDoc = app?.ActiveUIDocument;
            Document? doc = uiDoc?.Document;
            if (doc == null) return;

            try
            {
                string txName = ActionType == IgnoreActionType.Ignore
                    ? "Model Doctor - Ignore Element"
                    : "Model Doctor - Unignore Element";

                using (Transaction tx = new Transaction(doc, txName))
                {
                    tx.Start();
                    if (ActionType == IgnoreActionType.Ignore)
                    {
                        IgnoreStorageService.IgnoreElement(doc, TargetElementId);
                    }
                    else
                    {
                        IgnoreStorageService.UnignoreElement(doc, TargetElementId);
                    }
                    tx.Commit();
                }

                OnCompleted?.Invoke();
            }
            catch
            {
                // Silently swallow errors if transaction fails
            }
        }

        /// <inheritdoc />
        public string GetName() => "ModelDoctorIgnoreElementHandler";
    }
}
