using System;
using System.Windows.Controls;
using BI_Wizard.Helper;
using BI_Wizard.Model;
using BI_Wizard.View;
using BI_Wizard.ViewModel;

namespace BI_Wizard.View
{
    /// <summary>
    /// Interaction logic for Page_03_DS_V.xaml
    /// </summary>
    public partial class Page_03_Ds_V : MgaXctkWizardPage
    {
        public Page_03_Ds_Vm ViewModel;
        public Page_03_Ds_V()
        {
            InitializeComponent();
            ViewModel = (Page_03_Ds_Vm) DataContext;
        }

        public override void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            e.Cancel = !(ViewModel.AnalysisM.DwTableViewList.Count > 0);
            if (!e.Cancel)
            {
                ViewModel.AnalysisM.InitializeDsDwMap();
            }
        }

        private void UseInDw_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.MoveSelectedToDw();
        }

        private void DoNotUseInDw_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.MoveSelectedToDs();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if ((dataGrid != null) && (dataGrid.SelectedItem != null))
            {
                TableView_M aTableViewM = dataGrid.SelectedItem as TableView_M;
                if (aTableViewM != null)
                {
                    ShowSampleData_V iplWindow = new ShowSampleData_V();
                    iplWindow.ViewModel.LoadSampleData(string.Format("[{0}].[{1}]", aTableViewM.Schema, aTableViewM.Name), aTableViewM.IsTable);
                    //iplWindow.Owner = this.parent;
                    Nullable<Boolean> res = iplWindow.ShowDialog();
                }
            }
        }
    }
}
