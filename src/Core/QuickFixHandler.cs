using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Revit ExternalEvent handler to safely execute Quick Fix Transactions from modeless WPF UI.
    /// </summary>
    public class QuickFixHandler : IExternalEventHandler
    {
        public HealthRuleResult? TargetRule { get; set; }
        public IQuickFixableRule? FixableRule { get; set; }
        public Action? OnCompleted { get; set; }

        /// <inheritdoc />
        public void Execute(UIApplication app)
        {
            if (TargetRule == null || FixableRule == null)
                return;

            UIDocument? uiDoc = app?.ActiveUIDocument;
            Document? doc = uiDoc?.Document;
            if (doc == null) return;

            var activeOffending = TargetRule.OffendingElements.Where(e => !e.IsIgnored).ToList();
            if (activeOffending.Count == 0) return;

            try
            {
                string txName = $"Model Doctor Quick Fix - {TargetRule.RuleName}";
                int countFixed = 0;

                using (Transaction tx = new Transaction(doc, txName))
                {
                    tx.Start();
                    countFixed = FixableRule.ExecuteQuickFix(doc, activeOffending);
                    tx.Commit();
                }

                // Live re-evaluate rule against active document to update rule counts & status in real-time
                if (TargetRule.RuleSource != null)
                {
                    var freshResults = TargetRule.RuleSource.Execute(doc);
                    var fresh = freshResults?.FirstOrDefault();
                    if (fresh != null)
                    {
                        TargetRule.OffendingElements = fresh.OffendingElements;
                        TargetRule.Count = fresh.Count;
                        TargetRule.Status = fresh.Status;
                        TargetRule.Description = fresh.Description;
                    }
                }

                MessageBox.Show(
                    $"Quick Fix completed successfully!\n\nFixed {countFixed} item(s) for rule '{TargetRule.RuleName}'.\n\nYou can press Ctrl + Z in Revit if you need to undo this action.",
                    "Model Doctor Quick Fix",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                OnCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Quick Fix error while processing '{TargetRule.RuleName}':\n{ex.Message}",
                    "Model Doctor Quick Fix Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <inheritdoc />
        public string GetName() => "ModelDoctorQuickFixHandler";
    }
}
