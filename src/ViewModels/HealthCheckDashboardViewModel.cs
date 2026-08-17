using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ModelDoctor.Core;

namespace ModelDoctor.ViewModels
{
    /// <summary>
    /// ViewModel driving the interactive WPF Health Check Dashboard view.
    /// </summary>
    public class HealthCheckDashboardViewModel : INotifyPropertyChanged
    {
        private HealthRuleResult? _selectedRule;
        private OffendingElementInfo? _selectedElement;

        private bool _showIgnoredItems;

        public UIDocument? UiDoc { get; }
        public ExternalEvent? SelectElementEvent { get; }
        public SelectElementHandler? SelectElementHandler { get; }
        public ExternalEvent? IgnoreElementEvent { get; }
        public IgnoreElementHandler? IgnoreElementHandler { get; }

        public string DocumentTitle { get; }
        public string AuditTime { get; }
        public ObservableCollection<HealthRuleResult> RuleResults { get; }

        public bool ShowIgnoredItems
        {
            get => _showIgnoredItems;
            set
            {
                if (SetProperty(ref _showIgnoredItems, value))
                {
                    OnPropertyChanged(nameof(OffendingElements));
                    SelectedElement = OffendingElements.FirstOrDefault();
                }
            }
        }

        public HealthRuleResult? SelectedRule
        {
            get => _selectedRule;
            set
            {
                if (SetProperty(ref _selectedRule, value))
                {
                    OnPropertyChanged(nameof(OffendingElements));
                    SelectedElement = OffendingElements.FirstOrDefault();
                }
            }
        }

        public OffendingElementInfo? SelectedElement
        {
            get => _selectedElement;
            set => SetProperty(ref _selectedElement, value);
        }

        public IEnumerable<OffendingElementInfo> OffendingElements
        {
            get
            {
                if (SelectedRule?.OffendingElements == null)
                    return Enumerable.Empty<OffendingElementInfo>();

                if (ShowIgnoredItems)
                    return SelectedRule.OffendingElements;

                return SelectedRule.OffendingElements.Where(e => !e.IsIgnored);
            }
        }

        public ICommand SelectAndShowElementCommand { get; }
        public ICommand IgnoreSelectedElementCommand { get; }
        public ICommand UnignoreSelectedElementCommand { get; }
        public ICommand CopySelectedElementIdCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand CloseCommand { get; }

        public Action? RequestClose { get; set; }

        public HealthCheckDashboardViewModel(
            UIDocument? uiDoc, 
            IEnumerable<HealthRuleResult> results,
            ExternalEvent? selectElementEvent = null,
            SelectElementHandler? selectElementHandler = null,
            ExternalEvent? ignoreElementEvent = null,
            IgnoreElementHandler? ignoreElementHandler = null)
        {
            UiDoc = uiDoc;
            SelectElementEvent = selectElementEvent;
            SelectElementHandler = selectElementHandler;
            IgnoreElementEvent = ignoreElementEvent;
            IgnoreElementHandler = ignoreElementHandler;

            Document? doc = uiDoc?.Document;

            DocumentTitle = doc?.Title ?? "Unknown Document";
            AuditTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            RuleResults = new ObservableCollection<HealthRuleResult>(results ?? Enumerable.Empty<HealthRuleResult>());

            SelectedRule = RuleResults.FirstOrDefault();

            SelectAndShowElementCommand = new RelayCommand(ExecuteSelectAndShowElement, _ => SelectedElement != null);
            IgnoreSelectedElementCommand = new RelayCommand(ExecuteIgnoreSelectedElement, _ => SelectedElement != null && !SelectedElement.IsIgnored);
            UnignoreSelectedElementCommand = new RelayCommand(ExecuteUnignoreSelectedElement, _ => SelectedElement != null && SelectedElement.IsIgnored);
            CopySelectedElementIdCommand = new RelayCommand(ExecuteCopySelectedElementId, _ => SelectedElement != null);
            ExportCsvCommand = new RelayCommand(ExecuteExportCsv, _ => RuleResults != null && RuleResults.Count > 0);
            CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());

            RecalculateCountsAndStatuses();
        }

