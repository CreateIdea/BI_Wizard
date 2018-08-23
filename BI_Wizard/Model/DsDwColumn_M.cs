using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.SqlServer.Management.Smo;
using BI_Wizard.Helper;

namespace BI_Wizard.Model
{
    //class to store  Microsoft.SqlServer.Management.Smo.ForeignKeyColumn
    [Serializable]
    public class DsDwForeignKeyColumn_M : Base_VM
    {
        private int _ObjectId;
        private string _Name=string.Empty;
        private string _ReferencedColumn=string.Empty;

        public DsDwForeignKeyColumn_M(ForeignKeyColumn aForeignKeyColumn)
        {
            CopyFromForeignKeyColumn(aForeignKeyColumn);
        }

        public DsDwForeignKeyColumn_M()
        {
        }

        public int ObjectId
        {
            get { return _ObjectId; }
            set { _ObjectId = value; }
        }

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public string ReferencedColumn
        {
            get { return _ReferencedColumn; }
            set { _ReferencedColumn = value; }
        }

        public void CopyFromForeignKeyColumn(ForeignKeyColumn aForeignKeyColumn)
        {
            if (aForeignKeyColumn != null)
            {
                ObjectId = aForeignKeyColumn.ID;
                Name = aForeignKeyColumn.Name;
                ReferencedColumn = aForeignKeyColumn.ReferencedColumn;
            }
        }
    }

    //Class to hold  Microsoft.SqlServer.Management.Smo.ForeignKey
    [Serializable]
    public class DsDwForeignKey_M : Base_VM
    {
        private  ObservableCollection<DsDwForeignKeyColumn_M> _Columns=new ObservableCollection<DsDwForeignKeyColumn_M>();
        private int _ObjectId;
        private bool _IsChecked;
        private bool _IsEnabled;
        private string _Name=string.Empty;
        private string _ReferencedKey=string.Empty;
        private string _ReferencedTable=string.Empty;
        private string _ReferencedTableSchema=string.Empty;

        [XmlIgnore]
        public DsDwTableMap_M Parent;

        public DsDwForeignKey_M(ForeignKey aForeignKey)
        {
            CopyFromForeignKey(aForeignKey);
        }

        public DsDwForeignKey_M()
        {
            
        }
        public ObservableCollection<DsDwForeignKeyColumn_M> Columns
        {
            get { return _Columns; }
            set { SetProperty(ref _Columns, value); }
        }

        public int ObjectId
        {
            get { return _ObjectId; }
            set { SetProperty(ref  _ObjectId, value); }
        }

        public bool IsChecked
        {
            get { return _IsChecked; }
            set { SetProperty(ref  _IsChecked, value); }
        }

        public string Name
        {
            get { return _Name; }
            set { SetProperty(ref  _Name, value); }
        }

        public string ReferencedKey
        {
            get { return _ReferencedKey; }
            set { SetProperty(ref _ReferencedKey, value); }
        }

        public string ReferencedTable
        {
            get { return _ReferencedTable; }
            set { SetProperty(ref _ReferencedTable, value); }
        }

        public string ReferencedTableSchema
        {
            get { return _ReferencedTableSchema; }
            set { SetProperty(ref _ReferencedTableSchema, value); }
        }

        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { SetProperty(ref _IsEnabled, value); }
        }

        public void CopyFromForeignKey(ForeignKey aForeignKey)
        {
            if (aForeignKey != null)
            {
                IsEnabled = aForeignKey.IsEnabled;
                ReferencedTableSchema = aForeignKey.ReferencedTableSchema;
                ReferencedTable = aForeignKey.ReferencedTable;
                ReferencedKey = aForeignKey.ReferencedKey;
                Name = aForeignKey.Name;
                IsChecked = aForeignKey.IsChecked;
                ObjectId = aForeignKey.ID;
                CopyFromForeignKeyColumnCollection(aForeignKey.Columns);
            }
        }

