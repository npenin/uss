using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss
{
    public class HashSet<T> : ICollection<T>
    {
        public HashSet(IEnumerable<T> enumerables)
        {
            foreach (T item in enumerables)
                Add(item);
        }

        ICollection<T> innerCollection = new List<T>();

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (innerCollection.Contains(item))
                return;
            innerCollection.Add(item);
        }

        public void Clear()
        {
            innerCollection.Clear();
        }

        public bool Contains(T item)
        {
            return innerCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerCollection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return innerCollection.Count; }
        }

        public bool IsReadOnly
        {
            get { return innerCollection.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            return innerCollection.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return innerCollection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return innerCollection.GetEnumerator();
        }

        #endregion
    }
}
