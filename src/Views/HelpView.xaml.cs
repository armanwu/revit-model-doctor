using System.Windows;

namespace ModelDoctor.Views
{
    /// <summary>
    /// Interaction logic for HelpView.xaml - Health Audit &amp; Scoring Guide window.
    /// </summary>
    public partial class HelpView : Window
    {
        public HelpView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
