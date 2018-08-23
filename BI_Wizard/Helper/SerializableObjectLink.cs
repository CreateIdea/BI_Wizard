using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BI_Wizard.Helper
{
    [Serializable]
    public class SerializableObject: Base_VM, IDisposable
    {
        [XmlIgnore]
        public static readonly HashSet<object> ReferenceList = new HashSet<Object>();

        [XmlIgnore]
        public static readonly Dictionary<Guid, SerializableObject> ObjectList = new Dictionary<Guid, SerializableObject>();
        public override void SetMyGuid(Guid aGuid)
        {
            ObjectList[aGuid] = this;
        }

        public void Dispose()
        {
            if (MyGuid != Guid.Empty)
            {
                ObjectList.Remove(MyGuid);
            }
        }

        public static void ClearLists()
        {
            ReferenceList.Clear();
            ObjectList.Clear();
        }
        public static void FixAllReferences()
        {
            foreach (dynamic aRef in ReferenceList)
            {
                aRef.FixReference();
            }
        }

    }

    public class SerializableObjectReference<aType> where aType : SerializableObject, IDisposable
    {
        public void Dispose()
        {
            SerializableObject.ReferenceList.Remove(this);
        }

        public SerializableObjectReference()
        {
            SerializableObject.ReferenceList.Add(this);
        }

        [XmlIgnore]
        private Guid _ReferenceToObjectGuid;

        public Guid ReferenceToObjectGuid
        {
            get { return _ReferenceToObjectGuid; }
            set { _ReferenceToObjectGuid = value; }
        }

        public void FixReference()
        {
            if (ReferenceToObjectGuid != Guid.Empty)
            {
                SerializableObject _FoundReferenceToObject;

                if (SerializableObject.ObjectList.TryGetValue(ReferenceToObjectGuid, out _FoundReferenceToObject))
                {
                    _ReferenceToObject = (aType) _FoundReferenceToObject;
                }
                else
                {
                    _ReferenceToObject = default(aType);
                }
            }
            else
            {
                _ReferenceToObject = default(aType);
            }
        }

        [XmlIgnore]
        private aType _ReferenceToObject;

        [XmlIgnore]
        public aType ReferenceToObject
        {
            get { return _ReferenceToObject; }
            set
            {
                if (value == null && _ReferenceToObjectGuid != Guid.Empty)
                {
                    ReferenceToObjectGuid = Guid.Empty;
                }

                if (value != null)
                {
                    ReferenceToObjectGuid = value.MyGuid;
                }
                _ReferenceToObject = value;
            }
        }
    }
}
