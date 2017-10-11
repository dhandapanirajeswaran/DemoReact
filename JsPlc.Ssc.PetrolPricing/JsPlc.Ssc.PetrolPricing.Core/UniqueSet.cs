using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
    public class UniqueSet
    {
        private Dictionary<string, string> _set = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public void Clear()
        {
            _set.Clear();
        }

        public void AddKeyAndValue(int key, object value)
        {
            AddKeyAndValue(key.ToString(), value.ToString());
        }

        public void AddKeyAndValue(string key, object value)
        {
            AddKeyAndValue(key, value.ToString());
        }

        public void AddKeyAndValue(string key, string value)
        {
            if (_set.ContainsKey(key))
            {
                if (String.Compare(value, _set[key], true) == 0)
                    return;
                throw new ArgumentException("Key: " + key + " and Value: " + value + " is not unique. An exising key with a different value already exists");
            }
            _set.Add(key, value);
        }

        public bool IsUniqueKeyAndValue(int key, object value)
        {
            return IsUniqueKeyAndValue(key.ToString(), value.ToString());
        }

        public bool IsUniqueKeyAndValue(string key, object value)
        {
            return IsUniqueKeyAndValue(key, value.ToString());
        }

        public bool IsUniqueKeyAndValue(string key, string value)
        {
            if (_set.ContainsKey(key))
                return String.Compare(value, _set[key], true) == 0;
            return true;
        }

        public void AddUniqueKey(int key, object value)
        {
            AddUniqueKey(key.ToString(), value);
        }

        public void AddUniqueKey(string key, object value)
        {
            AddUniqueKey(key, value.ToString());
        }

        public void AddUniqueKey(string key, string value)
        {
            if (_set.ContainsKey(key))
                return;
            _set.Add(key, value);
        }

        public bool ContainsKey(int key)
        {
            return _set.ContainsKey(key.ToString());
        }

        public bool ContainsKey(string key)
        {
            return _set.ContainsKey(key);
        }

        public string this[int key]
        {
            get { return _set[key.ToString()]; }
        }

        public string this[string key]
        {
            get { return _set[key]; }
        }
    }
}
