using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Microsoft.SqlServer.Management.Smo;
using BI_Wizard.Helper;

namespace BI_Wizard.Model
{
    [Serializable]
    public class DataWarehouse_M : DataSource_M
    {
        private bool _DwSkipSelectSourceTables = false;

        private DsDwMap_M _DsDwMap;
        private int _DwDataSizeMb=200;
        [XmlIgnore]
        protected  Database _DwDb;
        private string _DwDbName;
        private string _DwDbServerName;
        private DateTime _DwEndDateTable;
        private bool _DwGenerateTimeTable;
        private Boolean _DwIntegratedSecurity = true;
        private int _DwLogSizeMb=200;
        private string _DwPassword;

        [XmlIgnore]
        protected Server _DwServer;
        private DateTime _DwStartDateTable;
        private TableViewList_M _DwTableViewList = new TableViewList_M();
        private string _DwUserId;
        private string _DwDataFileName;
        private string _DwLogFileName;
        private bool _DwRebuildDsDwTableMap;
        private bool _DsDwMapChanged=false;


        public string DwConnectionString;

        public TableViewList_M DwTableViewList
        {
            get { return _DwTableViewList; }
            set { SetProperty(ref _DwTableViewList, value); }
        }

        public string DwDbServerName
        {
            get { return _DwDbServerName; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                    value = value.Trim();
                SetProperty(ref _DwDbServerName, value);
            }
        }

        public string DwDbName
        {
            get { return _DwDbName; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                    value = value.Trim();
                SetProperty(ref _DwDbName, value);
            }
        }

        public bool DwIntegratedSecurity
        {
            get { return _DwIntegratedSecurity; }
            set { SetProperty(ref _DwIntegratedSecurity, value); }
        }

        public string DwUserId
        {
            get { return _DwUserId; }
            set { SetProperty(ref _DwUserId, value); }
        }

        public string DwPassword
        {
            get { return _DwPassword; }
            set { SetProperty(ref _DwPassword, value); }
        }

        public DsDwMap_M DsDwMap
        {
            get { return _DsDwMap; }
            set { SetProperty(ref _DsDwMap, value); }
        }

        public bool DwGenerateTimeTable
        {
            get { return _DwGenerateTimeTable; }
            set { SetProperty(ref _DwGenerateTimeTable, value); }
        }

        public DateTime DwStartDateTable
        {
            get { return _DwStartDateTable; }
            set { SetProperty(ref _DwStartDateTable, value); }
        }

        public DateTime DwEndDateTable
        {
            get { return _DwEndDateTable; }
            set { SetProperty(ref _DwEndDateTable, value); }
        }

        public int DwDataSizeMb
        {
            get { return _DwDataSizeMb; }
            set { SetProperty(ref _DwDataSizeMb, value); }
        }

        public int DwLogSizeMb
        {
            get { return _DwLogSizeMb; }
            set { SetProperty(ref _DwLogSizeMb, value); }
        }

        public bool DwRebuildDsDwTableMap
        {
            get { return _DwRebuildDsDwTableMap; }
            set { SetProperty(ref _DwRebuildDsDwTableMap, value); }
        }

        [XmlIgnore]
        public bool DsDwMapChanged
        {
            get { return _DsDwMapChanged; }
            set { SetProperty(ref _DsDwMapChanged ,value); }
        }

        public bool DwSkipSelectSourceTables
        {
            get { return _DwSkipSelectSourceTables; }
            set { SetProperty(ref _DwSkipSelectSourceTables, value); }
        }

        protected Database DwDb
        {
            get
            {
                if (_DwDb == null && _DwServer!=null)
                {
                    _DwDb = _DwServer.Databases[DwDbName];
                }
                return _DwDb;
            }
            set { _DwDb = value; }
        }

