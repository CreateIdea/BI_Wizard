using BI_Wizard.Helper;
using BI_Wizard.View;
using BI_Wizard.ViewModel;

namespace BI_Wizard.View
{
    /// <summary>
    /// Interaction logic for Page_02_DW_V.xaml
    /// </summary>
    public partial class Page_02_Dw_V : MgaXctkWizardPage
    {
        public Page_02_Dw_Vm ViewModel;
        public Page_02_Dw_V()
        {
            InitializeComponent();
            ViewModel = (Page_02_Dw_Vm) DataContext;
        }

        public override void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            e.Cancel = !ViewModel.AnalysisM.DwConnectToDataSource();
            if (!e.Cancel && ViewModel.AnalysisM.DwSkipSelectSourceTables)
            {
                ViewModel.AnalysisM.InitializeDsDwMap();
            }
        }
    }
}
