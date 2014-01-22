using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
#if !SILVERLIGHT
using System.Collections.Specialized;
#endif
using System.Globalization;
using System.Xml.Serialization;
using Evaluant.Uss.Utility;

namespace Evaluant.Uss.Domain
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Entity : INotifyPropertyChanging, INotifyPropertyChanged, IEditableObject, IStateable, ICollection<Entry>
    {
        [Flags]
        public enum LoadMetaData { AttributesLoaded = 1, ReferenceLoaded = 2 };

        public Entity()
        {
            Id = Guid.NewGuid().ToString();
        }

        public Entity(string entityType)
        {
            Id = Guid.NewGuid().ToString();
            Type = entityType;
            InferredReferences = new List<string>();
        }

        public string Id { get; set; }
        public string Type { get; internal protected set; }
        public List<string> InferredReferences { get; set; }

        EntryDictionary currentEntries = new EntryDictionary();
        EntryDictionary oldEntries;

        public LoadMetaData MetaData { get; set; }

        #region IEditableObject Members

        public void BeginEdit()
        {
            oldEntries = currentEntries;
            currentEntries = new EntryDictionary(oldEntries);
        }

        public void CancelEdit()
        {
            currentEntries = oldEntries;
        }

        public void EndEdit()
        {
            oldEntries = null;
        }

        public void AcceptChanges()
        {
            if (State == State.Updating)
                return;

            State = State.Updating;

            List<Entry> entriesToRemove = new List<Entry>();
            foreach (Entry entry in currentEntries.Values)
            {
                if (entry.State == State.Deleted)
                {
                    entriesToRemove.Add(entry);
                    continue;
                }

                if (entry.IsMultiple)
                {
                    foreach (Entry<Entity> subEntry in (MultipleEntry)entry)
                    {
                        if (subEntry.State == State.Deleted)
                        {
                            entriesToRemove.Add(subEntry);
                            continue;
                        }

                        subEntry.TypedValue.AcceptChanges();
                        subEntry.State = State.UpToDate;
                    }
                }
                else if (entry.IsEntity)
                    ((Entry<Entity>)entry).TypedValue.AcceptChanges();

                entry.State = State.UpToDate;

            }

            // Removes all entries which are marked as deleted
            foreach (Entry entry in entriesToRemove)
            {
                Remove(entry.Name, (Entity)entry.Value);
            }

            State = State.UpToDate;
        }

        #endregion

        public Entry this[string name]
        {
            get
            {
                Entry result;
                if (TryGet(name, out result))
                    return result;
                return null;
            }
            set
            {
                if (currentEntries.ContainsKey(name))
                    RaisePropertyChanging(name);
                else
                {
                    if (string.IsNullOrEmpty(value.Name))
                        value.Name = name;
                }
                currentEntries[name] = value;
                RaisePropertyChanged(name, value);
            }
        }

        public EntryDictionary EntityEntries { get { return currentEntries; } }

        private void RaisePropertyChanged(string name, Entry value)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private bool RaisePropertyChanging(string name)
        {
            PropertyChangingEventArgs args = new PropertyChangingEventArgs(name);
            if (PropertyChanging != null)
                PropertyChanging(this, args);

            return args.Cancel;
        }

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IStateable Members

        public State State { get; set; }

        #endregion


        #region IEnumerable<Entry> Members

        public IEnumerator<Entry> GetEnumerator()
        {
            return EntityEntries.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public void Remove(string role, Entity e)
        {
            Entry entry;
            if (currentEntries.TryGetValue(role, out entry))
            {
                if (entry.IsMultiple)
                    ((MultipleEntry)entry).Remove(e.Id);
                else
                    Remove(role);
            }
        }

        public void Remove(string roleName, string entityId)
        {
            if (currentEntries[roleName].IsMultiple)
                ((MultipleEntry)currentEntries[roleName]).Remove(entityId);
            else
                Remove(roleName);
        }

        public void Remove(Entity processedEntity)
        {
            Dictionary<string, string> referenceToRemove = new Dictionary<string, string>();
            foreach (Entry e in currentEntries.Values)
            {
                if (e.IsEntity)
                {
                    if (e.Value == processedEntity || e.IsMultiple)
                        referenceToRemove.Add(e.Name, processedEntity.Id);
                }
            }
            foreach (KeyValuePair<string, string> reference in referenceToRemove)
                Remove(reference.Key, reference.Value);
        }

        //Should not be removed : used by proxies
        public void Remove(string referenceName)
        {
            currentEntries.Remove(referenceName);
        }

        public void Add(Entry entry)
        {
            entry.Parent = this;
            currentEntries.Add(entry.Name, entry);
        }

        //Should not be removed : used by proxy
        public void Add(string name, Entity entity, bool isMultiple)
        {
            Entry entry;
            if (isMultiple)
            {
                entry = new MultipleEntry(name) { Entry.Create(name, State.New, entity) };
            }
            else
                entry = Entry.Create(name, State.New, entity);

            Add(name, entry);
        }

        public void Add(string name, Entity entity, bool isMultiple, State state)
        {
            Entry entry;
            if (isMultiple)
            {
                entry = new MultipleEntry(name) { Entry.Create(name, state, entity) };
                entry.State = state;
            }
            else
                entry = Entry.Create(name, State.New, entity);

            Add(name, entry);
        }

        public void Add(string name, Entry entry)
        {
            entry.Name = name;
            Add(entry);
        }

        public void Add(string name, object value)
        {
            Add(name, value, value.GetType(), State.New);
        }

        public void Add(string name, object value, Type type, State state)
        {
            Entry entry = Entry.Create(name, state, value, type);
            entry.Parent = this;
            currentEntries.Add(name, entry);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public Entity Clone()
        {
            return Clone(new string[0]);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <param name="attributes">
        /// null	        -> do not load attribute
        /// new string[0]	-> load all attributes
        /// other values    -> load the specified attributes</param>
        /// <returns></returns>
        public Entity Clone(string[] attributes)
        {

            Entity entity = new Entity(this.Type);
            entity.Id = this.Id;

            if (attributes != null)
            {
                foreach (Entry ee in this)
                {
                    if (!ee.IsEntity && (attributes.Length == 0 || Array.IndexOf(attributes, ee.Name) != -1))
                        entity.Add(ee.Name, ee.Value, ee.Type, ee.State);
                }
            }

            entity.State = this.State;

            return entity;
        }

        public void RemoveReference(params string[] references)
        {
            if (references == null || references.Length == 0)
            {
                List<string> referencesList = new List<string>();

                foreach (Entry entry in currentEntries)
                {
                    if (entry.IsEntity || entry.IsMultiple)
                        referencesList.Add(entry.Name);
                }

                references = referencesList.ToArray();
            }

            foreach (string reference in references)
                currentEntries.Remove(reference);
        }

        public bool IsNull(string property)
        {
            return EntityEntries[property].Value == null;
        }


        #region Getters

        public bool GetBoolean(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(bool))
            {
                return ((bool)obj1);
            }

            return Convert.ToBoolean(obj1, CultureInfo.InvariantCulture);
        }

        public byte GetByte(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(byte))
            {
                return ((byte)obj1);
            }

            return Convert.ToByte(obj1, CultureInfo.InvariantCulture);
        }

        public char GetChar(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(char))
            {
                return ((char)obj1);
            }

            return Convert.ToChar(obj1, CultureInfo.InvariantCulture);
        }

        public DateTime GetDateTime(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(DateTime))
            {
                return ((DateTime)obj1);
            }

            return Convert.ToDateTime(obj1, CultureInfo.InvariantCulture);
        }

        public decimal GetDecimal(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(decimal))
            {
                return ((decimal)obj1);
            }

            return Convert.ToDecimal(obj1, CultureInfo.InvariantCulture);
        }

        public double GetDouble(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(double))
            {
                return ((double)obj1);
            }

            return Convert.ToDouble(obj1, CultureInfo.InvariantCulture);
        }

        public short GetInt16(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(short))
            {
                return ((short)obj1);
            }

            return Convert.ToInt16(obj1, CultureInfo.InvariantCulture);
        }

        public int GetInt32(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(int))
            {
                return ((int)obj1);
            }

            return Convert.ToInt32(obj1, CultureInfo.InvariantCulture);
        }

        public long GetInt64(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(long))
            {
                return ((long)obj1);
            }

            return Convert.ToInt64(obj1, CultureInfo.InvariantCulture);
        }

        public sbyte GetSByte(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(sbyte))
            {
                return ((sbyte)obj1);
            }

            return Convert.ToSByte(obj1, CultureInfo.InvariantCulture);
        }

        public float GetSingle(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(float))
            {
                return ((float)obj1);
            }

            return Convert.ToSingle(obj1, CultureInfo.InvariantCulture);
        }

        public string GetString(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if ((type1 == typeof(string)) || (obj1 == null))
            {
                return ((string)obj1);
            }

            return Convert.ToString(obj1, CultureInfo.InvariantCulture);
        }

        public ushort GetUInt16(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(ushort))
            {
                return ((ushort)obj1);
            }

            return Convert.ToUInt16(obj1, CultureInfo.InvariantCulture);
        }

        public uint GetUInt32(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(uint))
            {
                return ((uint)obj1);
            }

            return Convert.ToUInt32(obj1, CultureInfo.InvariantCulture);
        }

        public ulong GetUInt64(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            if (type1 == typeof(ulong))
            {
                return ((ulong)obj1);
            }

            return Convert.ToUInt64(obj1, CultureInfo.InvariantCulture);
        }

        public Entity GetEntity(string name)
        {
            Type type1;
            object obj1 = this.GetElement(name, out type1);
            return obj1 as Entity;
        }

        /// <summary>
        /// Gets a value with its underlying type.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        /// <returns></returns>
        private object GetElement(string name, out Type type)
        {
            Entry entry = this[name];
            if (entry == null)
            {
                type = null;
                return null;
            }

            type = entry.Type;
            return entry.Value;
        }

        #endregion

        /// <summary>
        /// Sets the value of an attribute if the values differ.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void SetValue(string name, object value)
        {
            Entry entry = this[name];

            if (entry != null)
            {
                if (entry.Value == null || !entry.Value.Equals(value))
                {
                    //if (value == null)
                    //    entry.Type = typeof(object);
                    //else
                    //    entry.Type = value.GetType();

                    entry.Value = value;

                    if (entry.State == State.UpToDate)
                        entry.State = State.Modified;
                }
            }
            else
                Add(name, value);
        }

        //Should not be removed : used by proxy
        public object GetValue(string name)
        {
            Entry entry;
            if (TryGet(name, out entry))
                return entry.Value;
            return null;
        }

        public bool TryGet(string entryName, out Entry entry)
        {
            return currentEntries.TryGetValue(entryName, out entry);
        }

        #region ICollection<Entry> Members

        void ICollection<Entry>.Clear()
        {
            EntityEntries.Clear();
        }

        bool ICollection<Entry>.Contains(Entry item)
        {
            return EntityEntries.Contains(item);
        }

        void ICollection<Entry>.CopyTo(Entry[] array, int arrayIndex)
        {
            EntityEntries.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return EntityEntries.Count; }
        }

        bool ICollection<Entry>.IsReadOnly
        {
            get { return EntityEntries.IsReadOnly; }
        }

        bool ICollection<Entry>.Remove(Entry item)
        {
            return EntityEntries.Remove(item);
        }

        #endregion

        #region IEnumerable<Entry> Members

        IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator()
        {
            return EntityEntries.GetEnumerator();
        }

        #endregion
    }
}