        public bool SycDsDwTableViewList()
        {
            bool res=false;
            if ((DsTableViewList != null) && (DwTableViewList != null) && (DsDwMap != null) &&
                (DsDwMap.DsDwTableMapList != null))
            {
                foreach (TableView_M aTableViewM in DwTableViewList)
                {
                    DsTableViewList.RemoveAll(tv => tv.ObjectId == aTableViewM.ObjectId);
                }

                foreach (TableView_M aTableViewM in DsTableViewList)
                {
                    DwTableViewList.RemoveAll(tv => tv.ObjectId == aTableViewM.ObjectId);
                    res = res || DsDwMap.DsDwTableMapList.RemoveAll(tv => tv.DsObjectId == aTableViewM.ObjectId) > 0;
                }
            }
            DsDwMapChanged = _DsDwMapChanged || res;
            return res;
        }

        public bool SycDwTableViewMapList()
        {
            bool res=false;
            if ((DwTableViewList != null) && (DsDwMap != null) && (DsDwMap.DsDwTableMapList != null))
            {
                res =
                    DsDwMap.DsDwTableMapList.RemoveAll(tm => !DwTableViewList.Any(tv => tv.ObjectId == tm.DsObjectId)) >
                    0;
            }
            DsDwMapChanged = _DsDwMapChanged || res;
            return res;
        }

        private void InitializeServer()
        {
            //_DwServer.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject", "IsEncrypted");
            _DwServer.SetDefaultInitFields(typeof(Table), "IsSystemObject");
            _DwServer.SetDefaultInitFields(typeof(Microsoft.SqlServer.Management.Smo.View), "IsSystemObject");
            //_DwServer.SetDefaultInitFields(typeof(UserDefinedFunction), "IsSystemObject", "IsEncrypted");
            SycDsDwTableViewList();
            SycDwTableViewMapList();
        }

        //string dbServerName, string dbName, bool integratedSecurity, string userId, string password)
        public bool DwConnectToDataSource()
        {
            AppendLogLine("Connecting to Data WareHouse SQL Server...");
            bool res = ConnectToSqlServer(DwDbServerName, "master", DwIntegratedSecurity, DwUserId, DwPassword,
                ref _DwServer, out DwConnectionString);
            if (res)
            {
                InitializeServer();
            }
            return res;
        }

