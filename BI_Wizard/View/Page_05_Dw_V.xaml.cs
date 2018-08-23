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
    /// Interaction logic for Page_05_DW_V.xaml
    /// </summary>
    public partial class Page_05_Dw_V : MgaXctkWizardPage
    {
        public  Page_05_Dw_Vm ViewModel;
        public Page_05_Dw_V()
        {
            InitializeComponent();
            ViewModel = (Page_05_Dw_Vm) DataContext;
        }

        public override void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
           // e.Cancel = !(ViewModel.AnalysisM.DwTableViewList.Count > 0);
        }

    }
}
