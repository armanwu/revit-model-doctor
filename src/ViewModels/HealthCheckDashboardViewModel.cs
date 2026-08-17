using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
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

        public string DocumentTitle { get; }
        public string AuditTime { get; }
        public ObservableCollection<HealthRuleResult> RuleResults { get; }

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

        public IEnumerable<OffendingElementInfo> OffendingElements =>
            SelectedRule?.OffendingElements ?? Enumerable.Empty<OffendingElementInfo>();

        public ICommand CopySelectedElementIdCommand { get; }
        public ICommand CloseCommand { get; }

        public Action? RequestClose { get; set; }

        public HealthCheckDashboardViewModel(Document? doc, IEnumerable<HealthRuleResult> results)
        {
            DocumentTitle = doc?.Title ?? "Unknown Document";
            AuditTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            RuleResults = new ObservableCollection<HealthRuleResult>(results ?? Enumerable.Empty<HealthRuleResult>());

            SelectedRule = RuleResults.FirstOrDefault();

            CopySelectedElementIdCommand = new RelayCommand(ExecuteCopySelectedElementId, _ => SelectedElement != null);
            CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());
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
