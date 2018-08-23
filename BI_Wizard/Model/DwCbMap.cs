using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AnalysisServices;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using BI_Wizard.Helper;

namespace BI_Wizard.Model
{
    [Serializable]
    public class CbDimensionAttribute_M : Base_VM
    {
        private bool _IsSelected = false;
        [XmlIgnore]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (SetProperty(ref _Name, value))
                {
                    if (_Usage == AttributeUsage.Key && Parent != null)
                    {
                        Parent.KeyName = Name;
                    }
                }
            }
        }

        private bool _Include=true;
        public bool Include
        {
            get { return _Include; }
            set { SetProperty(ref _Include, value); }
        }

        private Guid _ReferenceToDsDwColumnMapGuid;
        public Guid ReferenceToDsDwColumnMapGuid
        {
            get { return _ReferenceToDsDwColumnMapGuid; }
            set { SetProperty(ref _ReferenceToDsDwColumnMapGuid, value); }
        }

        private DsDwColumnMap_M _ReferenceToDsDwColumnMap;

        [XmlIgnore]
        public DsDwColumnMap_M ReferenceToDsDwColumnMap
        {
            get { return _ReferenceToDsDwColumnMap; }
            set
            {
                if (SetProperty(ref _ReferenceToDsDwColumnMap, value) && value != null)
                {
                    _ReferenceToDsDwColumnMapGuid = value.MyGuid;
                    if (_ReferenceToDsDwColumnMap.Transformation == DsDwColumnTransformation.SurrogateKey && Parent != null)
                    {
                        Parent.KeyName = Name;
                    }
                }
            }
        }

        //public SerializableObjectReference<DsDwColumnMap_M> ReferenceToDsDwColumnMap = new SerializableObjectReference<DsDwColumnMap_M>();

        private Microsoft.AnalysisServices.AttributeUsage _Usage;
        public AttributeUsage Usage
        {
            get { return _Usage; }
            set
            {
                if (SetProperty(ref _Usage, value))
                {
                    if (_Usage == AttributeUsage.Key && Parent != null)
                    {
                        Parent.KeyName = Name;
                    }
                }
            }
        }

        private bool _AttributeHierarchyVisible;
        public bool AttributeHierarchyVisible
        {
            get { return _AttributeHierarchyVisible; }
            set { SetProperty(ref _AttributeHierarchyVisible, value); }
        }

        private bool _IsIncludeEditable=true;
        public bool IsIncludeEditable
        {
            get { return _IsIncludeEditable; }
            set { _IsIncludeEditable = value; }
        }

        public void UpdateProperties()
        {
            if (ReferenceToDsDwColumnMap.DwColumn != null)
            {
                switch (ReferenceToDsDwColumnMap.Transformation)
                {
                    case DsDwColumnTransformation.SurrogateKey:
                        AttributeHierarchyVisible = false;
                        Usage = AttributeUsage.Key;
                        IsIncludeEditable = false;
                        Include = true;
                        break;
                    case DsDwColumnTransformation.BusinesKey:
                        AttributeHierarchyVisible = false;
                        Usage = AttributeUsage.Regular;
                        IsIncludeEditable = true;
                        Include = false;
                        break;
                    case DsDwColumnTransformation.ForeignKey:
                        AttributeHierarchyVisible = false;
                        Include = true;
                        IsIncludeEditable = false;
                        Usage = AttributeUsage.Regular;
                        break;
                    default:
                        if (ReferenceToDsDwColumnMap.DwColumn.DataType.Name.ToLower() == "int" ||
                            ReferenceToDsDwColumnMap.DwColumn.DataType.Name.ToLower() == "decimal" ||
                            ReferenceToDsDwColumnMap.DwColumn.DataType.Name.ToLower() == "money")
                        {
                            AttributeHierarchyVisible = true;
                            Include = false;
                            IsIncludeEditable = true;
                        }
                        else if (ReferenceToDsDwColumnMap.DwColumn.DataType.Name.ToLower() == "nvarchar" ||
                                 ReferenceToDsDwColumnMap.DwColumn.DataType.Name.ToLower() == "bit")
                        {
                            AttributeHierarchyVisible = true;
                            IsIncludeEditable = true;
                            Include = true;
                        }
                        else
                        {
                            AttributeHierarchyVisible = false;
                            IsIncludeEditable = true;
                            Include = false;
                        }
                        Usage = AttributeUsage.Regular;
                        break;
                }
            }
            else
            {
                Include = false;
                IsIncludeEditable = false;
            }
        }

        private CbDimension_M _Parent;
        [XmlIgnore]
        public CbDimension_M Parent
        {
            get { return _Parent; }
            set { SetProperty(ref _Parent, value); }
        }
    }

    [Serializable]
    public class CbDimension_M : Base_VM
    {
        [XmlIgnore]
        public Dimension DimensionObj;
        [XmlIgnore]
        public CubeDimension CubeDimensionObj;

        private bool _IsSelected = false;
        [XmlIgnore]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }

        private ObservableCollection<CbDimensionAttribute_M> _AttributeList;
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }

        private string _KeyName;
        public string KeyName
        {
            get { return _KeyName; }
            set { SetProperty(ref _KeyName, value); }
        }

        private bool _Include=true;
        public bool Include
        {
            get { return _Include; }
            set { SetProperty(ref _Include, value); }
        }

        private Guid _ReferenceToDsDwTableMapGuid;
        public Guid ReferenceToDsDwTableMapGuid
        {
            get { return _ReferenceToDsDwTableMapGuid; }
            set { SetProperty(ref _ReferenceToDsDwTableMapGuid, value); }
        }

        private DsDwTableMap_M _ReferenceToDsDwTableMap;

        [XmlIgnore]
        public DsDwTableMap_M ReferenceToDsDwTableMap
        {
            get { return _ReferenceToDsDwTableMap; }
            set
            {
                if (SetProperty(ref _ReferenceToDsDwTableMap, value) && value != null)
                {
                    ReferenceToDsDwTableMapGuid = value.MyGuid;
                    ReferenceToDsDwTableMapLink.ReferenceToObject = value;
                }
            }
        }

        public SerializableObjectReference<DsDwTableMap_M> ReferenceToDsDwTableMapLink = new SerializableObjectReference<DsDwTableMap_M>();
   
        private CbDimensionList_M _Parent;
        [XmlIgnore]
        public CbDimensionList_M Parent
        {
            get { return _Parent; }
            set { SetProperty(ref _Parent, value); }
        }

        public CbDimension_M()
        {
            AttributeList = new ObservableCollection<CbDimensionAttribute_M>();
        }

        public ObservableCollection<CbDimensionAttribute_M> AttributeList
        {
            get { return _AttributeList; }
            set
            {
                if (SetProperty(ref _AttributeList, value) && value != null)
                    _AttributeList.CollectionChanged += NotifyCollectionChanged_AttributeList;
            }
        }

        public void NotifyCollectionChanged_AttributeList(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CbDimensionAttribute_M aCbDimensionAttribute in e.NewItems)
                {
                    if (aCbDimensionAttribute != null)
                    {
                        aCbDimensionAttribute.Parent = this;
                    }
                }
            }
        }
    }

    [Serializable]
    public class CbDimensionList_M : Base_VM
    {
        private ObservableCollection<CbDimension_M> _AList;

        private DwCbMap_M _Parent;
        [XmlIgnore]
        public DwCbMap_M Parent
        {
            get { return _Parent; }
            set { SetProperty(ref _Parent, value); }
        }

        public CbDimensionList_M()
        {
            AList = new ObservableCollection<CbDimension_M>();
        }

        public ObservableCollection<CbDimension_M> AList
        {
            get { return _AList; }
            set
            {
                if (SetProperty(ref _AList, value) && value != null)
                    _AList.CollectionChanged += NotifyCollectionChanged_AList;
            }
        }

        public void NotifyCollectionChanged_AList(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CbDimension_M aCbDimension in e.NewItems)
                {
                    if (aCbDimension != null)
                    {
                        aCbDimension.Parent = this;
                    }
                }
            }
        }

    }

    [Serializable]
    public class CbMeasure_M : Base_VM
    {
        [XmlIgnore]
        public Measure MeasureObj;
        [XmlIgnore]
        public CbDimension_M ReferenceToDimension;

        private bool _IsSelected = false;
        [XmlIgnore]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (SetProperty(ref _Name, value))
                {
                    if ((_ReferenceToDsDwColumnMap != null) &&
                        (_ReferenceToDsDwColumnMap.Transformation == DsDwColumnTransformation.SurrogateKey) &&
                        (Parent != null))
                    {
                        Parent.KeyName = Name;
                    }
                }
            }
        }

        private bool _Include=true;
        public bool Include
        {
            get { return _Include; }
            set { SetProperty(ref _Include, value); }
        }


        private Guid _ReferenceToDsDwColumnMapGuid;
        public Guid ReferenceToDsDwColumnMapGuid
        {
            get { return _ReferenceToDsDwColumnMapGuid; }
            set { SetProperty(ref _ReferenceToDsDwColumnMapGuid, value); }
        }

        private DsDwColumnMap_M _ReferenceToDsDwColumnMap;

        public void UpdateProperties()
        {
            if (ReferenceToDsDwColumnMap.DwColumn != null)
            {
                switch (ReferenceToDsDwColumnMap.Transformation)
                {
                    case DsDwColumnTransformation.SurrogateKey:
                        IsIncludeEditable = false;
                        Include = false;
                        if (Parent != null)
                        {
                            Parent.KeyName = Name;
                        }
                        break;
                    case DsDwColumnTransformation.BusinesKey:
                        IsIncludeEditable = false;
                        Include = false;
                        break;
                    case DsDwColumnTransformation.ForeignKey:
                        Include = false;
                        IsIncludeEditable = false;
                        break;
                    default:
                        if (ReferenceToDsDwColumnMap.DwColumn.DataType.Name.ToLower() == "int" ||
                            ReferenceToDsDwColumnMap.DwColumn.DataType.Name.ToLower() == "decimal" ||
                            ReferenceToDsDwColumnMap.DwColumn.DataType.Name.ToLower() == "money")
                        {
                            Include = true;
                            IsIncludeEditable = true;
                        }
                        else if (ReferenceToDsDwColumnMap.DwColumn.DataType.Name.ToLower() == "nvarchar")
                        {
                            IsIncludeEditable = false;
                            Include = false;
                        }
                        else
                        {
                            IsIncludeEditable = false;
                            Include = false;
                        }
                        break;
                }
            }
            else
            {
                Include = false;
                IsIncludeEditable = false;
            }

        }

        [XmlIgnore]
        public DsDwColumnMap_M ReferenceToDsDwColumnMap
        {
            get { return _ReferenceToDsDwColumnMap; }
            set
            {
                if (SetProperty(ref _ReferenceToDsDwColumnMap, value) && value != null)
                {
                    ReferenceToDsDwColumnMapGuid = value.MyGuid;
                    if ((_ReferenceToDsDwColumnMap != null) &&
                        (_ReferenceToDsDwColumnMap.Transformation == DsDwColumnTransformation.SurrogateKey) &&
                        (Parent != null))
                    {
                        Parent.KeyName = Name;
                    }
                }
            }
        }

        private bool _IsIncludeEditable=true;
        public bool IsIncludeEditable
        {
            get { return _IsIncludeEditable; }
            set { _IsIncludeEditable = value; }
        }


        private CbMeasureGroup_M _Parent;
        [XmlIgnore]
        public CbMeasureGroup_M Parent
        {
            get { return _Parent; }
            set { SetProperty(ref _Parent, value); }
        }
    }

    [Serializable]
    public class CbMeasureGroup_M : Base_VM
    {
        [XmlIgnore]
        public MeasureGroup MeasureGroupObj;

        [XmlIgnore]
        public CbDimension_M FactDimension;

        private bool _IsSelected = false;
        [XmlIgnore]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }

        private ObservableCollection<CbMeasure_M> _MeasureList;
        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }
        private bool _Include=true;
        public bool Include
        {
            get { return _Include; }
            set { SetProperty(ref _Include, value); }
        }

        private Guid _ReferenceToDsDwTableMapGuid;
        public Guid ReferenceToDsDwTableMapGuid
        {
            get { return _ReferenceToDsDwTableMapGuid; }
            set { SetProperty(ref _ReferenceToDsDwTableMapGuid, value); }
        }

        private DsDwTableMap_M _ReferenceToDsDwTableMap;
        [XmlIgnore]
        public DsDwTableMap_M ReferenceToDsDwTableMap
        {
            get { return _ReferenceToDsDwTableMap; }
            set
            {
                if (SetProperty(ref _ReferenceToDsDwTableMap, value) && value != null)
                {
                    ReferenceToDsDwTableMapGuid = value.MyGuid;
                }
            }
        }

        private CbMeasureList_M _Parent;
        [XmlIgnore]
        public CbMeasureList_M Parent
        {
            get { return _Parent; }
            set { SetProperty(ref _Parent, value); }
        }

        public CbMeasureGroup_M()
        {
            MeasureList = new ObservableCollection<CbMeasure_M>();
        }

        public ObservableCollection<CbMeasure_M> MeasureList
        {
            get { return _MeasureList; }
            set
            {
                if (SetProperty(ref _MeasureList, value) && value != null)
                    _MeasureList.CollectionChanged += NotifyCollectionChangedMeasureList;
            }
        }

        private string _KeyName;
        public string KeyName
        {
            get { return _KeyName; }
            set { SetProperty(ref _KeyName, value); }
        }

        public void NotifyCollectionChangedMeasureList(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CbMeasure_M aCbMeasure in e.NewItems)
                {
                    if (aCbMeasure != null)
                    {
                        aCbMeasure.Parent = this;
                    }
                }
            }
        }
    }

    [Serializable]
    public class CbMeasureList_M : Base_VM
    {
        private ObservableCollection<CbMeasureGroup_M> _AList;

        private DwCbMap_M _Parent;
        [XmlIgnore]
        public DwCbMap_M Parent
        {
            get { return _Parent; }
            set { SetProperty(ref _Parent, value); }
        }

        public CbMeasureList_M()
        {
            AList = new ObservableCollection<CbMeasureGroup_M>();
        }

        public ObservableCollection<CbMeasureGroup_M> AList
        {
            get { return _AList; }
            set
            {
                if (SetProperty(ref _AList, value) && value != null)
                    _AList.CollectionChanged += NotifyCollectionChanged_MeasureList;
            }
        }

        public void NotifyCollectionChanged_MeasureList(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CbMeasureGroup_M aCbMeasureGroup in e.NewItems)
                {
                    if (aCbMeasureGroup != null)
                    {
                        aCbMeasureGroup.Parent = this;
                    }
                }
            }
        }
    }

    [Serializable]
    public class DwCbMap_M : Base_VM
    {
        private CbMeasureList_M _MeasureGroupList;
        private CbDimensionList_M _DimensionList;

        private Guid _ReferenceToDsDwMapGuid;
        public Guid ReferenceToDsDwMapGuid
        {
            get { return _ReferenceToDsDwMapGuid; }
            set { SetProperty(ref _ReferenceToDsDwMapGuid, value); }
        }

        private DsDwMap_M _ReferenceToDsDwMap;

        [XmlIgnore]
        public DsDwMap_M ReferenceToDsDwMap
        {
            get { return _ReferenceToDsDwMap; }
            set
            {
                if (SetProperty(ref _ReferenceToDsDwMap, value) && value != null)
                {
                    ReferenceToDsDwMapGuid = value.MyGuid;
                }
            }
        }

        public DwCbMap_M()
        {
            MeasureGroupList = new CbMeasureList_M();
            DimensionList = new CbDimensionList_M();
        }

        public CbDimensionList_M DimensionList
        {
            get { return _DimensionList; }
            set
            {
                if (SetProperty(ref _DimensionList, value) && value != null)
                    value.Parent = this;
            }
        }

        public CbMeasureList_M MeasureGroupList
        {
            get { return _MeasureGroupList; }
            set
            {
                if (SetProperty(ref _MeasureGroupList, value) && value != null)
                    value.Parent = this;
            }
        }

        public void SyncToDsDwMap(DsDwMap_M aDsDwMap, bool aReload)
        {
            ReferenceToDsDwMap = aDsDwMap;
            if (aReload)
            {
                MeasureGroupList.AList.Clear();
                DimensionList.AList.Clear();
            }
            if (aDsDwMap != null)
            {
                foreach (DsDwTableMap_M aDsDwTableMap in aDsDwMap.DsDwTableMapList.Where(tm => tm != null))
                {
                    CbMeasureGroup_M aCbMeasureGroup = MeasureGroupList.AList.FirstOrDefault(mg => mg.ReferenceToDsDwTableMapGuid == aDsDwTableMap.MyGuid);
                    if (aCbMeasureGroup != null)
                    {
                        aCbMeasureGroup.ReferenceToDsDwTableMap = aDsDwTableMap;
                    }
                    else
                    {
                        aCbMeasureGroup = new CbMeasureGroup_M
                        {
                            Name = aDsDwTableMap.DwSchemaTableName.Replace("dbo.", "").Replace(".", ""),
                            ReferenceToDsDwTableMap = aDsDwTableMap
                        };
                        MeasureGroupList.AList.Add(aCbMeasureGroup);
                    }

                    CbDimension_M aCbDimension = DimensionList.AList.FirstOrDefault(mg => mg.ReferenceToDsDwTableMapGuid == aDsDwTableMap.MyGuid);
                    if (aCbDimension != null)
                    {
                        aCbDimension.ReferenceToDsDwTableMap = aDsDwTableMap;
                    }
                    else
                    {
                        aCbDimension = new CbDimension_M
                        {
                            Name = aDsDwTableMap.DwSchemaTableName.Replace("dbo.", "").Replace(".", ""),
                            ReferenceToDsDwTableMap = aDsDwTableMap
                        };
                        DimensionList.AList.Add(aCbDimension);
                    }
                    aCbMeasureGroup.FactDimension = aCbDimension;

                    if (aDsDwTableMap.DsDwColumnMapList != null)
                    {
                        foreach (DsDwColumnMap_M aDsDwColumnMap in aDsDwTableMap.DsDwColumnMapList.Where(cm => cm != null && cm.Include))
                        {
                            CbMeasure_M aCbMeasure = aCbMeasureGroup.MeasureList.FirstOrDefault(mg => mg.ReferenceToDsDwColumnMapGuid == aDsDwColumnMap.MyGuid);
                            if (aCbMeasure != null)
                            {
                                aCbMeasure.ReferenceToDsDwColumnMap = aDsDwColumnMap;
                            }
                            else
                            {
                                aCbMeasure = new CbMeasure_M
                                {
                                    Name = aDsDwColumnMap.DwColumn.Name,
                                    ReferenceToDsDwColumnMap = aDsDwColumnMap
                                };
                                aCbMeasureGroup.MeasureList.Add(aCbMeasure);
                                aCbMeasure.UpdateProperties();
                            }

                            CbDimensionAttribute_M aCbDimensionAttribute = aCbDimension.AttributeList.FirstOrDefault(mg => mg.ReferenceToDsDwColumnMapGuid == aDsDwColumnMap.MyGuid);
                            if (aCbDimensionAttribute != null)
                            {
                                aCbDimensionAttribute.ReferenceToDsDwColumnMap = aDsDwColumnMap;
                            }
                            else
                            {
                                aCbDimensionAttribute = new CbDimensionAttribute_M
                                {
                                    Name = aDsDwColumnMap.DwColumn.Name,
                                    ReferenceToDsDwColumnMap = aDsDwColumnMap
                                };
                                aCbDimension.AttributeList.Add(aCbDimensionAttribute);
                                aCbDimensionAttribute.UpdateProperties();
                            }
                        }
                        aCbMeasureGroup.MeasureList.RemoveAll(m => m == null || m.ReferenceToDsDwColumnMap == null);
                        aCbDimension.AttributeList.RemoveAll(a => a == null || a.ReferenceToDsDwColumnMap == null);
                    }
                }

                MeasureGroupList.AList.RemoveAll(mg => mg == null || mg.ReferenceToDsDwTableMap == null);
                DimensionList.AList.RemoveAll(d => d == null || d.ReferenceToDsDwTableMap == null);

                //fix ReferenceToDimension links
                //SerializableObject.FixAllReferences();
                foreach (CbMeasureGroup_M aCbMeasureGroup in MeasureGroupList.AList)
                {
                    foreach (CbMeasure_M aCbMeasure in aCbMeasureGroup.MeasureList.Where(m => m != null && m.ReferenceToDsDwColumnMap != null))
                    {
                        if (aCbMeasure.ReferenceToDsDwColumnMap.Transformation == DsDwColumnTransformation.ForeignKey)
                        {
                            aCbMeasure.ReferenceToDimension =
                                DimensionList.AList.FirstOrDefault(
                                    dm => dm.ReferenceToDsDwTableMap == aCbMeasure.ReferenceToDsDwColumnMap.DwForeignKeyReferencedTableMap);
                        }
                        else
                        {
                            aCbMeasure.ReferenceToDimension = null;
                        }
                    }
                }
            }
        }
    }
}
