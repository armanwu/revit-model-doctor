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
    /// Supports Category Ignoring, Rule Name Ignoring, Dynamic Overall Model QC Assessment, and Rule/Element Level Auditing.
    /// </summary>
    public class HealthCheckDashboardViewModel : INotifyPropertyChanged
    {
        private HealthRuleResult? _selectedRule;
        private OffendingElementInfo? _selectedElement;

        private bool _showIgnoredItems;
        private bool _showIgnoredCategories = true;
        private bool _isUpdatingResults;

        private string _overallQcStatusText = "EVALUATING...";
        private string _overallQcMessage = "Calculating model QC status...";
        private string _overallQcColorHex = "#64748B";
        private int _overallHealthScore = 100;

        private int _passedRulesCount;
        private int _warningRulesCount;
        private int _failedRulesCount;
        private int _totalActiveIssuesCount;
        private int _ignoredCategoriesCount;
        private int _ignoredRulesCount;
        private int _totalRulesCount;

        public UIDocument? UiDoc { get; }
        public ExternalEvent? SelectElementEvent { get; }
        public SelectElementHandler? SelectElementHandler { get; }
        public ExternalEvent? IgnoreElementEvent { get; }
        public IgnoreElementHandler? IgnoreElementHandler { get; }

        public string DocumentTitle { get; }
        public string AuditTime { get; }

        public List<HealthRuleResult> AllRuleResults { get; }
        public ObservableCollection<HealthRuleResult> RuleResults { get; }
        public ObservableCollection<CategoryFilterItem> CategoryFilters { get; }

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

        public bool ShowIgnoredCategories
        {
            get => _showIgnoredCategories;
            set
            {
                if (SetProperty(ref _showIgnoredCategories, value))
                {
                    UpdateFilteredRuleResults();
                }
            }
        }

        public HealthRuleResult? SelectedRule
        {
            get => _selectedRule;
            set
            {
                if (_isUpdatingResults) return;
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

        // --- Overall QC Assessment Properties ---
        public string OverallQcStatusText
        {
            get => _overallQcStatusText;
            private set => SetProperty(ref _overallQcStatusText, value);
        }

        public string OverallQcMessage
        {
            get => _overallQcMessage;
            private set => SetProperty(ref _overallQcMessage, value);
        }

        public string OverallQcColorHex
        {
            get => _overallQcColorHex;
            private set => SetProperty(ref _overallQcColorHex, value);
        }

        public int OverallHealthScore
        {
            get => _overallHealthScore;
            private set => SetProperty(ref _overallHealthScore, value);
        }

        public int PassedRulesCount
        {
            get => _passedRulesCount;
            private set => SetProperty(ref _passedRulesCount, value);
        }

        public int WarningRulesCount
        {
            get => _warningRulesCount;
            private set => SetProperty(ref _warningRulesCount, value);
        }

        public int FailedRulesCount
        {
            get => _failedRulesCount;
            private set => SetProperty(ref _failedRulesCount, value);
        }

        public int TotalActiveIssuesCount
        {
            get => _totalActiveIssuesCount;
            private set => SetProperty(ref _totalActiveIssuesCount, value);
        }

        public int IgnoredCategoriesCount
        {
            get => _ignoredCategoriesCount;
            private set => SetProperty(ref _ignoredCategoriesCount, value);
        }

        public int IgnoredRulesCount
        {
            get => _ignoredRulesCount;
            private set => SetProperty(ref _ignoredRulesCount, value);
        }

        public int TotalRulesCount
        {
            get => _totalRulesCount;
            private set => SetProperty(ref _totalRulesCount, value);
        }

        // --- Commands ---
        public ICommand SelectAndShowElementCommand { get; }
        public ICommand IgnoreSelectedElementCommand { get; }
        public ICommand UnignoreSelectedElementCommand { get; }
        public ICommand CopySelectedElementIdCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ToggleAllCategoriesCommand { get; }
        public ICommand ToggleSelectedRuleIgnoreCommand { get; }
        public ICommand IncludeAllRulesCommand { get; }
        public ICommand OpenHelpCommand { get; }

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

            AllRuleResults = results?.ToList() ?? new List<HealthRuleResult>();
            RuleResults = new ObservableCollection<HealthRuleResult>();
            CategoryFilters = new ObservableCollection<CategoryFilterItem>();

            foreach (var r in AllRuleResults)
            {
                r.OnIgnoreStateChanged = () => RecalculateCountsAndStatuses();
            }

            // Initialize distinct categories
            var distinctCategories = AllRuleResults
                .Select(r => r.Category)
                .Distinct()
                .OrderBy(c => c);

            foreach (var catName in distinctCategories)
            {
                var catItem = new CategoryFilterItem
                {
                    CategoryName = catName,
                    IsIgnored = false,
                    OnIgnoredChanged = () => RecalculateCountsAndStatuses()
                };
                CategoryFilters.Add(catItem);
            }

            SelectAndShowElementCommand = new RelayCommand(ExecuteSelectAndShowElement, _ => SelectedElement != null);
            IgnoreSelectedElementCommand = new RelayCommand(ExecuteIgnoreSelectedElement, _ => SelectedElement != null && !SelectedElement.IsIgnored);
            UnignoreSelectedElementCommand = new RelayCommand(ExecuteUnignoreSelectedElement, _ => SelectedElement != null && SelectedElement.IsIgnored);
            CopySelectedElementIdCommand = new RelayCommand(ExecuteCopySelectedElementId, _ => SelectedElement != null);
            ExportCsvCommand = new RelayCommand(ExecuteExportCsv, _ => AllRuleResults != null && AllRuleResults.Count > 0);
            CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());
            ToggleAllCategoriesCommand = new RelayCommand(ExecuteToggleAllCategories);
            ToggleSelectedRuleIgnoreCommand = new RelayCommand(ExecuteToggleSelectedRuleIgnore, _ => SelectedRule != null);
            IncludeAllRulesCommand = new RelayCommand(ExecuteIncludeAllRules);
            OpenHelpCommand = new RelayCommand(ExecuteOpenHelp);

            RecalculateCountsAndStatuses();
        }

        private void ExecuteToggleAllCategories(object? parameter)
        {
            bool hasIncluded = CategoryFilters.Any(c => c.IsIncluded);
            bool targetState = hasIncluded; // if has included, disable all

            foreach (var cat in CategoryFilters)
            {
                cat.IsIgnored = targetState;
            }
            RecalculateCountsAndStatuses();
        }

        private void ExecuteToggleSelectedRuleIgnore(object? parameter)
        {
            if (SelectedRule != null)
            {
                SelectedRule.IsRuleIgnored = !SelectedRule.IsRuleIgnored;
            }
        }

        private void ExecuteIncludeAllRules(object? parameter)
        {
            foreach (var r in AllRuleResults)
            {
                r.IsRuleIgnored = false;
            }
            foreach (var cat in CategoryFilters)
            {
                cat.IsIgnored = false;
            }
            RecalculateCountsAndStatuses();
        }

        private void ExecuteOpenHelp(object? parameter)
        {
            try
            {
                var helpView = new ModelDoctor.Views.HelpView();

                try
                {
                    var activeWpfWindow = System.Windows.Application.Current?.Windows
                        .OfType<System.Windows.Window>()
                        .FirstOrDefault(w => w.IsVisible && w is ModelDoctor.Views.HealthCheckDashboardView);

                    if (activeWpfWindow != null)
                    {
                        helpView.Owner = activeWpfWindow;
                    }
                    else if (UiDoc?.Application?.MainWindowHandle != null && UiDoc.Application.MainWindowHandle != IntPtr.Zero)
                    {
                        var helper = new System.Windows.Interop.WindowInteropHelper(helpView);
                        helper.Owner = UiDoc.Application.MainWindowHandle;
                    }
                }
                catch
                {
                    helpView.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                }

                helpView.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to open Help Guide: {ex.Message}", "Model Doctor Help Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void RecalculateCountsAndStatuses()
        {
            // 1. Recalculate active counts and status per rule based on element ignores
            foreach (var rule in AllRuleResults)
            {
                if (rule.OffendingElements != null)
                {
                    var activeElements = rule.OffendingElements.Where(e => !e.IsIgnored).ToList();
                    rule.Count = activeElements.Count;

                    if (rule.StatusEvaluator != null)
                    {
                        rule.Status = rule.StatusEvaluator(activeElements);
                    }
                }
            }

            // 2. Identify ignored vs active categories
            var ignoredCatNames = CategoryFilters
                .Where(c => c.IsIgnored)
                .Select(c => c.CategoryName)
                .ToHashSet();

            IgnoredCategoriesCount = ignoredCatNames.Count;
            TotalRulesCount = AllRuleResults.Count;

            // 3. Mark rule objects with IsCategoryIgnored
            foreach (var rule in AllRuleResults)
            {
                rule.IsCategoryIgnored = ignoredCatNames.Contains(rule.Category);
            }

            IgnoredRulesCount = AllRuleResults.Count(r => r.IsRuleIgnored);

            // 4. Update category summary metrics
            foreach (var cat in CategoryFilters)
            {
                var catRules = AllRuleResults.Where(r => r.Category == cat.CategoryName).ToList();
                cat.RuleCount = catRules.Count;
                cat.ActiveIssueCount = catRules.Where(r => !r.IsEffectivelyIgnored).Sum(r => r.Count);
                if (catRules.Any(r => r.Status == HealthStatus.Fail && !r.IsEffectivelyIgnored))
                    cat.Status = HealthStatus.Fail;
                else if (catRules.Any(r => r.Status == HealthStatus.Warning && !r.IsEffectivelyIgnored))
                    cat.Status = HealthStatus.Warning;
                else
                    cat.Status = HealthStatus.Pass;
            }

            // 5. Evaluate Overall Model QC Assessment on ACTIVE (NON-EFFECTIVELY IGNORED) rules ONLY
            var activeRules = AllRuleResults.Where(r => !r.IsEffectivelyIgnored).ToList();

            TotalActiveIssuesCount = activeRules.Sum(r => r.Count);
            FailedRulesCount = activeRules.Count(r => r.Status == HealthStatus.Fail);
            WarningRulesCount = activeRules.Count(r => r.Status == HealthStatus.Warning);
            PassedRulesCount = activeRules.Count(r => r.Status == HealthStatus.Pass);

            double totalScore = 0.0;
            foreach (var r in activeRules)
            {
                if (r.Status == HealthStatus.Pass)
                    totalScore += 100.0;
                else if (r.Status == HealthStatus.Warning)
                    totalScore += 70.0;
                else
                    totalScore += 0.0;
            }

            int scorePercent = activeRules.Count > 0 ? (int)Math.Round(totalScore / activeRules.Count) : 0;
            OverallHealthScore = scorePercent;

            if (AllRuleResults.Count > 0 && activeRules.Count == 0)
            {
                OverallQcStatusText = "ALL RULES IGNORED";
                OverallQcMessage = "All audit rules are currently ignored. Enable at least one rule or category to evaluate model Quality Control.";
                OverallQcColorHex = "#64748B"; // Gray
            }
            else if (scorePercent >= 85)
            {
                OverallQcStatusText = $"PASS / HEALTHY ({scorePercent}%)";
                OverallQcMessage = $"Model is HEALTHY & PASS (Overall Score: {scorePercent}%). Model is clean, performance optimal, ready for coordination and deliverables.";
                OverallQcColorHex = "#10B981"; // Green
            }
            else if (scorePercent >= 65)
            {
                OverallQcStatusText = $"WARNING / NEEDS ATTENTION ({scorePercent}%)";
                OverallQcMessage = $"Model HAS WARNINGS / NEEDS ATTENTION (Overall Score: {scorePercent}%). Model runs smoothly, but periodic cleanup is required before issues expand.";
                OverallQcColorHex = "#F59E0B"; // Warning Orange
            }
            else
            {
                OverallQcStatusText = $"FAIL / CRITICAL ({scorePercent}%)";
                OverallQcMessage = $"Model FAILED / CRITICAL STATE (Overall Score: {scorePercent}%). High risk of model sluggishness, file corruption, or inaccurate quantity schedules. Immediate fix required!";
                OverallQcColorHex = "#EF4444"; // Red
            }

            UpdateFilteredRuleResults();
        }

        private void UpdateFilteredRuleResults()
        {
            _isUpdatingResults = true;

            try
            {
                var prevRule = _selectedRule;
                var prevElem = _selectedElement;

                RuleResults.Clear();
                var visibleRules = ShowIgnoredCategories
                    ? AllRuleResults
                    : AllRuleResults.Where(r => !r.IsEffectivelyIgnored);

                foreach (var r in visibleRules)
                {
                    RuleResults.Add(r);
                }

                if (prevRule != null && RuleResults.Contains(prevRule))
                {
                    _selectedRule = prevRule;
                }
                else
                {
                    _selectedRule = RuleResults.FirstOrDefault();
                }
                OnPropertyChanged(nameof(SelectedRule));

                OnPropertyChanged(nameof(OffendingElements));

                var currentOffendingList = OffendingElements.ToList();
                if (prevElem != null && currentOffendingList.Contains(prevElem))
                {
                    _selectedElement = prevElem;
                }
                else
                {
                    _selectedElement = currentOffendingList.FirstOrDefault();
                }
                OnPropertyChanged(nameof(SelectedElement));
            }
            finally
            {
                _isUpdatingResults = false;
            }
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
                    sb.AppendLine("Overall QC Status: " + OverallQcStatusText);
                    sb.AppendLine("Audit Date: " + AuditTime);
                    sb.AppendLine("Document: " + DocumentTitle);
                    sb.AppendLine();
                    sb.AppendLine("Category,Category Ignored?,Rule Name,Rule Ignored?,Status,Total Issues,Element ID,Element Ignored?,Issue Description");

                    foreach (var rule in AllRuleResults)
                    {
                        string catIgnoredStr = rule.IsCategoryIgnored ? "YES" : "NO";
                        string ruleIgnoredStr = rule.IsRuleIgnored ? "YES" : "NO";

                        if (rule.OffendingElements != null && rule.OffendingElements.Count > 0)
                        {
                            foreach (var elem in rule.OffendingElements)
                            {
                                string elemIgnoredStr = elem.IsIgnored ? "YES" : "NO";
                                sb.AppendLine($"{EscapeCsv(rule.Category)},{catIgnoredStr},{EscapeCsv(rule.RuleName)},{ruleIgnoredStr},{EscapeCsv(rule.Status.ToString())},{rule.Count},{EscapeCsv(elem.ElementIdValue)},{elemIgnoredStr},{EscapeCsv(elem.IssueDescription)}");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"{EscapeCsv(rule.Category)},{catIgnoredStr},{EscapeCsv(rule.RuleName)},{ruleIgnoredStr},{EscapeCsv(rule.Status.ToString())},{rule.Count},N/A,NO,{EscapeCsv(rule.Description)}");
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