        public void CopyFromForeignKeyColumnCollection(ForeignKeyColumnCollection aForeignKeyColumnCollection)
        {
            Columns.Clear();
            if (aForeignKeyColumnCollection != null && aForeignKeyColumnCollection.Count > 0)
            {
                foreach (ForeignKeyColumn aForeignKeyColumn in aForeignKeyColumnCollection)
                {
                    Columns.Add(new DsDwForeignKeyColumn_M(aForeignKeyColumn));
                }
            }
        }
    }

    //class to hold  Microsoft.SqlServer.Management.Smo.Column
    [Serializable]
    public class DsDwColumn_M : Base_VM
    {
        private string _Collation=string.Empty;
        private bool _Computed;
        private string _ComputedText=string.Empty;
        private DataType _DataType;
        private string _Default=string.Empty;
        private string _DefaultSchema=string.Empty;
        private int _ObjectId;
        private bool _Identity;
        private long _IdentityIncrement;
        private long _IdentitySeed;
        private bool _InPrimaryKey;
        private bool _IsColumnSet;
        private bool _IsDeterministic;
        private bool _IsFileStream;
        private bool _IsForeignKey;
        private bool _IsFullTextIndexed;
        private bool _IsPersisted;
        private bool _IsPrecise;
        private bool _IsSparse;
        private string _Name=string.Empty;
        private bool _IsNullable;
        private bool _RowGuidCol;

        public DsDwColumn_M(Column aColumn)
        {
            CopyFromColumn(aColumn);
        }

        public DsDwColumn_M()
        {

        }

        public void CopyFromColumn(Column aColumn)
        {
            if (aColumn != null)
            {
                Collation = aColumn.Collation;
                Computed = aColumn.Computed;
                ComputedText = aColumn.ComputedText;
                DataType = aColumn.DataType; ;
                Default = aColumn.Default;
                DefaultSchema = aColumn.DefaultSchema;
                ObjectId = aColumn.ID;
                Identity = aColumn.Identity;
                IdentityIncrement = aColumn.IdentityIncrement;
                IdentitySeed = aColumn.IdentitySeed;
                InPrimaryKey = aColumn.InPrimaryKey;
                IsColumnSet = aColumn.IsColumnSet;
                IsDeterministic = aColumn.IsDeterministic;
                IsFileStream = aColumn.IsFileStream;
                IsForeignKey = aColumn.IsForeignKey;
                IsFullTextIndexed = aColumn.IsFullTextIndexed;
                IsPersisted = aColumn.IsPersisted;
                IsPrecise = aColumn.IsPrecise;
                IsSparse = aColumn.IsSparse;
                Name = aColumn.Name;
                IsNullable = aColumn.Nullable;
                RowGuidCol = aColumn.RowGuidCol;
            }
        }


        public string Collation
        {
            get { return _Collation; }
            set { SetProperty(ref _Collation, value); }
        }

        public bool Computed
        {
            get { return _Computed; }
            set { SetProperty(ref _Computed, value); }
        }

        public string ComputedText
        {
            get { return _ComputedText; }
            set { SetProperty(ref _ComputedText, value); }
        }

        public DataType DataType
        {
            get { return _DataType; }
            set { SetProperty(ref _DataType, value); }
        }

        public string Default
        {
            get { return _Default; }
            set { SetProperty(ref _Default, value); }
        }

        public string DefaultSchema
        {
            get { return _DefaultSchema; }
            set { SetProperty(ref _DefaultSchema, value); }
        }

        public int ObjectId
        {
            get { return _ObjectId; }
            set { _ObjectId = value; }
        }

        public bool Identity
        {
            get { return _Identity; }
            set { _Identity = value; }
        }

        public long IdentityIncrement
        {
            get { return _IdentityIncrement; }
            set { _IdentityIncrement = value; }
        }

        public long IdentitySeed
        {
            get { return _IdentitySeed; }
            set { _IdentitySeed = value; }
        }

        public bool InPrimaryKey
        {
            get { return _InPrimaryKey; }
            set { _InPrimaryKey = value; }
        }

        public bool IsColumnSet
        {
            get { return _IsColumnSet; }
            set { _IsColumnSet = value; }
        }

        public bool IsDeterministic
        {
            get { return _IsDeterministic; }
            set { _IsDeterministic = value; }
        }

        public bool IsFileStream
        {
            get { return _IsFileStream; }
            set { _IsFileStream = value; }
        }

        public bool IsForeignKey
        {
            get { return _IsForeignKey; }
            set { _IsForeignKey = value; }
        }

        public bool IsFullTextIndexed
        {
            get { return _IsFullTextIndexed; }
            set { _IsFullTextIndexed = value; }
        }

        public bool IsPersisted
        {
            get { return _IsPersisted; }
            set { _IsPersisted = value; }
        }

        public bool IsPrecise
        {
            get { return _IsPrecise; }
            set { _IsPrecise = value; }
        }

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public bool IsNullable
        {
            get { return _IsNullable; }
            set { _IsNullable = value; }
        }

        public bool RowGuidCol
        {
            get { return _RowGuidCol; }
            set { _RowGuidCol = value; }
        }

        public bool IsSparse
        {
            get { return _IsSparse; }
            set { _IsSparse = value; }
        }
    }


}
