using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.AnalysisServices;
using Microsoft.SqlServer.Management.Smo;
using Database = Microsoft.AnalysisServices.Database;
using Server = Microsoft.AnalysisServices.Server;

namespace BI_Wizard.Model
{
    [Serializable]
    public class Cube_M : TransformationGenerator_M, IDisposable
    {
        private string _CbProviderName = "MSOLAP";

        private string _DwDsUserName = "";
        private string _DwDsPassword = "";

        private string _CbDbName;
        private string _CbName;
        private string _CbDataSourceName = "OLAPDS";
        private string _CbDataSourceViewName = "OLAPDSView";
        private Boolean _CbIntegratedSecurity = true;
        private string _CbUserId;
        private string _CbPassword;
        private bool _CbRebuildDwCbMap=false;
        private string _CbServerName;

        private DwCbMap_M _DwCbMap=new DwCbMap_M();

        public DwCbMap_M DwCbMap
        {
            get { return _DwCbMap; }
            set { SetProperty(ref _DwCbMap, value); }
        }

        public bool LoadFromDsDwMap()
        {
            if (DsDwMap != null)
            {
                DwCbMap.SyncToDsDwMap(DsDwMap, CbRebuildDwCbMap);
                return true;
            }
            else
            {
                return false;
            }
        }

        [XmlIgnore]
        [NonSerialized]
        protected Server _CbServer;
        [XmlIgnore] 
        [NonSerialized]
        protected Database _CbDatabase;
        [XmlIgnore]
        [NonSerialized]
        protected RelationalDataSource _CbDataSource;
        [XmlIgnore]
        [NonSerialized]
        protected DataSourceView _CbDataSourceView;
        [XmlIgnore]
        [NonSerialized]
        protected DataSet _CbDataSet;
        [XmlIgnore]
        [NonSerialized]
        protected SqlConnection _CbConnection;
        [XmlIgnore]
        [NonSerialized]
        protected OleDbConnection _CBOleConnection;
        [XmlIgnore]
        [NonSerialized]
        protected Cube _CbCube;

        [XmlIgnore]
        public string CbProviderName
        {
            get { return _CbProviderName; }
            set { SetProperty(ref _CbProviderName, value); }
        }

        public string DwDsUserName
        {
            get { return _DwDsUserName; }
            set { SetProperty(ref _DwDsUserName, value); }
        }

        public string DwDsPassword
        {
            get { return _DwDsPassword; }
            set { SetProperty(ref _DwDsPassword, value); }
        }

        public string CbDbName
        {
            get { return _CbDbName; }
            set { SetProperty(ref _CbDbName, value); }
        }

        public string CbDataSourceName
        {
            get { return _CbDataSourceName; }
            set { SetProperty(ref _CbDataSourceName, value); }
        }

        public string CbDataSourceViewName
        {
            get { return _CbDataSourceViewName; }
            set { SetProperty(ref _CbDataSourceViewName, value); }
        }

        public bool CbIntegratedSecurity
        {
            get { return _CbIntegratedSecurity; }
            set { SetProperty(ref _CbIntegratedSecurity, value); }
        }

        public string CbUserId
        {
            get { return _CbUserId; }
            set { SetProperty(ref _CbUserId, value); }
        }

        public string CbPassword
        {
            get { return _CbPassword; }
            set { SetProperty(ref _CbPassword, value); }
        }

        public bool CbRebuildDwCbMap
        {
            get { return _CbRebuildDwCbMap; }
            set { SetProperty(ref _CbRebuildDwCbMap, value); }
        }

        public string CbName
        {
            get { return _CbName; }
            set { SetProperty(ref _CbName, value); }
        }

        public string CbServerName
        {
            get { return _CbServerName; }
            set { SetProperty(ref _CbServerName, value); }
        }

        public bool ConnectToAnalysisServer()
        {
            _CbServer = new Server();
            _CbDatabase = new Database();
            _CbDataSource = new RelationalDataSource();
            _CbDataSourceView = new DataSourceView();
            _CbDataSet = new DataSet();

            //Connecting to the Analysis Services.
            _CbServer = (Server) ConnectAnalysisServices(CbServerName, CbProviderName);
            if (_CbServer != null)
                LoadFromDsDwMap();
            return (_CbServer != null);
        }

