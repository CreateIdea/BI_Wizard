using BI_Wizard.Helper;
using BI_Wizard.View;
using BI_Wizard.ViewModel;

namespace BI_Wizard.View
{
    /// <summary>
    /// Interaction logic for Page_01_DS_V.xaml
    /// </summary>
    public partial class Page_01_Ds_V : MgaXctkWizardPage
    {
        public Page_01_Ds_Vm ViewModel;
        public Page_01_Ds_V()
        {
            InitializeComponent();
            ViewModel = (Page_01_Ds_Vm) DataContext;
        }

        public override void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            e.Cancel = !ViewModel.AnalysisM.DsConnectToDataSource();
        }
    }
}