        private void AddDwSupportColumns(DsDwTableMap_M aDsDwTableMap)
        {
            //Add Data warehouse support columns
            //[Description("Scd 2 is active")] Scd2IsActive,
            //[Description("Scd 2 date from")] Scd2DateFrom,
            //[Description("Scd 2 date to")] Scd2DateTo,
            //[Description("Surrogate key")] SurrogateKey,
            //[Description("Created date")] CreatedDate,
            //[Description("Deleted date")] DeletedDate,
            //[Description("Modified date")] ModifiedDate,
            //[Description("Is deleted")] IsDeleted

            DsDwColumnMap_M aDsDwColumnMap;
            aDsDwColumnMap = new DsDwColumnMap_M
            {
                DsColumn = null,
                DsReferencedColumn = null,
                DwColumn = new DsDwColumn_M { Name = "Scd2IsActive", DataType = new DataType(SqlDataType.Bit) },
                Transformation = DsDwColumnTransformation.Scd2IsActive,
                Include = false
            };
            aDsDwTableMap.DsDwColumnMapList.Add(aDsDwColumnMap);

            aDsDwColumnMap = new DsDwColumnMap_M
            {
                DsColumn = null,
                DsReferencedColumn = null,
                DwColumn = new DsDwColumn_M { Name = "Scd2DateFrom", DataType = new DataType(SqlDataType.DateTime2) },
                Transformation = DsDwColumnTransformation.Scd2DateFrom,
                Include = false
            };
            aDsDwTableMap.DsDwColumnMapList.Add(aDsDwColumnMap);

            aDsDwColumnMap = new DsDwColumnMap_M
            {
                DsColumn = null,
                DsReferencedColumn = null,
                DwColumn = new DsDwColumn_M { Name = "Scd2DateTo", DataType = new DataType(SqlDataType.DateTime2) },
                Transformation = DsDwColumnTransformation.Scd2DateTo,
                Include = false
            };
            aDsDwTableMap.DsDwColumnMapList.Add(aDsDwColumnMap);

            aDsDwColumnMap = new DsDwColumnMap_M
            {
                DsColumn = null,
                DsReferencedColumn = null,
                DwColumn = new DsDwColumn_M { Name = aDsDwTableMap.DwTableName + "_Key", DataType = new DataType(SqlDataType.Int) },

                Transformation = DsDwColumnTransformation.SurrogateKey,
                Include = true
            };
            aDsDwTableMap.DsDwColumnMapList.Insert(0, aDsDwColumnMap);

            aDsDwColumnMap = new DsDwColumnMap_M
            {
                DsColumn = null,
                DsReferencedColumn = null,
                DwColumn = new DsDwColumn_M { Name = "CreatedDate", DataType = new DataType(SqlDataType.DateTime2) },
                Transformation = DsDwColumnTransformation.CreatedDate,
                Include = true
            };
            aDsDwTableMap.DsDwColumnMapList.Add(aDsDwColumnMap);

            aDsDwColumnMap = new DsDwColumnMap_M
            {
                DsColumn = null,
                DsReferencedColumn = null,
                DwColumn = new DsDwColumn_M { Name = "DeletedDate", DataType = new DataType(SqlDataType.DateTime2) },
                Transformation = DsDwColumnTransformation.DeletedDate,
                Include = true
            };
            aDsDwTableMap.DsDwColumnMapList.Add(aDsDwColumnMap);

            aDsDwColumnMap = new DsDwColumnMap_M
            {
                DsColumn = null,
                DsReferencedColumn = null,
                DwColumn = new DsDwColumn_M { Name = "ModifiedDate", DataType = new DataType(SqlDataType.DateTime2) },
                Transformation = DsDwColumnTransformation.ModifiedDate,
                Include = true
            };
            aDsDwTableMap.DsDwColumnMapList.Add(aDsDwColumnMap);

            aDsDwColumnMap = new DsDwColumnMap_M
            {
                DsColumn = null,
                DsReferencedColumn = null,
                DwColumn = new DsDwColumn_M { Name = "IsDeleted", DataType = new DataType(SqlDataType.Bit) },
                Transformation = DsDwColumnTransformation.IsDeleted,
                Include = true
            };
            aDsDwTableMap.DsDwColumnMapList.Add(aDsDwColumnMap);
        }


        protected void DwGenerateTables()
        {
            if (DwDb != null && DsDwMap != null && DsDwMap.DsDwTableMapList != null)
            {
                foreach (DsDwTableMap_M aDwTableMap in DsDwMap.DsDwTableMapList)
                {
                    DwGenerateTable(aDwTableMap);
                }
                foreach (DsDwTableMap_M aDwTableMap in DsDwMap.DsDwTableMapList)
                {
                    if (aDwTableMap != null)
                    {
                        foreach (DsDwColumnMap_M aDsDwColumnMap in aDwTableMap.DsDwColumnMapList.Where(cm => cm != null &&
                            cm.Include && cm.Transformation == DsDwColumnTransformation.ForeignKey))
                        {
                            DwGenerateForeignKey(aDsDwColumnMap);
                        }
                    }
                }
            }
            AppendLogLine("Data warehouse database tables created successfully!");
        }

