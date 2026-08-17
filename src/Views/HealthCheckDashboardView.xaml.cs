using System.Windows;
using ModelDoctor.ViewModels;

namespace ModelDoctor.Views
{
    /// <summary>
    /// Interaction logic for HealthCheckDashboardView.xaml
    /// </summary>
    public partial class HealthCheckDashboardView : Window
    {
        public HealthCheckDashboardView(HealthCheckDashboardViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            if (viewModel != null)
            {
                viewModel.RequestClose = () => Close();
            }
        }

        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is HealthCheckDashboardViewModel vm && vm.SelectAndShowElementCommand.CanExecute(null))
            {
                vm.SelectAndShowElementCommand.Execute(null);
            }
        }
    }
}
