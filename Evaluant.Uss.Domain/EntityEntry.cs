using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using Evaluant.Uss.Utility;

namespace Evaluant.Uss.Domain
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Entry : IStateable
    {
        protected Entry() { }

        static Entry()
        {
            if (createGeneric == null)
            {
                foreach (MethodInfo mi in typeof(Entry).GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    if (mi.IsGenericMethodDefinition)
                    {
                        createGeneric = mi;
                        break;
                    }
                }
            }
        }

        private static MethodInfo createGeneric;

        public virtual bool IsEntity { get { return Value is Entity; } }
        public virtual bool IsMultiple { get { return false; } }

        public string Name { get; set; }
        public State State { get; set; }
        public virtual object Value { get; set; }
        public virtual Type Type { get; set; }
        public Entity Parent { get; set; }
        public TypeCode TypeCode { get; private set; }

        public static Entry<T> Create<T>(string name, State state, T value)
        {
            TypeCode tc = Helper.GetTypeCode(value);
            if (tc == TypeCode.Empty)
                tc = Type.GetTypeCode(typeof(T));
            return new Entry<T>() { Name = name, State = state, Value = value, TypeCode = tc };
        }

        public virtual Entry Clone()
        {
            return Create(Name, State, Value, Value.GetType());
        }

        public static Entry Create(string name, State state, object value, Type type)
        {
            return (Entry)createGeneric.MakeGenericMethod(type).Invoke(null, new object[] { name, state, value });
        }
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public class Entry<T> : Entry
    {
        public virtual T TypedValue
        {
            get { return (T)this.Value; }
            set { this.Value = value; }
        }

        public override Type Type
        {
            get
            {
                return typeof(T);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override Entry Clone()
        {
            return Create<T>(Name, State, TypedValue);
        }
    }


#if !SILVERLIGHT
    [Serializable]
#endif
    public class MultipleEntry : Entry<List<Entry<Entity>>>, ICollection<Entry<Entity>>, ICollection<Entry>, ICollection<Entity>, ICollection
    {
        public MultipleEntry(string name)
        {
            Name = name;
        }

        public override Entry Clone()
        {
            MultipleEntry clone = new MultipleEntry(Name);
            foreach (Entry entry in this)
                clone.Add((Entry<Entity>)entry.Clone());
            return clone;
        }

        public override object Value
        {
            get
            {
                return entries;
            }
            set
            {
                entries = (List<Entry<Entity>>)value;
            }
        }

        List<Entry<Entity>> entries = new List<Entry<Entity>>();

        public override bool IsEntity { get { return true; } }
        public override bool IsMultiple { get { return true; } }

        #region ICollection<Entry> Members

        public void Add(Entry<Entity> item)
        {
            entries.Add(item);
        }

        public void Clear()
        {
            entries.Clear();
        }

        public bool Contains(Entry<Entity> item)
        {
            return entries.Contains(item);
        }

        public void CopyTo(Entry<Entity>[] array, int arrayIndex)
        {
            entries.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return entries.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<Entry>)entries).IsReadOnly; }
        }

        public bool Remove(Entry<Entity> item)
        {
            return entries.Remove(item);
        }

        #endregion

        #region IEnumerable<Entry> Members

        public IEnumerator<Entry<Entity>> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public void Remove(string entityId)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (((Entry<Entity>)entries[i]).TypedValue.Id == entityId)
                {
                    entries.RemoveAt(i);
                    return;
                }
            }
        }

        public bool Contains(Entity e)
        {
            foreach (Entry entry in this)
            {
                if (entry.Value == e)
                    return true;
            }
            return false;
        }

        #region IEnumerable<Entity> Members

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
        {
            foreach (Entry entry in entries)
            {
                yield return (Entity)entry.Value;
            }
        }

        #endregion

        #region ICollection<Entity> Members

        public void Add(Entity item)
        {
            Add(Entry.Create(Name, State.New, item));
        }

        public void CopyTo(Entity[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Entity item)
        {
            Remove(item.Id);
            return true;
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            ((ICollection)entries).CopyTo(array, index);
        }

        public bool IsSynchronized
        {
            get { return ((ICollection)entries).IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return ((ICollection)entries).SyncRoot; }
        }

        #endregion

        public void Add(Entry item)
        {
            Add((Entry<Entity>)item);
        }

        public bool Contains(Entry item)
        {
            return Contains((Entry<Entity>)item);
        }

        public void CopyTo(Entry[] array, int arrayIndex)
        {
            CopyTo((Entry<Entity>[])array, arrayIndex);
        }

        public bool Remove(Entry item)
        {
            return Remove((Entry<Entity>)item);
        }

        IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