        private void DwGenerateForeignKey(DsDwColumnMap_M aDsDwColumnMap)
        {
            if (aDsDwColumnMap != null && aDsDwColumnMap.Parent != null &&
                aDsDwColumnMap.DwForeignKeyReferencedTableMap != null &&
                !string.IsNullOrWhiteSpace(aDsDwColumnMap.DwForeignKeyReferencedTableMap.DwTableName))
            {
                Table tbea;
                tbea =
                    DwDb.Tables.Cast<Table>()
                        .FirstOrDefault(tb => tb.Name.ToLower() == aDsDwColumnMap.Parent.DwTableName.ToLower() &&
                                              tb.Schema.ToLower() == aDsDwColumnMap.Parent.DwSchemaName.ToLower());

                DsDwTableMap_M aDwFkReferencedTableMap = aDsDwColumnMap.DwForeignKeyReferencedTableMap;
                    //DsDwMap.DsDwTableMapList.FirstOrDefault(
                    //    tm =>
                    //        tm.DwTableName.ToLower() == aDsDwColumnMap.DwForeignKeyReferencedTableMap.DwTableName.ToLower() &&
                    //        tm.DwSchemaName.ToLower() == aDsDwColumnMap.DwForeignKeyReferencedTableMap.DwSchemaName.ToLower());

                DsDwColumnMap_M aDsDwPrimaryKeyColumnMap=null;
                if (aDwFkReferencedTableMap != null)
                {
                    aDsDwPrimaryKeyColumnMap =
                        aDwFkReferencedTableMap.DsDwColumnMapList.FirstOrDefault(
                            cm => cm.Transformation == DsDwColumnTransformation.SurrogateKey);
                }
                if (tbea != null && aDsDwPrimaryKeyColumnMap != null)
                {
                    //Define a Foreign Key object variable by supplying the EmployeeDepartmentHistory as the parent table and the foreign key name in the constructor. 
                    ForeignKey fk;
                    fk = new ForeignKey(tbea, string.Format("FK_{0}_{1}", aDsDwColumnMap.Parent.DwTableName, aDsDwColumnMap.DwForeignKeyReferencedTableMap.DwTableName));
                    //Add BusinessEntityID as the foreign key column. 
                    ForeignKeyColumn fkc;
                    fkc = new ForeignKeyColumn(fk, aDsDwColumnMap.DwColumn.Name, aDsDwPrimaryKeyColumnMap.DwColumn.Name);
                    fk.Columns.Add(fkc);
                    //Set the referenced table and schema. 
                    fk.ReferencedTable = aDsDwColumnMap.DwForeignKeyReferencedTableMap.DwTableName;
                    fk.ReferencedTableSchema = aDsDwColumnMap.DwForeignKeyReferencedTableMap.DwSchemaName;
                    //Create the foreign key on the instance of SQL Server. 
                    fk.Create();
                }
            }
        }

