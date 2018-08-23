using System.Windows;
using System.Windows.Controls;
using BI_Wizard.Helper;
using BI_Wizard.Model;
using BI_Wizard.ViewModel;

namespace BI_Wizard.View
{
    /// <summary>
    /// Interaction logic for Page_06_CB_V.xaml
    /// </summary>
    public partial class Page_10_Cb_V : MgaXctkWizardPage
    {
        public  Page_10_Cb_Vm ViewModel;
        public Page_10_Cb_V()
        {
            InitializeComponent();
            ViewModel = (Page_10_Cb_Vm) DataContext;
        }
        public override void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {

        }

        private void CreateCube_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AnalysisM.CreateCube();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.AnalysisM.DwDsPassword = ((PasswordBox)sender).Password;
        }

        private void ProcessCube_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AnalysisM.ProcessCube();
        }
    }
}
