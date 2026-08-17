using Autodesk.Revit.DB;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Holds details of an individual offending element and its specific error/warning message.
    /// </summary>
    public class OffendingElementInfo
    {
        /// <summary>
        /// The Revit ElementId.
        /// </summary>
        public ElementId ElementId { get; set; } = ElementId.InvalidElementId;

        /// <summary>
        /// Gets the string representation of the ElementId value.
        /// </summary>
        public string ElementIdValue => ElementId?.Value.ToString() ?? "0";

        /// <summary>
        /// Specific error or warning description for this element.
        /// </summary>
        public string IssueDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this offending element has been marked as Ignored / Suppressed.
        /// </summary>
        public bool IsIgnored { get; set; }
    }
}