        private void ExecuteSelectAndShowElement(object? parameter)
        {
            if (SelectedElement == null) return;

            ElementId elementId = SelectedElement.ElementId;
            if (elementId == null || elementId == ElementId.InvalidElementId)
            {
                MessageBox.Show("Invalid or non-existent Element ID.", "Model Doctor", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectElementHandler != null && SelectElementEvent != null)
            {
                SelectElementHandler.ElementIdToSelect = elementId;
                SelectElementEvent.Raise();
            }
            else if (UiDoc != null)
            {
                try
                {
                    Element targetElem = UiDoc.Document.GetElement(elementId);
                    if (targetElem is View targetView && !targetView.IsTemplate)
                    {
                        UiDoc.ActiveView = targetView;
                    }
                    else
                    {
                        var idList = new List<ElementId> { elementId };
                        UiDoc.Selection.SetElementIds(idList);
                        UiDoc.ShowElements(elementId);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not select/show element in Revit:\n{ex.Message}", "Model Doctor Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteIgnoreSelectedElement(object? parameter)
        {
            if (SelectedElement == null) return;
            SelectedElement.IsIgnored = true;

            if (IgnoreElementHandler != null && IgnoreElementEvent != null)
            {
                IgnoreElementHandler.TargetElementId = SelectedElement.ElementId;
                IgnoreElementHandler.ActionType = IgnoreActionType.Ignore;
                IgnoreElementHandler.OnCompleted = () => RecalculateCountsAndStatuses();
                IgnoreElementEvent.Raise();
            }

            RecalculateCountsAndStatuses();
        }

        private void ExecuteUnignoreSelectedElement(object? parameter)
        {
            if (SelectedElement == null) return;
            SelectedElement.IsIgnored = false;

            if (IgnoreElementHandler != null && IgnoreElementEvent != null)
            {
                IgnoreElementHandler.TargetElementId = SelectedElement.ElementId;
                IgnoreElementHandler.ActionType = IgnoreActionType.Unignore;
                IgnoreElementHandler.OnCompleted = () => RecalculateCountsAndStatuses();
                IgnoreElementEvent.Raise();
            }

            RecalculateCountsAndStatuses();
        }

        public void RecalculateCountsAndStatuses()
        {
            foreach (var rule in RuleResults)
            {
                if (rule.OffendingElements != null)
                {
                    int activeCount = rule.OffendingElements.Count(e => !e.IsIgnored);
                    rule.Count = activeCount;
                    if (activeCount == 0)
                        rule.Status = HealthStatus.Pass;
                    else if (activeCount < 15)
                        rule.Status = HealthStatus.Warning;
                    else
                        rule.Status = HealthStatus.Fail;
                }
            }
            OnPropertyChanged(nameof(OffendingElements));
        }

        private void ExecuteCopySelectedElementId(object? parameter)
        {
            if (SelectedElement != null)
            {
                string idString = SelectedElement.ElementIdValue;
                Clipboard.SetText(idString);
                MessageBox.Show($"Copied Element ID ({idString}) to clipboard.", "Model Doctor", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteExportCsv(object? parameter)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"ModelDoctor_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Category,Rule Name,Status,Total Issues,Element ID,Issue Description");

                    foreach (var rule in RuleResults)
                    {
                        if (rule.OffendingElements != null && rule.OffendingElements.Count > 0)
                        {
                            foreach (var elem in rule.OffendingElements)
                            {
                                sb.AppendLine($"{EscapeCsv(rule.Category)},{EscapeCsv(rule.RuleName)},{EscapeCsv(rule.Status.ToString())},{rule.Count},{EscapeCsv(elem.ElementIdValue)},{EscapeCsv(elem.IssueDescription)}");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"{EscapeCsv(rule.Category)},{EscapeCsv(rule.RuleName)},{EscapeCsv(rule.Status.ToString())},{rule.Count},N/A,{EscapeCsv(rule.Description)}");
                        }
                    }

                    File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Audit report successfully exported to CSV:\n{dialog.FileName}", "Model Doctor", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export CSV: {ex.Message}", "Model Doctor Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string EscapeCsv(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
