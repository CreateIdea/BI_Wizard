using System;
using System.Collections.Generic;

namespace BI_Wizard.Helper
{
    public class UniqueStringsWithUsageList
    {
        private int _totalNrOfStrings;
        private Dictionary<string, int> _dictionary;

        public int TotalNrOfStrings
        {
            get
            {
                return _totalNrOfStrings;
            }
        }

        public UniqueStringsWithUsageList()
        {
            _dictionary = new Dictionary<string, int>();
            _totalNrOfStrings = 0;
        }

        public void AddString(string str)
        {
            AddString(str, 1);
        }
        public void AddString(string str, int count)
        {
            if (_dictionary.ContainsKey(str))
            {
                _dictionary[str] += count;
            }
            else
            {
                if (count > 0)
                {

                    _dictionary.Add(str, count);
                }
            }
            _totalNrOfStrings += count;
        }

        public void RemoveString(string str)
        {
            RemoveString(str, 1);
        }

        public void RemoveString(string str, int count)
        {
            if (_dictionary.ContainsKey(str))
            {
                _totalNrOfStrings = _totalNrOfStrings - Math.Min(_dictionary[str], count);
                _dictionary[str] = _dictionary[str] - count;
                if (_dictionary[str] <= 0)
                {
                    _dictionary.Remove(str);
                }
            }
        }

        public void Clear()
        {
            _totalNrOfStrings = 0;
            _dictionary.Clear();
        }

        //"• {0} ({1})x", true, true 
        public string ToString(bool addNewLine, bool showCount)
        {
            string resStr = string.Empty;
            foreach (var i in _dictionary)
            {
                if (showCount)
                {
                    resStr = resStr + string.Format(i.Key, i.Value);
                }
                else
                {
                    resStr = resStr + i.Key;
                }

                if (addNewLine)
                    resStr = resStr + Environment.NewLine;
            }
            return resStr;
        }


    }
}
