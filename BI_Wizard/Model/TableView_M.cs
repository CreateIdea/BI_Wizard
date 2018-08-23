using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using BI_Wizard.Helper;

namespace BI_Wizard.Model
{
    [Serializable]
    public class TableView_M : Base_VM
    {
        private string _Schema;
        private string _Name;
        private Boolean _IsTable;
        private int _ObjectId;
        private Urn _Urn;
        private bool _IsSelected;

        private long _RowCount;
        public string Schema
        {
            get { return _Schema; }
            set { SetProperty(ref _Schema, value); }
        }

        public string Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }

        public bool IsTable
        {
            get { return _IsTable; }
            set { SetProperty(ref _IsTable, value); }
        }

        public int ObjectId
        {
            get { return _ObjectId; }
            set { SetProperty(ref _ObjectId, value); }
        }

        public Urn Urn
        {
            get { return _Urn; }
            set { SetProperty(ref _Urn, value); }
        }

        public long RowCount
        {
            get { return _RowCount; }
            set { SetProperty(ref _RowCount, value); }
        }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref  _IsSelected, value); }
        }
    }


    public class TableViewList_M : ObservableCollection<TableView_M>
    { }

}