        public void ProcessCube()
        {
            AppendLogLine("Start full process Cube.");
            if (_CbDatabase != null)
            {
                _CbDatabase.Process(ProcessType.ProcessFull);
                AppendLogLine("End full process Cube.");
            }
        }

        public void CreateCube()
        {
            try
            {
                AppendLogLine("Cube creation process started.");
                AppendLogLine("");
                if (_CbServer == null)
                {
                    AppendLogLine("No connection to Analysis server, abort create cube.");
                    AppendLogLine("");
                    return;
                }

                //Create a Analysis Database.
                CreateDatabase();

                //Creating a Analysis DataSource.
                CreateDataSource();

                //Creating a Analysis DataSourceView.
                GenerateDwSchema();
                CreateDataSourceView();

                //Creating the Dimension, Attribute, Hierarchy, and MemberProperty Objects.                
                //objDimensions = (Dimension[])  strTableNamesAndKeys, intDimensionTableCount
                CreateDimensions();

                //Creating the Cube, MeasureGroup, Measure, and Partition Objects.
                //, objDimensions, CbFactTableName, strTableNamesAndKeys, intDimensionTableCount
                CreateMyCube();

                //Process cube
                _CbDatabase.Process(ProcessType.ProcessFull);

                AppendLogLine("Cube created successfully.");
            }
            catch (Exception ex)
            {
                AppendLogLine("Error -> " + ex.Message);
            }

            AppendLogLine("");
        }

        #region Connecting to the Analysis Services.

        private Server ConnectAnalysisServices(string strDBServerName, string strProviderName)
        {
            try
            {
                AppendLogLine("");
                AppendLogLine(string.Format("Connecting to the Analysis Services: {0}...", strDBServerName));

                Server objServer = new Server();
                string strConnection = string.Empty;
                if (CbIntegratedSecurity)
                {
                    //Direct Connection
                    strConnection = string.Format("Data Source={0};Integrated Security=SSPI;Provider={1}", strDBServerName, strProviderName);
                }
                else
                {
                    //HTTP connection
                    strConnection = strConnection +
                                    string.Format(@"Data Source=http://{0}/;Provider={1};User Id={2};Password={3}",
                                                  strDBServerName, strProviderName, CbUserId, CbPassword);
                }

                AppendLogLine(string.Format("Analysis services connection string: {0}", strConnection));
                //Disconnect from current connection if it's currently connected.
                if (objServer.Connected)
                    objServer.Disconnect();
                else
                    objServer.Connect(strConnection);

                AppendLogLine(string.Format("  Product name    : {0}", objServer.ProductName));
                AppendLogLine(string.Format("  Product edition : {0}", objServer.Edition));
                AppendLogLine(string.Format("  Version         : {0}", objServer.Version));
                foreach (Database aAnalysisDatabase in objServer.Databases)
                {
                    AppendLogLine(string.Format("  Database        : [{0}] with cubes:", aAnalysisDatabase.Name));
                    foreach (Cube aCube in aAnalysisDatabase.Cubes)
                    {
                        AppendLogLine(string.Format("             Cube : [{0}]", aCube.Name));
                    }
                }
                AppendLogLine("");

                return objServer;
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in Connecting to the Analysis Services. Error Message -> " + ex.Message);
                return null;
            }
        }
        #endregion Connecting to the Analysis Services.

        #region Create a Analysis Database.

