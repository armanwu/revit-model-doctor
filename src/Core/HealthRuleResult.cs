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
        private IQuickFixableRule? _quickFixRule;

        /// <summary>
        /// Optional reference to quick fix rule implementation.
        /// </summary>
        public IQuickFixableRule? QuickFixRule
        {
            get => _quickFixRule;
            set
            {
                if (SetProperty(ref _quickFixRule, value))
                {
                    OnPropertyChanged(nameof(IsQuickFixable));
                    OnPropertyChanged(nameof(QuickFixDescription));
                }
            }
        }

        /// <summary>
        /// Reference to the rule instance that generated this result, used for live re-evaluations after Quick Fix.
        /// </summary>
        public IHealthCheckRule? RuleSource { get; set; }

        /// <summary>
        /// Indicates whether this rule supports safe 1-click Quick Fix remediation.
        /// </summary>
        public bool IsQuickFixable => QuickFixRule != null;

        /// <summary>
        /// Indicates whether Quick Fix is currently active and available for this rule (has active issues to fix).
        /// </summary>
        public bool CanQuickFix => IsQuickFixable && !IsEffectivelyIgnored && Count > 0;

        /// <summary>
        /// Human-readable explanation of what the Quick Fix will execute.
        /// </summary>
        public string QuickFixDescription => QuickFixRule?.QuickFixDescription ?? string.Empty;

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
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(CanQuickFix));
                }
            }
        }

        /// <summary>
        /// Quantitative count of offending elements or issues detected.
        /// </summary>
        public int Count
        {
            get => _count;
            set
            {
                if (SetProperty(ref _count, value))
                {
                    OnPropertyChanged(nameof(CanQuickFix));
                }
            }
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
                    OnPropertyChanged(nameof(CanQuickFix));
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
                    OnPropertyChanged(nameof(CanQuickFix));
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