        private void DwGenerateTable(DsDwTableMap_M aDsDwTableMap)
        {
            if (aDsDwTableMap != null && aDsDwTableMap.DsDwColumnMapList != null)
            {
                Table aSMOTable = new Table(DwDb, aDsDwTableMap.DwTableName, aDsDwTableMap.DwSchemaName);
                //Add columns to the table
                foreach (DsDwColumnMap_M aDsDwColumnMap in aDsDwTableMap.DsDwColumnMapList)
                {
                    if (aDsDwColumnMap != null && aDsDwColumnMap.Include)
                    {
                        Column aNewColumn;
                        switch (aDsDwColumnMap.Transformation)
                        {
                            case DsDwColumnTransformation.SurrogateKey:
                                aNewColumn = new Column(aSMOTable, aDsDwColumnMap.DwColumn.Name, aDsDwColumnMap.DwColumn.DataType);
                                aNewColumn.Nullable = false;
                                aNewColumn.Identity = true;
                                aNewColumn.IdentityIncrement = 1;
                                aNewColumn.IdentitySeed = 1;
                                aSMOTable.Columns.Add(aNewColumn);
                                break;
                            case DsDwColumnTransformation.BusinesKey:
                                aNewColumn = new Column(aSMOTable, aDsDwColumnMap.DwColumn.Name, aDsDwColumnMap.DwColumn.DataType);
                                aNewColumn.Nullable = false;
                                aNewColumn.Collation = aDsDwColumnMap.DwColumn.Collation;
                                aSMOTable.Columns.Add(aNewColumn);
                                break;
                            default:
                                aNewColumn = new Column(aSMOTable, aDsDwColumnMap.DwColumn.Name, aDsDwColumnMap.DwColumn.DataType);
                                aNewColumn.Collation = aDsDwColumnMap.DwColumn.Collation;
                                aNewColumn.Nullable = true;
                                aSMOTable.Columns.Add(aNewColumn);
                                break;
                        }
                    }
                }

                aSMOTable.Create();
                //Add indexes and primary key the table
                foreach (DsDwColumnMap_M aDsDwColumnMap in aDsDwTableMap.DsDwColumnMapList)
                {
                    if (aDsDwColumnMap != null && aDsDwColumnMap.Include)
                    {
                        switch (aDsDwColumnMap.Transformation)
                        {
                            case DsDwColumnTransformation.SurrogateKey:
                                Index primaryKey = new Index(aSMOTable, "PK_" + aDsDwTableMap.DwTableName);
                                IndexedColumn indexedColumn = new IndexedColumn(primaryKey, aDsDwColumnMap.DwColumn.Name);
                                primaryKey.IndexedColumns.Add(indexedColumn);
                                primaryKey.IndexKeyType = IndexKeyType.DriPrimaryKey;
                                primaryKey.Create();
                                break;
                            case DsDwColumnTransformation.BusinesKey:
                                // Define an Index object variable by providing the parent table and index name in the constructor. 
                                Index idx;
                                idx = new Index(aSMOTable, "IX_" + aDsDwTableMap.DwTableName + "_" + aDsDwColumnMap.DwColumn.Name);
                                // Add indexed columns to the index. 
                                IndexedColumn icol1;
                                icol1 = new IndexedColumn(idx, aDsDwColumnMap.DwColumn.Name, true);
                                idx.IndexedColumns.Add(icol1);
                                // Set the index properties. 
                                idx.IndexKeyType = IndexKeyType.None;
                                idx.IsClustered = false;
                                idx.FillFactor = 70;
                                // Create the index on the instance of SQL Server. 
                                idx.Create();
                                break;
                        }
                    }
                }
            }
        }

