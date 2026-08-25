using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Interface contract for health check rules that support safe, automated 1-click Quick Fix remediations.
    /// </summary>
    public interface IQuickFixableRule
    {
        /// <summary>
        /// Gets a clear, human-readable description of what the Quick Fix action will perform.
        /// </summary>
        string QuickFixDescription { get; }

        /// <summary>
        /// Executes the quick fix remediation inside an active Revit transaction.
        /// </summary>
        /// <param name="doc">Active Revit document.</param>
        /// <param name="offendingElements">Collection of offending elements to fix.</param>
        /// <returns>Number of elements successfully remediated.</returns>
        int ExecuteQuickFix(Document doc, IEnumerable<OffendingElementInfo> offendingElements);
    }
}
