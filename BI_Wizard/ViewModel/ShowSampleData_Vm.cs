using System.Data;
using System.Data.SqlClient;
using BI_Wizard.Model;

namespace BI_Wizard.ViewModel
{
    public class ShowSampleData_Vm:BaseData_M
    {
        public Analysis_M AnalysisM
        {
            get { return App_M.Instance.AnalysisM; }
        }

        public DataView TableViewDataView
        {
            get { return _TableViewDataView; }
            set { SetProperty(ref _TableViewDataView, value); }
        }

        public string Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value); }
        }

        private DataView _TableViewDataView;

        private string _Title;

        public void LoadSampleData(string tableName, bool isTable)
        {
            DataView tableViewDataView;
            string title;
            AnalysisM.LoadSampleData(tableName, AnalysisM.DSConnectionString, isTable, out tableViewDataView, out title);
            TableViewDataView = tableViewDataView;
            Title = title;
        }
    }
}
