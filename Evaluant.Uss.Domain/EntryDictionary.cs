using System;
using System.Collections.Generic;
using System.Text;
#if !SILVERLIGHT
using System.Collections.Specialized;
#endif

namespace Evaluant.Uss.Domain
{
    public class EntryDictionary : IDictionary<string, Entry>, IList<Entry>
    {
        public EntryDictionary()
        {

        }

        public EntryDictionary(IEnumerable<Entry> entries)
        {
            foreach (Entry entry in entries)
            {
                Add(entry);
            }
        }

        IList<string> keys = new List<string>();
        IList<Entry> values = new List<Entry>();

        #region IList<Entry> Members

        public int IndexOf(Entry item)
        {
            return keys.IndexOf(item.Name);
        }

        public void Insert(int index, Entry item)
        {
            keys.Insert(index, item.Name);
            values.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }

        public Entry this[int index]
        {
            get
            {
                return values[index];
            }
            set
            {
                values[index] = value;
                keys[index] = value.Name;
            }
        }

        #endregion

        #region ICollection<Entry> Members

        public void Add(Entry item)
        {
            keys.Add(item.Name);
            values.Add(item);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public bool Contains(Entry item)
        {
            return values.Contains(item);
        }

        public void CopyTo(Entry[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return keys.IsReadOnly; }
        }

        public bool Remove(Entry item)
        {
            if (values.Remove(item))
            {
                keys.Remove(item.Name);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<Entry> Members

        public IEnumerator<Entry> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDictionary<string,Entry> Members

        public void Add(string key, Entry value)
        {
            Add(value);
        }

        public bool ContainsKey(string key)
        {
            return keys.Contains(key);
        }

        public ICollection<string> Keys
        {
            get { return keys; }
        }

        public bool Remove(string key)
        {
            int keyIndex = keys.IndexOf(key);
            if (keyIndex < 0)
                return false;
            keys.RemoveAt(keyIndex);
            values.RemoveAt(keyIndex);
            return true;
        }

        public bool TryGetValue(string key, out Entry value)
        {
            int keyIndex = keys.IndexOf(key);
            if (keyIndex < 0)
            {
                value = null;
                return false;
            }
            value = values[keyIndex];
            return true;

        }

        public ICollection<Entry> Values
        {
            get { return values; }
        }

        public Entry this[string key]
        {
            get
            {
                int keyIndex = keys.IndexOf(key);
                if (keyIndex < 0)
                    throw new KeyNotFoundException();
                return values[keyIndex];
            }
            set
            {
                int keyIndex = keys.IndexOf(key);
                values[keyIndex] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,Entry>> Members

        public void Add(KeyValuePair<string, Entry> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, Entry> item)
        {
            return ContainsKey(item.Value.Name);
        }

        public void CopyTo(KeyValuePair<string, Entry>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, Entry> item)
        {
            return Remove(item.Value.Name);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,Entry>> Members

        IEnumerator<KeyValuePair<string, Entry>> IEnumerable<KeyValuePair<string, Entry>>.GetEnumerator()
        {
            foreach (Entry entry in values)
            {
                yield return new KeyValuePair<string, Entry>(entry.Name, entry);
            }
        }

        #endregion
    }
}