        private void CreateDatabase()
        {
            try
            {
                AppendLogLine("");
                AppendLogLine(string.Format("Creating a Analysis database: {0} ...", CbDbName));

                _CbServer.Update();
                if (_CbServer.Databases.Contains(CbDbName))
                {
                    AppendLogLine(string.Format("Analysis database: [{0}] already exists, drop it?.", CbDbName));

                    if (MessageBox.Show(string.Format("The Analysis database '{0} - {1}' already exists. Do you want to drop it?", CbServerName, CbDbName),
                                        "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _CbDatabase = _CbServer.Databases.GetByName(CbDbName);
                        _CbDatabase.Drop();
                    }
                    else
                    {
                        throw new Exception(string.Format("The Analysis database '{0} - {1}' already exists, but user did not want to drop it. Aborting procedure.",
                                            CbServerName, CbDbName));
                    }
                }

                _CbDatabase = new Database();
                //_CbServer.Databases.GetNewName(CbDbName)
                _CbDatabase = _CbServer.Databases.Add(CbDbName);
                //Save Database to the Analysis Services.
                _CbDatabase.Update();
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in creating a analysis database. Error Message -> " + ex.Message);
                throw;
            }
        }
        #endregion Creating a Database.

        #region Creating a DataSource.
        private void CreateDataSource()
        {
            try
            {
                AppendLogLine("");
                AppendLogLine("Creating a DataSource ...");
                _CbDataSource = new RelationalDataSource();
                //Add Data Source to the Database.
                _CbDataSource = _CbDatabase.DataSources.Add(_CbServer.Databases.GetNewName(CbDataSourceName));
                _CbDataSource.ConnectionString = "Provider=SQLNCLI10.1; Data Source=" + DwDbServerName + "; Initial Catalog=" + DwDbName + "; Integrated Security=SSPI;";
                if (DwDsUserName != "")
                {
                    _CbDataSource.ImpersonationInfo = new ImpersonationInfo(ImpersonationMode.ImpersonateAccount, DwDsUserName, DwDsPassword);
                } 
                _CbDataSource.Update();
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in creating a dataSource. Error Message -> " + ex.Message);
                throw;
            }
        }
        #endregion Creating a DataSource.

        #region Creating a DataSourceView.
        private void GenerateDwSchema()
        {
            try
            {
                AppendLogLine("Creating a DataSourceView ...");
                //Create the connection string.
                string conxString = "Data Source=" + DwDbServerName + "; Initial Catalog=" + DwDbName + "; Integrated Security=True;";
                string conxStringOle = "Provider=sqloledb; Data Source=" + DwDbServerName + "; Initial Catalog=" + DwDbName + "; Integrated Security=SSPI;";
                //Create the SqlConnection.
                _CbConnection = new SqlConnection(conxString);
                _CBOleConnection = new OleDbConnection(conxStringOle);
                _CbDataSet = new DataSet();

                foreach (DsDwTableMap_M aDwTableMap in DsDwMap.DsDwTableMapList)
                {
                    FillDataSet(aDwTableMap);
                }

                if (DsDwMap.DwTableMapTopologicalSortList != null)
                {
                    for (int i = DsDwMap.DwTopologicalLevels - 1; i >= 1; i--)
                    {
                        ICollection<DsDwTableMap_M> dsDwTableMaps = DsDwMap.DwTableMapTopologicalSortList[i];
                        foreach (DsDwTableMap_M aChildTable in dsDwTableMaps)
                        {
                            foreach (DsDwColumnMap_M aChildFkColumn in aChildTable.DsDwColumnMapList.Where(cm => cm.Include && cm.Transformation == DsDwColumnTransformation.ForeignKey))
                            {
                                DsDwColumnMap_M aParentColumnPK = 
                                    aChildFkColumn.DwForeignKeyReferencedTableMap.DsDwColumnMapList.FirstOrDefault(cm => cm.Transformation == DsDwColumnTransformation.SurrogateKey);
                                AddDataTableRelation(aParentColumnPK.Parent.DwTableName, aParentColumnPK.DwColumn.Name, aChildTable.DwTableName, aChildFkColumn.DwColumn.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in creating a dataSourceView - GenerateDwSchema. Error Message -> " + ex.Message);
                _CbDataSet = null;
                throw;
            }
        }

        private void FillDataSet(DsDwTableMap_M aDsDwTableMap)
        {
            try
            {
                string strCommand = "Select * from " + aDsDwTableMap.DwSchemaTableName + " WHERE 1=0";
                OleDbDataAdapter objEmpData = new OleDbDataAdapter(strCommand, _CBOleConnection);
                DataTable[] dataTables = objEmpData.FillSchema(_CbDataSet, SchemaType.Mapped, aDsDwTableMap.DwTableName);
                DataTable dataTable = dataTables[0];
                dataTable.ExtendedProperties.Add("TableType", "Table");
                dataTable.ExtendedProperties.Add("DbSchemaName", aDsDwTableMap.DsSchemaName);
                dataTable.ExtendedProperties.Add("DbTableName", aDsDwTableMap.DwTableName);
                dataTable.ExtendedProperties.Add("FriendlyName", aDsDwTableMap.DwTableName);
                dataTable = null;
                dataTables = null;
                objEmpData = null;

            }
            catch (Exception ex)
            {
                AppendLogLine("Error in Creating a DataSourceView - FillDataSet. Error Message -> " + ex.Message);
                throw;
            }
        }

        private void AddDataTableRelation(string strParentTableName, string strParentTableKey, string strChildTableName, string strChildTableKey)
        {
            try
            {
                _CbDataSet.Relations.Add(strChildTableName + "_" + strParentTableName + "_FK",
                                         _CbDataSet.Tables[strParentTableName].Columns[strParentTableKey],
                                         _CbDataSet.Tables[strChildTableName].Columns[strChildTableKey], true);
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in Creating a DataSourceView - AddDataTableRelation. Error Message -> " + ex.Message);
                throw;
            }
        }

        private void CreateDataSourceView()
        {
            try
            {
                _CbDataSourceView = new DataSourceView();
                //Add Data Source View to the Database.
                _CbDataSourceView = _CbDatabase.DataSourceViews.Add(_CbDatabase.DataSourceViews.GetNewName(CbDataSourceViewName));
                _CbDataSourceView.DataSourceID = _CbDataSource.ID;
                _CbDataSourceView.Schema = _CbDataSet;
                _CbDataSourceView.Update();
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in Creating a DataSourceView - CreateDataSourceView. Error Message -> " + ex.Message);
                throw;
            }
        }
        #endregion Creating a DataSourceView.

        private void CreateDimensions()
        {
            try
            {
                AppendLogLine("Creating the Dimension, Attribute, Hierarchy, and MemberProperty Objects ...");

                foreach (CbDimension_M cbDimensionM in DwCbMap.DimensionList.AList)
                {
                    GenerateDimension(cbDimensionM);
                }
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in Creating Dimensions. Error Message -> " + ex.Message);
                throw;
            }
        }

        public OleDbType ConvertFromSqlDataType(DataType aDataType)
        {
            switch (aDataType.SqlDataType)
            {
                case SqlDataType.BigInt: return OleDbType.BigInt;
                case SqlDataType.Binary: return OleDbType.Binary;
                case SqlDataType.Bit: return OleDbType.Boolean;
                case SqlDataType.Date: return OleDbType.Date;
                case SqlDataType.DateTime: return OleDbType.DBTimeStamp;
                case SqlDataType.DateTime2: return OleDbType.Filetime;
                case SqlDataType.Decimal: return OleDbType.Double;
                case SqlDataType.Float: return OleDbType.Double;
                case SqlDataType.Int: return OleDbType.Integer;
                case SqlDataType.Money: return OleDbType.Currency;
                case SqlDataType.Real: return OleDbType.Double;
                case SqlDataType.UniqueIdentifier: return OleDbType.Guid;
                case SqlDataType.NVarChar: return OleDbType.WChar;
                case SqlDataType.NChar: return OleDbType.WChar;
                case SqlDataType.VarChar: return OleDbType.VarChar;
                case SqlDataType.Char: return OleDbType.Char;
                case SqlDataType.VarBinary: return OleDbType.VarBinary;
                case SqlDataType.Variant: return OleDbType.Variant;
                case SqlDataType.TinyInt: return OleDbType.TinyInt;
                default: return OleDbType.Integer;
            }
        }

        private DataItem CreateDataItem(string tableName, string columnName, NullProcessing aNullProcessing, DataType aDataType)
        {
            DataTable dataTable = _CbDataSourceView.Schema.Tables[tableName];
            DataColumn dataColumn = dataTable.Columns[columnName];
            OleDbType aOleDbType;
            if (aDataType == null)
                aOleDbType = OleDbTypeConverter.GetRestrictedOleDbType(dataColumn.DataType);
            else
                aOleDbType = ConvertFromSqlDataType(aDataType);

            DataItem aDataItem = new DataItem(tableName, columnName, aOleDbType);
            aDataItem.NullProcessing = aNullProcessing;

            return aDataItem;
        }

        private void GenerateDimension(CbDimension_M aCbDimension)
        {
            try
            {
                aCbDimension.DimensionObj = new Dimension();

                //Add Dimension to the Database
                aCbDimension.DimensionObj = _CbDatabase.Dimensions.Add(aCbDimension.Name);
                aCbDimension.DimensionObj.Source = new DataSourceViewBinding(_CbDataSourceView.ID);
                aCbDimension.DimensionObj.UnknownMember = UnknownMemberBehavior.Visible;
                aCbDimension.DimensionObj.StorageMode = DimensionStorageMode.Molap;

                DimensionAttributeCollection objDimensionAttributesColl = aCbDimension.DimensionObj.Attributes;

                //Add attributes to dimension
                foreach (CbDimensionAttribute_M cbDimensionAttributeM in aCbDimension.AttributeList.Where(da => da.Include))
                {
                    DimensionAttribute objAttribute = objDimensionAttributesColl.Add(cbDimensionAttributeM.Name);
                    objAttribute.Usage = cbDimensionAttributeM.Usage;
                    objAttribute.AttributeHierarchyVisible = cbDimensionAttributeM.AttributeHierarchyVisible;
                    objAttribute.KeyColumns.Add(
                        CreateDataItem(aCbDimension.ReferenceToDsDwTableMap.DwTableName,
                                       cbDimensionAttributeM.ReferenceToDsDwColumnMap.DwColumn.Name,
                                       NullProcessing.UnknownMember,
                                       cbDimensionAttributeM.ReferenceToDsDwColumnMap.DwColumn.DataType
                                       ));
                }

                //Save dimension to database
                aCbDimension.DimensionObj.Update();
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in creating a dimension. Error Message -> " + ex.Message);
                throw;
            }
        }

        private void CreateMyCube()
        {
            try
            {
                AppendLogLine("Creating the Cube, MeasureGroup, Measure, and Partition Objects ...");

                _CbCube = _CbDatabase.Cubes.Add(CbName);
                _CbCube.Source = new DataSourceViewBinding(_CbDataSourceView.ID);
                _CbCube.StorageMode = StorageMode.Molap;

                //add measure groups
                foreach (CbMeasureGroup_M aCbMeasureGroup in DwCbMap.MeasureGroupList.AList)
                {
                    aCbMeasureGroup.MeasureGroupObj = _CbCube.MeasureGroups.Add(aCbMeasureGroup.Name);
                    aCbMeasureGroup.MeasureGroupObj.StorageMode = StorageMode.Molap;
                    aCbMeasureGroup.MeasureGroupObj.ProcessingMode = ProcessingMode.LazyAggregations;
                    foreach (CbMeasure_M aCbMeasure in aCbMeasureGroup.MeasureList.Where(m => m.Include))
                    {
                        aCbMeasure.MeasureObj = aCbMeasureGroup.MeasureGroupObj.Measures.Add(aCbMeasure.Name);
                        aCbMeasure.MeasureObj.AggregateFunction = AggregationFunction.Sum;
                        if (aCbMeasure.ReferenceToDsDwColumnMap.DwColumn.DataType.SqlDataType == SqlDataType.Money)
                        {
                            aCbMeasure.MeasureObj.FormatString = "Currency";
                        }
                        if (aCbMeasure.ReferenceToDsDwColumnMap.DwColumn.DataType.SqlDataType == SqlDataType.Decimal)
                        {
                            aCbMeasure.MeasureObj.FormatString = "#.##0,00;-#.##0,00";
                        }
                        aCbMeasure.MeasureObj.Source =
                            CreateDataItem(aCbMeasureGroup.ReferenceToDsDwTableMap.DwTableName,
                                           aCbMeasure.ReferenceToDsDwColumnMap.DwColumn.Name,
                                           NullProcessing.Automatic,
                                           aCbMeasure.ReferenceToDsDwColumnMap.DwColumn.DataType);
                    }
                    //add count measure
                    Measure meas = aCbMeasureGroup.MeasureGroupObj.Measures.Add(aCbMeasureGroup.Name + "Count");
                    meas.AggregateFunction = AggregationFunction.Count;
                    meas.FormatString = "#,#";
                    meas.Source = CreateDataItem(aCbMeasureGroup.ReferenceToDsDwTableMap.DwTableName,
                                                 aCbMeasureGroup.KeyName,
                                                 NullProcessing.Automatic,
                                                 null);

                    Partition aCubePartition = aCbMeasureGroup.MeasureGroupObj.Partitions.Add(aCbMeasureGroup.Name);
                    aCubePartition.Source = new TableBinding(_CbDataSource.ID,
                                                             aCbMeasureGroup.ReferenceToDsDwTableMap.DwSchemaName,
                                                             aCbMeasureGroup.ReferenceToDsDwTableMap.DwTableName);
                    aCubePartition.ProcessingMode = ProcessingMode.Regular;
                    aCubePartition.StorageMode = StorageMode.Molap;
                }

                foreach (CbDimension_M aCbDimension in DwCbMap.DimensionList.AList)
                {
                    AddDimensionToCube(aCbDimension);
                }

                foreach (CbMeasureGroup_M aCbMeasureGroup in DwCbMap.MeasureGroupList.AList)
                {
                    foreach (CbMeasure_M aCbMeasure in aCbMeasureGroup.MeasureList.Where(m => m != null && m.ReferenceToDimension != null))
                    {
                        AddLinkDimensionMeasureGroup(aCbMeasure, aCbMeasureGroup);
                    }
                    if (aCbMeasureGroup.FactDimension != null)
                        AddFactLinkDimensionMeasureGroup(aCbMeasureGroup.FactDimension, aCbMeasureGroup);

                }
                //Save Cube and all major objects to the Analysis Services
                _CbCube.Update(UpdateOptions.ExpandFull);
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in creating the cube. Error Message -> " + ex.Message);
                throw;
            }
        }

        private void AddLinkDimensionMeasureGroup(CbMeasure_M aCbMeasure, CbMeasureGroup_M aCbMeasureGroup)
        {
            try
            {
                CbDimension_M aCbDimension = aCbMeasure.ReferenceToDimension;
                RegularMeasureGroupDimension objRegMGDim = new RegularMeasureGroupDimension();
                MeasureGroupAttribute objMGA = new MeasureGroupAttribute();
                objRegMGDim = aCbMeasureGroup.MeasureGroupObj.Dimensions.Add(aCbDimension.CubeDimensionObj.ID);
                //Link TableKey in DimensionTable with TableKey in FactTable Measure Group
                objMGA = objRegMGDim.Attributes.Add(aCbDimension.DimensionObj.KeyAttribute.ID);
                objMGA.Type = MeasureGroupAttributeType.Granularity;
                objMGA.KeyColumns.Add(
                    CreateDataItem(aCbMeasureGroup.ReferenceToDsDwTableMap.DwTableName,
                                   aCbMeasure.ReferenceToDsDwColumnMap.DwColumn.Name,
                                   NullProcessing.UnknownMember,
                                   aCbMeasure.ReferenceToDsDwColumnMap.DwColumn.DataType));
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in creating link between measure group and dimension. Error Message -> " + ex.Message);
                throw;
            }
        }

        private void AddFactLinkDimensionMeasureGroup(CbDimension_M aCbDimension, CbMeasureGroup_M aCbMeasureGroup)
        {
            try
            {
                DegenerateMeasureGroupDimension objMGFactDim = new DegenerateMeasureGroupDimension();
                objMGFactDim.CubeDimensionID = aCbDimension.CubeDimensionObj.ID;
                aCbMeasureGroup.MeasureGroupObj.Dimensions.Add(objMGFactDim);

                MeasureGroupAttribute objMGA = new MeasureGroupAttribute();

                //Link TableKey in DimensionTable with TableKey in FactTable Measure Group
                objMGA = objMGFactDim.Attributes.Add(aCbDimension.DimensionObj.KeyAttribute.ID);
                objMGA.Type = MeasureGroupAttributeType.Granularity;
                objMGA.KeyColumns.Add(
                    CreateDataItem(aCbMeasureGroup.ReferenceToDsDwTableMap.DwTableName,
                                   aCbMeasureGroup.KeyName,
                                   NullProcessing.UnknownMember,
                                   null));
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in creating fact link between measure group and dimension. Error Message -> " + ex.Message);
                throw;
            }
        }

        private void AddDimensionToCube(CbDimension_M aCbDimension)
        {
            try
            {
                aCbDimension.CubeDimensionObj = _CbCube.Dimensions.Add(aCbDimension.DimensionObj.ID);
            }
            catch (Exception ex)
            {
                AppendLogLine("Error in adding dimension to cube. Error Message -> " + ex.Message);
                throw;
            }
        }

        public void Dispose()
        {
            _CbServer.Dispose();
            _CbDatabase.Dispose();
            _CbDataSource.Dispose();
            _CbDataSourceView.Dispose();
            _CbDataSet.Dispose();
            _CbConnection.Dispose();
            _CBOleConnection.Dispose();
        }
    }

}
