using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Represents a category filter item used for toggling and ignoring entire rule categories in QC evaluation.
    /// </summary>
    public class CategoryFilterItem : INotifyPropertyChanged
    {
        private bool _isIgnored;
        private int _ruleCount;
        private int _activeIssueCount;
        private HealthStatus _status = HealthStatus.Pass;

        /// <summary>
        /// Display name of the category (e.g. "Imports & Links", "Model Hygiene").
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether this entire category is ignored in QC evaluation.
        /// </summary>
        public bool IsIgnored
        {
            get => _isIgnored;
            set
            {
                if (_isIgnored != value)
                {
                    _isIgnored = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsIncluded));
                    OnIgnoredChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Inverse of <see cref="IsIgnored"/> for WPF CheckBox binding ([x] Included).
        /// </summary>
        public bool IsIncluded
        {
            get => !_isIgnored;
            set => IsIgnored = !value;
        }

        /// <summary>
        /// Total number of rules under this category.
        /// </summary>
        public int RuleCount
        {
            get => _ruleCount;
            set
            {
                if (_ruleCount != value)
                {
                    _ruleCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Total active (non-ignored) offending elements/issues in this category.
        /// </summary>
        public int ActiveIssueCount
        {
            get => _activeIssueCount;
            set
            {
                if (_activeIssueCount != value)
                {
                    _activeIssueCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Aggregated worst health status of rules under this category.
        /// </summary>
        public HealthStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Action callback invoked whenever the ignored state changes.
        /// </summary>
        public Action? OnIgnoredChanged { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
