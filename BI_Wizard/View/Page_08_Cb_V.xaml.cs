using System.Windows;
using BI_Wizard.Helper;
using BI_Wizard.Model;
using BI_Wizard.ViewModel;

namespace BI_Wizard.View
{
    /// <summary>
    /// Interaction logic for Page_04_CB_V.xaml
    /// </summary>
    public partial class Page_08_Cb_V : MgaXctkWizardPage
    {
        public  Page_08_Cb_Vm ViewModel;

        public Page_08_Cb_V()
        {
            InitializeComponent();
            ViewModel = (Page_08_Cb_Vm) DataContext;
        }

        public override void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            e.Cancel = !ViewModel.AnalysisM.ConnectToAnalysisServer();
        }
    }
}
