using System;
using System.Collections;
#if !SILVERLIGHT
using System.Collections.Specialized;
#endif
using System.Collections.Generic;

namespace Evaluant.Uss.Collections
{
    /// <summary>
    /// Description résumée de HashedArray.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class HashedList<T> : IList<T>
    {
        private readonly IList<T> list = new List<T>();
        private readonly IDictionary<T, object> index = new Dictionary<T, object>();

        public virtual void Add(T value)
        {
            index.Add(value, null);
            list.Add(value);
        }

        public void AddRange(IEnumerable<T> values)
        {
            foreach (T value in values)
                Add(value);
        }

        public void Clear()
        {
            index.Clear();
            list.Clear();
        }

        public bool Contains(T value)
        {
            return index.ContainsKey(value);
        }

        public int IndexOf(T value)
        {
            return list.IndexOf(value);
        }

        public void Insert(int index, T value)
        {
            list.Insert(index, value);
            this.index.Add(value, null);
        }

        public bool Remove(T value)
        {
            index.Remove(value);
            return list.Remove(value);
        }

        public void RemoveAt(int index)
        {
            this.index.Remove(list[index]);
            list.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }

        public void CopyTo(T[] array, int index)
        {
            list.CopyTo(array, index);
        }

        public int Count
        {
            get { return list.Count; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public bool IsReadOnly
        {
            get { return list.IsReadOnly; }
        }
    }
}