        protected void DwGenerateDb()
        {
            AppendLogLine(string.Format("Creating data warehouse database {0} on server {1}...", DwDbName, DwDbServerName));
            _DwServer.Databases.Refresh();
            DwDb = _DwServer.Databases[DwDbName];
            if (DwDb != null)
            {
                if (MessageBox.Show(
                    string.Format("The database '{0} - {1}' already exists. Do you want to drop it?",
                        DwDbServerName, DwDbName),
                    "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DwDb.Drop();
                }
                else
                {
                    throw new Exception(string.Format(
                        "The database '{0} - {1}' already exists, but user did not want to drop it. Aborting procedure.",
                        DwDbServerName, DwDbName));
                }
            }
            DwDb = new Database(_DwServer, DwDbName);
            GetDwFileNames();
            DwDb.FileGroups.Add(new FileGroup(DwDb, "PRIMARY"));
            DataFile dtPrimary = new DataFile(DwDb.FileGroups["PRIMARY"], DwDbName, _DwDataFileName);
            dtPrimary.Size = DwDataSizeMb * 1024.0;
            dtPrimary.GrowthType = FileGrowthType.KB;
            dtPrimary.Growth = 500.0 * 1024.0;
            DwDb.FileGroups["PRIMARY"].Files.Add(dtPrimary);

            LogFile logFile = new LogFile(DwDb, DwDbName + "_Log", _DwLogFileName);
            logFile.Size = DwLogSizeMb * 1024.0;
            logFile.GrowthType = FileGrowthType.KB;
            logFile.Growth = 500.0 * 1024.0;
            DwDb.LogFiles.Add(logFile);

            DwDb.Create();
            DwDb.Refresh();
            AppendLogLine("Data warehouse database created successfully!");
            AppendLogLine("");
        }

        private void GetDwFileNames()
        {
            if (_DwServer.Settings.DefaultFile == string.Empty)
                _DwDataFileName = _DwServer.Information.MasterDBPath + "\\" + DwDbName + ".mdf";
            else
                _DwDataFileName = _DwServer.Settings.DefaultFile + "\\" + DwDbName + ".mdf";

            if (_DwServer.Settings.DefaultLog == string.Empty)
                _DwLogFileName = _DwServer.Information.MasterDBLogPath + "\\" + DwDbName + "_log.ldf";
            else
                _DwLogFileName = _DwServer.Settings.DefaultLog + "\\" + DwDbName + "_log.ldf";
        }

        private void AddExistingForeignKeysTable(Table aTable, DsDwTableMap_M aDsDwTableMap)
        {
            foreach (ForeignKey aForeignKey in aTable.ForeignKeys)
            {
                aDsDwTableMap.DsDwForeignKeyList.Add(new DsDwForeignKey_M(aForeignKey));
            }
        }

        public Boolean DwBuildActiveDepenencyLists()
        {
            if (DsDwMap != null)
            {
                AppendLogLine("Rebuilding Active Depenency Lists...");
                AppendLogLine(string.Format("Total Dependency Links:{0}", DsDwMap.BuildActiveDepenencyLists()));
                return true;
            }
            else
            {
                return false;
            }
        }

        public void InitializeDsDwMap()
        {
            if (DwRebuildDsDwTableMap)
            {
                AppendLogLine("Rebuilding table map...");
                DsDwMapChanged = true;
                DsDwMap = null;
            }
            if (DsDwMap == null)
            {
                DsDwMap = new DsDwMap_M();
            }
            else
            {
                SycDwTableViewMapList();
            }
            foreach (TableView_M aTableView in DwTableViewList)
            {
                if ((aTableView != null) && (!DsDwMap.DsDwTableMapList.Any(tm => tm.DsObjectId == aTableView.ObjectId)))
                {
                    DsDwMapChanged = true;
                    AppendLogLine(string.Format("Processing table/view: {0}...", aTableView.Name));
                    DsDwTableMap_M aDsDwTableMap = new DsDwTableMap_M
                    {
                        DsIsTable = aTableView.IsTable,
                        DsTableName = aTableView.Name,
                        DsSchemaName = aTableView.Schema,
                        DwTableName = aTableView.Name,
                        DwSchemaName = aTableView.Schema,
                        DsRowCount = aTableView.RowCount,
                        DsObjectId = aTableView.ObjectId,
                        DsDatabaseName = DsDbName,
                        DsServerName = DsDbServerName
                    };
                    DsDwMap.DsDwTableMapList.Add(aDsDwTableMap);

                    TableViewBase aTableViewBase;
                    if (aTableView.IsTable)
                    {
                        aTableViewBase = _DsDb.Tables.Cast<Table>().FirstOrDefault(t => t.ID == aTableView.ObjectId);
                    }
                    else
                    {
                        aTableViewBase =
                            _DsDb.Views.Cast<Microsoft.SqlServer.Management.Smo.View>()
                                .FirstOrDefault(t => t.ID == aTableView.ObjectId);
                    }

                    if (aTableViewBase != null)
                    {
                        foreach (Column aColumn in aTableViewBase.Columns)
                        {
                            DsDwColumnMap_M aDsDwColumnMap = new DsDwColumnMap_M
                            {
                                DsColumn = new DsDwColumn_M(aColumn),
                                DwColumn = new DsDwColumn_M(aColumn),
                                Include = true
                            };

                            if (aColumn.InPrimaryKey)
                            {
                                aDsDwColumnMap.Transformation = DsDwColumnTransformation.BusinesKey;
                                aDsDwTableMap.DsDwColumnMapList.Insert(0, aDsDwColumnMap);
                            }
                            else
                            {
                                aDsDwColumnMap.Transformation = DsDwColumnTransformation.Scd1;
                                aDsDwTableMap.DsDwColumnMapList.Add(aDsDwColumnMap);
                            }

                        }
                        if (aTableView.IsTable)
                        {
                            AddExistingForeignKeysTable((Table) aTableViewBase, aDsDwTableMap);
                        }

                        if (!aTableView.IsTable) //this is a view
                        {
                            aTableViewBase =
                                _DsDb.Views.Cast<Microsoft.SqlServer.Management.Smo.View>()
                                    .FirstOrDefault(t => t.ID == aTableView.ObjectId);

                            ParseViewSql aParseViewSql = new ParseViewSql();

                            Microsoft.SqlServer.Management.Smo.View aView =
                                (aTableViewBase as Microsoft.SqlServer.Management.Smo.View);

                            if (aParseViewSql.ParseView(aView.TextHeader + aView.TextBody))
                            {
                                FillColumnAndForeignKeyInfoView(aParseViewSql.ColumnList, aDsDwTableMap);
                            }
                        }
                        AddDwSupportColumns(aDsDwTableMap);
                    }
                }
                else
                {
                    //Update DbName and ServerName
                    DsDwTableMap_M aUpdateDsDwTableMap = DsDwMap.DsDwTableMapList.FirstOrDefault(tm => tm.DsObjectId == aTableView.ObjectId);
                    if (aUpdateDsDwTableMap != null)
                    {
                        aUpdateDsDwTableMap.DsDatabaseName = DsDbName;
                        aUpdateDsDwTableMap.DsServerName = DsDbServerName;
                    }
                }
            }
            if (DsDwMapChanged || true) 
            {
                AppendLogLine(string.Format("Always rebuilding foreign key links."));
                DsDwMap.ConfigureForeignKeyTransformations();
            }
            else
            {
                AppendLogLine(string.Format("Map not changed, not rebuilding foreign key links."));
            }
            DsDwMapChanged = false;
            NotifyPropertyChanged("DsDwMap");
        }



        private void FillColumnAndForeignKeyInfoView(ObservableCollection<ColumnInfo> aParsedColumnList, DsDwTableMap_M aDsDwTableMap)
        {
            if (aDsDwTableMap != null && aParsedColumnList != null && aParsedColumnList.Count > 0)
            {
                Table aTable;
                ForeignKey aForeignKey;
                DsDwColumnMap_M aDsDwColumnMap;
                foreach (ColumnInfo aColumnInfo in aParsedColumnList)
                {
                    if (string.IsNullOrWhiteSpace(aColumnInfo.TableSchema))
                    {
                        aColumnInfo.TableSchema = _DsDb.DefaultSchema.ToString();
                    }
                    aDsDwColumnMap =
                        aDsDwTableMap.DsDwColumnMapList.FirstOrDefault(
                            cm => cm.DsColumn.Name.ToLower() == aColumnInfo.Alias.ToLower());
                    if (aDsDwColumnMap != null)
                    {
                        aDsDwColumnMap.DsReferencedTable = new DsDwTableReference_M
                        {
                            TableName = aColumnInfo.TableName,
                            SchemaName = aColumnInfo.TableSchema,
                            DatabaseName = aColumnInfo.TableDatabase,
                            ServerName = aColumnInfo.TableServer
                        };

                        aTable = _DsDb.Tables.Cast<Table>().FirstOrDefault(t =>
                            t.Name.ToLower() == aColumnInfo.TableName.ToLower() &&
                            t.Schema.ToLower() == aColumnInfo.TableSchema.ToLower());
                        if (aTable != null)
                        {
                            aDsDwColumnMap.DsReferencedColumn = new DsDwColumn_M(
                                aTable.Columns.Cast<Column>().
                                    FirstOrDefault(c => c.Name.ToLower() == aColumnInfo.TableColumnName.ToLower()));
                            if (aDsDwColumnMap.DsReferencedColumn.IsForeignKey)
                            {
                                aForeignKey =
                                    aTable.ForeignKeys.Cast<ForeignKey>()
                                        .FirstOrDefault(
                                            fk =>
                                                fk.Columns.Cast<ForeignKeyColumn>()
                                                    .Any(c => c.Name.ToLower() == aDsDwColumnMap.DsReferencedColumn.Name.ToLower()));
                                if (aForeignKey != null)
                                {
                                    aDsDwTableMap.DsDwForeignKeyList.Add(new DsDwForeignKey_M(aForeignKey));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}