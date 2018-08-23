using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml.Serialization;

namespace BI_Wizard.Helper
{
    [Serializable]
    public class Base_VM : DependencyObject, INotifyPropertyChanged
    {
        private Guid _MyGuid;
        public Guid MyGuid
        {
            get
            {
                //there is a verry low chance that a  Guid.NewGuid() is empty.
                //therfore we will try again to get new guid if the new guid is empty.
                while (_MyGuid == Guid.Empty)
                {
                    _MyGuid = Guid.NewGuid();
                    if (_MyGuid != Guid.Empty)
                      SetMyGuid(_MyGuid);
                }
                return _MyGuid;
            }
            set
            {
                if (value == Guid.Empty)
                {
                    _MyGuid = MyGuid;
                }
                else
                {
                    if (SetProperty(ref _MyGuid, value))
                        SetMyGuid(_MyGuid);
                }
            }
        }

        public virtual void SetMyGuid(Guid aGuid)
        {
            
        }

        private bool _isDirty=false;

        [XmlIgnore]
        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        public Base_VM()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnore]
        public Boolean NotifyEnabled = true;

        protected virtual void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null && NotifyEnabled)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            _isDirty = true;
            storage = value;
            this.NotifyPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public void NotifyAllPropertyChanged()
        {
            NotifyPropertyChanged(string.Empty);
        }
    }
}
