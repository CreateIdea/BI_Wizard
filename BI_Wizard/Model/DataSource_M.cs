using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Xml.Serialization;
using Microsoft.SqlServer.Management.Common;
using BI_Wizard.Helper;
using Microsoft.SqlServer.Management.Smo;

namespace BI_Wizard.Model
{
    [Serializable]
    public class DataSource_M : BaseData_M
    {
        private string _DsDbServerName = "";
        private string _DsDbName="";
        private Boolean _DsIntegratedSecurity = true;
        private string _DsUserId;
        private string _DsPassword;

        public string DSConnectionString;

        [NonSerialized]
        private Server _DsServer;

        [XmlIgnore]
        [NonSerialized]
        protected Database _DsDb;

        [XmlIgnore]
        [NonSerialized]
        public ICollectionView TableViewListView;

        private TableViewList_M _DsTableViewList;

        public void NotifyDsTableViewList()
        {
            NotifyPropertyChanged("DsTableViewList");
        }
        public TableViewList_M DsTableViewList
        {
            get {return _DsTableViewList; }
            set
            {
                if (SetProperty(ref _DsTableViewList, value))
                {
                    if (value == null)
                    {
                        TableViewListView = null;
                    }
                    else
                    {
                        TableViewListView = CollectionViewSource.GetDefaultView(_DsTableViewList);
                    }

                }
            }
        }

        private void PolulateTableViewList()
        {
            AppendLogLine("Loading Source table and view info...");
            if (DsTableViewList == null)
            {
                DsTableViewList = new TableViewList_M();
                if (_DsDb != null)
                {
                    foreach (Table aTable in _DsDb.Tables.Cast<Table>().Where(t => !t.IsSystemObject))
                    {
                        DsTableViewList.Add(new TableView_M
                        {
                            Schema = aTable.Schema,
                            Name = aTable.Name,
                            IsTable = true,
                            ObjectId = aTable.ID,
                            Urn = aTable.Urn,
                            RowCount = aTable.RowCount
                        });
                    }
                    AppendLogLine(string.Format("Loaded {0} Tables.", _DsTableViewList.Count));
                    int tableCount = _DsTableViewList.Count;
                    foreach (
                        Microsoft.SqlServer.Management.Smo.View aView in
                            _DsDb.Views.Cast<Microsoft.SqlServer.Management.Smo.View>().Where(t => !t.IsSystemObject))
                    {
                        _DsTableViewList.Add(new TableView_M
                        {
                            Schema = aView.Schema,
                            Name = aView.Name,
                            IsTable = false,
                            ObjectId = aView.ID,
                            Urn = aView.Urn,
                            RowCount = -1
                        });
                    }
                    AppendLogLine(string.Format("Loaded {0} Views.", _DsTableViewList.Count - tableCount));
                    AppendLogLine("");
                }
                NotifyPropertyChanged("DsTableViewList");
            }
            else
            {
                AppendLogLine(string.Format("Skip reloading source table and view info."));
                AppendLogLine("");
            }
        }


        public string DsDbServerName
        {
            get { return _DsDbServerName; }
            set { SetProperty(ref _DsDbServerName, value); }
        }

        public string DsDbName
        {
            get { return _DsDbName; }
            set { SetProperty(ref _DsDbName, value); }
        }

        public bool DsIntegratedSecurity
        {
            get { return _DsIntegratedSecurity; }
            set { SetProperty(ref _DsIntegratedSecurity, value); }
        }

        public string DsUserId
        {
            get { return _DsUserId; }
            set { SetProperty(ref _DsUserId, value); }
        }

        public string DsPassword
        {
            get { return _DsPassword; }
            set { SetProperty(ref _DsPassword, value); }
        }

        private void InitializeServer()
        {
            //_DsServer.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject", "IsEncrypted");
            _DsServer.SetDefaultInitFields(typeof(Table), "IsSystemObject", "Schema", "Name", "ID", "Urn", "RowCount");
            _DsServer.SetDefaultInitFields(typeof(Microsoft.SqlServer.Management.Smo.View), "IsSystemObject", "Schema", "Name", "ID", "Urn");
            //_DsServer.SetDefaultInitFields(typeof(UserDefinedFunction), "IsSystemObject", "IsEncrypted");
            _DsServer.ConnectionContext.SqlExecutionModes = SqlExecutionModes.CaptureSql;
            _DsDb = _DsServer.Databases[DsDbName];
            if (_DsDb == null)
            {
                AppendLogLine(String.Format("Database {0} not found on server {1}", DsDbName, _DsServer.Name));
            }

            if (DsTableViewList == null)
            {
                ScriptingOptions scriptingOptions = new ScriptingOptions();
                scriptingOptions.ExtendedProperties = true;
                _DsDb.PrefetchObjects(typeof (Table), scriptingOptions);
            }
            PolulateTableViewList();
        }

        //string dbServerName, string dbName, bool integratedSecurity, string userId, string password)
        public bool DsConnectToDataSource()
        {
            AppendLogLine("Connecting to Source SQL Server and Database...");
            bool res = ConnectToSqlServer(DsDbServerName, DsDbName, DsIntegratedSecurity, DsUserId, DsPassword, ref _DsServer, out DSConnectionString);
            if (res)
            {
                InitializeServer();
            }
            return res;
        }

    }
}
