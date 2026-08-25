using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Represents the result output generated after running a health check rule.
    /// Supports category-level and individual rule-level ignore controls for QC assessment.
    /// </summary>
    public class HealthRuleResult : INotifyPropertyChanged
    {
        private string _ruleName = string.Empty;
        private string _category = string.Empty;
        private string _description = string.Empty;
        private HealthStatus _status = HealthStatus.Pass;
        private int _count;
        private bool _isCategoryIgnored;
        private bool _isRuleIgnored;

        /// <summary>
        /// The display name of the rule evaluated.
        /// </summary>
        public string RuleName
        {
            get => _ruleName;
            set => SetProperty(ref _ruleName, value);
        }

        /// <summary>
        /// The category of the health rule (e.g., "Imports &amp; Links", "Model Integrity").
        /// </summary>
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        /// <summary>
        /// Detailed summary description of the health evaluation rule.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Overall health status (Pass, Warning, or Fail).
        /// </summary>
        public HealthStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// Quantitative count of offending elements or issues detected.
        /// </summary>
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        /// <summary>
        /// Indicates whether this rule's parent category is currently ignored in QC evaluation.
        /// </summary>
        public bool IsCategoryIgnored
        {
            get => _isCategoryIgnored;
            set
            {
                if (SetProperty(ref _isCategoryIgnored, value))
                {
                    OnPropertyChanged(nameof(IsEffectivelyIgnored));
                    OnPropertyChanged(nameof(IsRuleIncluded));
                    OnIgnoreStateChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Indicates whether this specific rule is individually ignored by rule name.
        /// </summary>
        public bool IsRuleIgnored
        {
            get => _isRuleIgnored;
            set
            {
                if (SetProperty(ref _isRuleIgnored, value))
                {
                    OnPropertyChanged(nameof(IsEffectivelyIgnored));
                    OnPropertyChanged(nameof(IsRuleIncluded));
                    OnIgnoreStateChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Helper property for WPF CheckBox binding ([x] Included in QC).
        /// </summary>
        public bool IsRuleIncluded
        {
            get => !IsEffectivelyIgnored;
            set => IsRuleIgnored = !value;
        }

        /// <summary>
        /// True if either the parent category or the rule itself is ignored.
        /// </summary>
        public bool IsEffectivelyIgnored => IsCategoryIgnored || IsRuleIgnored;

        /// <summary>
        /// Action callback invoked whenever the rule ignore state changes.
        /// </summary>
        public Action? OnIgnoreStateChanged { get; set; }

        /// <summary>
        /// Optional delegate evaluator used to dynamically recalculate rule status when elements are ignored or unignored.
        /// </summary>
        public Func<IEnumerable<OffendingElementInfo>, HealthStatus>? StatusEvaluator { get; set; }

        /// <summary>
        /// Collection of offending elements along with their specific error/warning details.
        /// </summary>
        public ICollection<OffendingElementInfo> OffendingElements { get; set; } = new List<OffendingElementInfo>();

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
