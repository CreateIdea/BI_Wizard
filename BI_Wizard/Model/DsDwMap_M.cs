using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using BI_Wizard.Helper;

namespace BI_Wizard.Model
{

    public enum DsDwColumnUserTransformation
    {
        [Description("Scd 1")]
        Scd1,
        [Description("Scd 2")]
        Scd2,
        [Description("Busines key")]
        BusinesKey
    }

    public enum DsDwColumnTransformation
    {
        [Description("Scd 1")]
        Scd1,
        [Description("Scd 2")]
        Scd2,
        [Description("Busines key")]
        BusinesKey,
        [Description("Foreign Key")]
        ForeignKey,
        [Description("Scd 2 is active")]
        Scd2IsActive,
        [Description("Scd 2 date from")]
        Scd2DateFrom,
        [Description("Scd 2 date to")]
        Scd2DateTo,
        [Description("Surrogate key")]
        SurrogateKey,
        [Description("Created date")]
        CreatedDate,
        [Description("Deleted date")]
        DeletedDate,
        [Description("Modified date")]
        ModifiedDate,
        [Description("Is deleted")]
        IsDeleted
    }

    [Serializable]
    public class DsDwTableReference_M : Base_VM
    {
        private string _ServerName = string.Empty;
        private string _DatabaseName = string.Empty;
        private string _SchemaName = string.Empty;
        private string _TableName = string.Empty;

        public string ServerName
        {
            get { return _ServerName; }
            set { SetProperty(ref _ServerName, value); }
        }

        public string DatabaseName
        {
            get { return _DatabaseName; }
            set { SetProperty(ref _DatabaseName, value); }
        }

        public string SchemaName
        {
            get { return _SchemaName; }
            set { SetProperty(ref _SchemaName, value); }
        }

        public string TableName
        {
            get { return _TableName; }
            set { SetProperty(ref _TableName, value); }
        }

        public string SchemaTableName
        {
            get { return _SchemaName + "." + _TableName; }
        }

    }

    [Serializable]
    public class DsDwColumnMap_M : Base_VM
    {
        private bool _Include;
        private DsDwColumnTransformation _Transformation;
        private DsDwColumn_M _DsColumn;
        private DsDwColumn_M _DsReferencedColumn; //Only used for views
        private DsDwTableReference_M _DsReferencedTable; //Only used for views - view column referenced base table
        private DsDwColumn_M _DwColumn;
        private ObservableCollection<DsDwTableReference_M> _DwForeignKeyReferencedTableList = new ObservableCollection<DsDwTableReference_M>();
        private DsDwTableReference_M _DwForeignKeyReferencedTable;
        private DsDwTableMap_M _DwForeignKeyReferencedTableMap;

        [XmlIgnore]
        public DsDwTableMap_M Parent;


        public bool IsTransformationEditable
        {
            get
            {
                return (_Transformation == DsDwColumnTransformation.Scd1) ||
                       (_Transformation == DsDwColumnTransformation.BusinesKey) ||
                       (_Transformation == DsDwColumnTransformation.Scd2);
            }
        }

        public bool IsIncludeEditable
        {
            get
            {
                return (_Transformation != DsDwColumnTransformation.Scd2DateFrom) &&
                       (_Transformation != DsDwColumnTransformation.Scd2IsActive) &&
                       (_Transformation != DsDwColumnTransformation.SurrogateKey) &&
                       (_Transformation != DsDwColumnTransformation.BusinesKey) &&
                       (_Transformation != DsDwColumnTransformation.Scd2DateTo);
            }
        }

        private bool _IsSelected;
        [XmlIgnore]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }

        public DsDwColumnTransformation Transformation
        {
            get { return _Transformation; }
            set
            {
                if (SetProperty(ref _Transformation, value))
                {
                    NotifyPropertyChanged("IsTransformationEditable");
                    NotifyPropertyChanged("IsIncludeEditable");
                    if (Parent != null)
                    {
                        Parent.UpdateHasScd2Transformation();
                    }
                    if (_Transformation == DsDwColumnTransformation.BusinesKey)
                    {
                        Include = true;
                    }
                }
            }
        }

