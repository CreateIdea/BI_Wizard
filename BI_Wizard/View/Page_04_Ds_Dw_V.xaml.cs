using System;
using System.Windows.Controls;
using System.Windows.Input;
using BI_Wizard.Helper;
using BI_Wizard.Model;
using BI_Wizard.View;
using BI_Wizard.ViewModel;

namespace BI_Wizard.View
{
    /// <summary>
    /// Interaction logic for Page_04_DS_DW_V.xaml
    /// </summary>
    public partial class Page_04_Ds_Dw_V : MgaXctkWizardPage
    {
        public  Page_04_Ds_Dw_Vm ViewModel;
        public Page_04_Ds_Dw_V()
        {
            InitializeComponent();
            ViewModel = (Page_04_Ds_Dw_Vm) DataContext;
        }

        public override void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            e.Cancel =  !ViewModel.AnalysisM.DwBuildActiveDepenencyLists();
        }

        private void IncludeAllDw_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.IncludeAllDw();
        }

        private void IncludeNone_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.IncludeNoneDw();
        }


    }
}
