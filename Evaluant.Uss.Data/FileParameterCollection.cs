using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using Evaluant.Uss.Data.FileClient;
using System.Data.SqlClient;
using System.Collections;

namespace Evaluant.Uss.Data
{
    public class FileParameterCollection : DbParameterCollection
    {
        List<object> items = new List<object>();

        public override int Add(object value)
        {
            items.Add(value);
            return items.Count - 1;
        }

        public override void AddRange(Array values)
        {
            foreach (object item in values)
            {
                items.Add(item);
            }
        }

        public override void Clear()
        {
            items.Clear();
        }

        public override bool Contains(string value)
        {
            return items.IndexOf(value) != -1;
        }

        public override bool Contains(object value)
        {
            return items.IndexOf(value) != -1;
        }

        public override void CopyTo(Array array, int index)
        {
            items.ToArray().CopyTo(array, index);
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return items.GetEnumerator();
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            int index = this.IndexOf(parameterName);
            return GetParameter(index);
        }

        protected override DbParameter GetParameter(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            return items[index] as DbParameter;
        }

        public override int IndexOf(string parameterName)
        {
            int index = 0;
            foreach (FileParameter fp in items)
            {
                if (fp.ParameterName == parameterName)
                {
                    return index; 
                }

                index++;
            }

            return -1;
        }

        public override int IndexOf(object value)
        {
            return items.IndexOf(value);
        }

        public override void Insert(int index, object value)
        {
            items.Insert(index, value);
        }

        public override bool IsFixedSize
        {
            get { return ((IList)items).IsFixedSize;  }
        }

        public override bool IsReadOnly
        {
            get { return ((IList)items).IsReadOnly; }
        }

        public override bool IsSynchronized
        {
            get { return ((IList)items).IsSynchronized; }
        }

        public override void Remove(object value)
        {
            items.Remove(value);
        }

        public override void RemoveAt(string parameterName)
        {
            RemoveAt(IndexOf(parameterName));
        }

        public override void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            SetParameter(IndexOf(parameterName), value);
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            object parameter = items[index];
            items[index] = value;
        }

        public override object SyncRoot
        {
            get { return ((IList)items).SyncRoot; }
        }
    }
}