        public bool Include
        {
            get { return _Include; }
            set { SetProperty(ref _Include, value); }
        }

        public DsDwColumn_M DsColumn
        {
            get { return _DsColumn; }
            set { SetProperty(ref _DsColumn, value); }
        }

        public DsDwColumn_M DsReferencedColumn
        {
            get { return _DsReferencedColumn; }
            set { SetProperty(ref _DsReferencedColumn, value); }
        }

        public DsDwColumn_M DwColumn
        {
            get { return _DwColumn; }
            set { SetProperty(ref _DwColumn, value); }
        }

        public ObservableCollection<DsDwTableReference_M> DwForeignKeyReferencedTableList
        {
            get { return _DwForeignKeyReferencedTableList; }
            set { SetProperty(ref _DwForeignKeyReferencedTableList, value); }
        }

        public DsDwTableReference_M DsReferencedTable
        {
            get { return _DsReferencedTable; }
            set { SetProperty(ref _DsReferencedTable, value); }
        }

        public DsDwTableReference_M DwForeignKeyReferencedTable
        {
            get { return _DwForeignKeyReferencedTable; }
            set { SetProperty(ref _DwForeignKeyReferencedTable, value); }
        }

        [XmlIgnore]
        public DsDwTableMap_M DwForeignKeyReferencedTableMap
        {
            get { return _DwForeignKeyReferencedTableMap; }
            set { SetProperty(ref _DwForeignKeyReferencedTableMap, value); }
        }
    }

    [Serializable]
    public class DsDwTableMap_M : SerializableObject
    {
        private ObservableCollection<DsDwColumnMap_M> _DsDwColumnMapList;
        private ObservableCollection<DsDwForeignKey_M> _DsDwForeignKeyList;
        private ObservableCollection<DsDwTableMap_M> _DwActiveDependencyList = new ObservableCollection<DsDwTableMap_M>();
        private string _DsServerName=string.Empty;
        private string _DsDatabaseName=string.Empty;
        private Boolean _DsIsTable;
        private string _DsTableName=string.Empty;
        private int _DsObjectId;
        private long _DsRowCount;
        private string _DsSchemaName=string.Empty;
        private Urn _DsUrn;
        private string _DwTableName=string.Empty;
        private string _DwSchemaName=string.Empty;
        private bool _IsSelected;
        private bool _DeleteNotMatched;
        private bool _HasScd2Transformation;

        [XmlIgnore] public int Level = 0;

        public DsDwTableMap_M()
        {
            DsDwColumnMapList = new ObservableCollection<DsDwColumnMap_M>();
            DsDwForeignKeyList = new ObservableCollection<DsDwForeignKey_M>();
        }

        public int BuildActiveDepenencyList()
        {
            DwActiveDependencyList.Clear();
            foreach (DsDwColumnMap_M dsDwColumnMap in DsDwColumnMapList.Where(cm=>cm.Include && 
                                                                              cm.Transformation== DsDwColumnTransformation.ForeignKey  && 
                                                                              cm.DwForeignKeyReferencedTableMap != null ))
            {
                DwActiveDependencyList.Add(dsDwColumnMap.DwForeignKeyReferencedTableMap); 
            }
            return DwActiveDependencyList.Count;
            // DwActiveDependencyList
        }


        public void NotifyCollectionChangedEventHandlerColumnMap(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (DsDwColumnMap_M aDsDwColumnMap in e.NewItems)
                {
                    if (aDsDwColumnMap != null)
                    {
                        aDsDwColumnMap.Parent = this;
                    }
                }
            }
        }

        public void NotifyCollectionChangedEventHandlerForeignKey(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (DsDwForeignKey_M dsDwForeignKeyM in e.NewItems)
                {
                    if (dsDwForeignKeyM != null)
                    {
                        dsDwForeignKeyM.Parent = this;
                    }
                }
            }
        }

        [XmlIgnore]
        public string DsSchemaTableName
        {
            get { return string.Format("{0}.{1}", DsSchemaName, DsTableName); }
        }

        public string DsSchemaName
        {
            get { return _DsSchemaName; }
            set { SetProperty(ref _DsSchemaName, value); }
        }

        public string DsTableName
        {
            get { return _DsTableName; }
            set { SetProperty(ref _DsTableName, value); }
        }

        [XmlIgnore] public string DwSchemaTableName
        {
            get { return string.Format("{0}.{1}", DwSchemaName, DwTableName); }
        }

        [XmlIgnore] public string DwSchemaTableName_
        {
            get { return string.Format("{0}_{1}", DwSchemaName, DwTableName); }
        }

        public string DwSchemaName
        {
            get { return _DwSchemaName; }
            set { SetProperty(ref _DwSchemaName, value); }
        }

        public string DwTableName
        {
            get { return _DwTableName; }
            set { SetProperty(ref _DwTableName, value); }
        }

        public bool DsIsTable
        {
            get { return _DsIsTable; }
            set { SetProperty(ref _DsIsTable, value); }
        }

        public int DsObjectId
        {
            get { return _DsObjectId; }
            set { SetProperty(ref _DsObjectId, value); }
        }

        public Urn DsUrn
        {
            get { return _DsUrn; }
            set { SetProperty(ref _DsUrn, value); }
        }

        public long DsRowCount
        {
            get { return _DsRowCount; }
            set { SetProperty(ref _DsRowCount, value); }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }

        public ObservableCollection<DsDwColumnMap_M> DsDwColumnMapList
        {
            get { return _DsDwColumnMapList; }
            set
            {
                if (SetProperty(ref _DsDwColumnMapList, value) && value != null)
                    _DsDwColumnMapList.CollectionChanged += NotifyCollectionChangedEventHandlerColumnMap;
            }
        }

        public bool DeleteNotMatched
        {
            get { return _DeleteNotMatched; }
            set { SetProperty(ref _DeleteNotMatched, value); }
        }

        [XmlIgnore]
        public bool HasScd2Transformation
        {
            get
            {
                return _HasScd2Transformation;
            }
            protected set
            {
                SetProperty(ref _HasScd2Transformation, value);
            }
        }

        public ObservableCollection<DsDwForeignKey_M> DsDwForeignKeyList
        {
            get { return _DsDwForeignKeyList; }
            set
            {
                if (SetProperty(ref _DsDwForeignKeyList, value))
                {
                    _DsDwForeignKeyList.CollectionChanged += NotifyCollectionChangedEventHandlerForeignKey;
                }
            }
        }

        public string DsServerName
        {
            get { return _DsServerName; }
            set { SetProperty(ref _DsServerName, value); }
        }

        public string DsDatabaseName
        {
            get { return _DsDatabaseName; }
            set { SetProperty(ref _DsDatabaseName, value); }
        }

        [XmlIgnore]
        public ObservableCollection<DsDwTableMap_M> DwActiveDependencyList
        {
            get { return _DwActiveDependencyList; }
            set { SetProperty(ref _DwActiveDependencyList, value); }
        }

        public void UpdateHasScd2Transformation()
        {
            foreach (DsDwColumnMap_M aDsDwColumnMap in _DsDwColumnMapList)
            {
                if (aDsDwColumnMap != null)
                {
                    if (aDsDwColumnMap.Transformation == DsDwColumnTransformation.Scd2)
                    {
                        SetEnabledScd2SupportColumns(true);
                        HasScd2Transformation = true;
                        return;
                    }
                }
            }
            SetEnabledScd2SupportColumns(false);
            HasScd2Transformation = false;
        }

        public void SetEnabledScd2SupportColumns(bool aValue)
        {
            foreach (DsDwColumnMap_M aDsDwColumnMap in _DsDwColumnMapList)
            {
                if (aDsDwColumnMap != null)
                {
                    if ((aDsDwColumnMap.Transformation == DsDwColumnTransformation.Scd2IsActive) ||
                       (aDsDwColumnMap.Transformation == DsDwColumnTransformation.Scd2DateFrom) ||
                       (aDsDwColumnMap.Transformation == DsDwColumnTransformation.Scd2DateTo))
                        aDsDwColumnMap.Include = aValue;
                }
            }
        }
    }

    [Serializable]
    public class DsDwMap_M : Base_VM
    {
        private ObservableCollection<DsDwTableMap_M> _DsDwTableMapList=new ObservableCollection<DsDwTableMap_M>();

        [XmlIgnore]
        public IList<ICollection<DsDwTableMap_M>> DwTableMapTopologicalSortList;

        public string PrintTopologicalSortList()
        {
            string resString=string.Empty;
            int aLevel=0;
            if (DwTableMapTopologicalSortList != null)
            {
                foreach (ICollection<DsDwTableMap_M> dsDwTableMapMs in DwTableMapTopologicalSortList)
                {
                    resString = resString + aLevel.ToString("D3")+": ";
                    foreach (DsDwTableMap_M dsDwTableMapM in dsDwTableMapMs)
                    {

                        resString = resString +
                                    string.Format("[{0}.{1}] ", dsDwTableMapM.DwSchemaName, dsDwTableMapM.DwTableName);
                    }
                    resString = resString + Environment.NewLine;
                    aLevel = aLevel + 1;
                }
            }
            return resString;
        }

        [XmlIgnore]
        public int DwTopologicalLevels;
        public void  UpdateLevelsTopologicalSortList()
        {
            int aLevel=0;
            if (DwTableMapTopologicalSortList != null)
            {
                foreach (ICollection<DsDwTableMap_M> dsDwTableMapMs in DwTableMapTopologicalSortList)
                {
                    foreach (DsDwTableMap_M dsDwTableMapM in dsDwTableMapMs)
                    {
                        dsDwTableMapM.Level = aLevel;
                    }
                    aLevel = aLevel + 1;
                }
            }
            DwTopologicalLevels = aLevel;
        }


        public DsDwMap_M()
        {
            
        }

        public ObservableCollection<DsDwTableMap_M> DsDwTableMapList
        {
            get { return _DsDwTableMapList; }
            set { SetProperty(ref _DsDwTableMapList, value); }
        }

        public string DwTopologicalSortList
        {
            get { return PrintTopologicalSortList(); }
        }

        public int BuildActiveDepenencyLists()
        {
            int aTotalDepLinks = 0;
            foreach (DsDwTableMap_M dsDwTableMap in DsDwTableMapList)
            {
                aTotalDepLinks = aTotalDepLinks + dsDwTableMap.BuildActiveDepenencyList();
            }
            DwTableMapTopologicalSortList = TopologicalSort.Group<DsDwTableMap_M>(DsDwTableMapList, x => x.DwActiveDependencyList);
            UpdateLevelsTopologicalSortList();
            NotifyPropertyChanged("DwTopologicalSortList");
            return aTotalDepLinks;
        }

        public void AddUniqueLocalForeignKeyTableReference(DsDwColumnMap_M aDsDwColumnMap, DsDwTableMap_M aDsDwTableMap)
        {
            if (aDsDwColumnMap != null)
            {
                if (!aDsDwColumnMap.DwForeignKeyReferencedTableList.Any(fk =>
                    fk.TableName.ToLower() == aDsDwTableMap.DsTableName.ToLower() &&
                    fk.SchemaName.ToLower() == aDsDwTableMap.DsSchemaName.ToLower()))
                {
                    aDsDwColumnMap.DwForeignKeyReferencedTableList.Add(
                        new DsDwTableReference_M
                        {
                            TableName = aDsDwTableMap.DsTableName,
                            SchemaName = aDsDwTableMap.DsSchemaName
                        });
                    if (aDsDwColumnMap.DwForeignKeyReferencedTableList.Count == 1)
                    {
                        aDsDwColumnMap.DwForeignKeyReferencedTable = aDsDwColumnMap.DwForeignKeyReferencedTableList[0];
                        aDsDwColumnMap.DwForeignKeyReferencedTableMap = aDsDwTableMap;
                    }
                }
            }
        }
        public void FindForeignKeyTableMatch(DsDwColumnMap_M aDsDwColumnMap, DsDwForeignKey_M aDsDwForeignKey)
        {
            if (aDsDwColumnMap != null && aDsDwForeignKey != null)
            {
                aDsDwColumnMap.DwForeignKeyReferencedTableList.Clear();
                aDsDwColumnMap.DwForeignKeyReferencedTable = null;
                foreach (DsDwTableMap_M aDsDwTableMap in this.DsDwTableMapList)
                {

                    if ((aDsDwTableMap != null) && (aDsDwTableMap != aDsDwColumnMap.Parent))
                    {
                        if (aDsDwTableMap.DsSchemaName.ToLower() == aDsDwForeignKey.ReferencedTableSchema.ToLower() &&
                            aDsDwTableMap.DsTableName.ToLower() == aDsDwForeignKey.ReferencedTable.ToLower() &&
                            aDsDwTableMap.DsIsTable)
                        {
                            //found table reference - no need to check for column - we assume FK point to primary keys
                            AddUniqueLocalForeignKeyTableReference(aDsDwColumnMap, aDsDwTableMap);
                        }
                        if (!aDsDwTableMap.DsIsTable && aDsDwTableMap.DsDwColumnMapList != null)  //search in related meta data of the view
                        {
                            foreach (DsDwColumnMap_M dsDwColumnMapLocal in aDsDwTableMap.DsDwColumnMapList)
                            {
                                //we assume FK point to primary keys
                                if (dsDwColumnMapLocal.DsReferencedColumn != null &&
                                    dsDwColumnMapLocal.DsReferencedTable != null &&
                                    dsDwColumnMapLocal.DsReferencedColumn.InPrimaryKey &&
                                    dsDwColumnMapLocal.DsReferencedTable.SchemaName.ToLower() == aDsDwForeignKey.ReferencedTableSchema.ToLower() &&
                                    dsDwColumnMapLocal.DsReferencedTable.TableName.ToLower() == aDsDwForeignKey.ReferencedTable.ToLower())
                                {
                                    //found view reference
                                    AddUniqueLocalForeignKeyTableReference(aDsDwColumnMap, aDsDwTableMap);
                                }
                            }
                        }
                    }
                }
            }
        }
        public void ConfigureForeignKeyTransformations()
        {
            foreach (DsDwTableMap_M aDsDwTableMap in this.DsDwTableMapList)
            {
                if ((aDsDwTableMap != null) &&
                    (aDsDwTableMap.DsDwForeignKeyList != null) &&
                    (aDsDwTableMap.DsDwForeignKeyList.Count > 0))
                {
                    DsDwColumnMap_M aDsDwColumnMap;
                    foreach (DsDwForeignKey_M aDsDwForeignKey in aDsDwTableMap.DsDwForeignKeyList)
                    {
                        if (aDsDwTableMap.DsIsTable)
                        {
                            aDsDwColumnMap = aDsDwTableMap.DsDwColumnMapList.FirstOrDefault(
                                cl => aDsDwForeignKey.Columns.Any(fk =>
                                    cl.DsColumn != null &&
                                    fk.Name.ToLower() == cl.DsColumn.Name.ToLower()));
                        }
                        else
                        {
                            aDsDwColumnMap = aDsDwTableMap.DsDwColumnMapList.FirstOrDefault(
                                cl => aDsDwForeignKey.Columns.Any(fk =>
                                    cl.DsReferencedColumn != null &&
                                    fk.Name.ToLower() == cl.DsReferencedColumn.Name.ToLower()));
                        }
                        if (aDsDwColumnMap != null)
                        {
                            FindForeignKeyTableMatch(aDsDwColumnMap, aDsDwForeignKey);
                            if (aDsDwColumnMap.DwForeignKeyReferencedTableList != null &&
                                aDsDwColumnMap.DwForeignKeyReferencedTableList.Count > 0)
                            {
                                //only make it foreign key if we found a foreign key table match
                                aDsDwColumnMap.Transformation = DsDwColumnTransformation.ForeignKey;
                                aDsDwColumnMap.DwColumn.DataType = new DataType(SqlDataType.Int);
                                if (aDsDwColumnMap.DwColumn.Name.ToLower() == aDsDwColumnMap.DsColumn.Name.ToLower())
                                {
                                    aDsDwColumnMap.DwColumn.Name = string.Format("{0}_Key", aDsDwColumnMap.DsColumn.Name);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}