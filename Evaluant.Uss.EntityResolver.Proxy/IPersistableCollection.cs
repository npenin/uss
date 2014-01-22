using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Evaluant.Uss;
using Evaluant.Uss.Collections;
using System.Diagnostics;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Utility;

namespace Evaluant.Uss.EntityResolver.Proxy
{
    internal sealed class CollectionDebugView
    {

        private IEnumerable _collection;

        public CollectionDebugView(IEnumerable collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get
            {
                List<object> list = new List<object>();
                foreach (object o in _collection)
                    list.Add(o);
                return list.ToArray();
            }
        }
    }

#if !SILVERLIGHT
    [Serializable()]
#endif
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebugView))]
    public class IPersistableCollection<T> : IList<T>, INotifyCollectionChanged<T>//, System.Collections.Specialized.INotifyCollectionChanged
#if !SILVERLIGHT
        , ISerializable//, IList
#endif

    {
        #region Members

        private IList<T> _List;
        protected IPersistenceEngineObjectContext oc;
        protected IPersistenceEngineObjectContextAsync ocAsync;
        protected IPersistable _Parent;
        protected string _Role;
        protected MultipleEntry multipleEntry;

        #endregion

        #region Ctor

        public IPersistableCollection()
        {
            _List = new HashedList<T>();
            
        }

        public IPersistableCollection(IPersistenceEngineObjectContextAsync oc, IPersistable parent, string role, IEnumerable<T> data)
            : this()
        {
            this.ocAsync = oc;
            _Parent = parent;
            _Role = role;
            multipleEntry = new MultipleEntry(role);
            if (data != null)
            {
                foreach (T o in data)
                    _List.Add(o);
            }
            else
            {
                // create by dynamicProxy, fill the collection if already loaded
                if (parent.Entity.InferredReferences.Contains(role) && oc != null)
                {
                    MultipleEntry entities = (MultipleEntry)parent.Entity[role];
                    IList<T> items = ocAsync.LoadWithEntities<T>(entities);
                    foreach (T o in items)
                        _List.Add(o);
                }
            }

        }

        public IPersistableCollection(IPersistenceEngineObjectContext oc, IPersistable parent, string role, IEnumerable<T> data)
            : this()
        {
            this.oc = oc;
            _Parent = parent;
            _Role = role;
            multipleEntry = new MultipleEntry(role);

            if (data != null)
            {
                foreach (T o in data)
                    _List.Add(o);
            }
            else
            {
                // create by dynamicProxy, fill the collection if already loaded
                if (parent.Entity.InferredReferences.Contains(role) && oc != null)
                {
                    MultipleEntry entities = (MultipleEntry)parent.Entity[role];
                    IList<T> items = oc.LoadWithEntities<T>(entities);
                    foreach (T o in items)
                        _List.Add(o);
                }
            }
        }

        public IPersistableCollection(ICollection<T> value)
        {
            this.AddRange(value);
        }

        #endregion

        public bool IsAttached
        {
            get { return _Parent != null && _Parent.Entity != null; }
        }

        internal bool IsLoaded()
        {
            if (IsAttached)
                return _Parent.Entity.InferredReferences.Contains(_Role);

            return _List != null;
        }

        internal void EnsureLoaded()
        {
            if (IsLoaded() || (ObjectContext != null && !ObjectContext.IsLazyLoadingEnabled) || (ObjectContextAsync != null && !ObjectContextAsync.IsLazyLoadingEnabled))
                return;

            if (oc != null && IsAttached)
            {
                _List = oc.LoadReference<T>(_Parent.Entity, _Role);
                _Parent.Entity.Add(_Role, multipleEntry);
                // mark reference as "loaded"
                if (!_Parent.Entity.InferredReferences.Contains(_Role))
                    _Parent.Entity.InferredReferences.Add(_Role);

            }
            else if (ocAsync != null && IsAttached)
            {
                ocAsync.BeginLoadReference<T>(ReferencesLoaded, _Parent.Entity, _Role);
                _Parent.Entity.Add(_Role, multipleEntry);
            }
            else // Standalone collection
            {
                _List = new HashedList<T>();
            }
        }

        private void ReferencesLoaded(IAsyncResult result)
        {
            _List = ocAsync.EndLoadReference<T>(result);
            // mark reference as "loaded"
            if (!_Parent.Entity.InferredReferences.Contains(_Role))
                _Parent.Entity.InferredReferences.Add(_Role);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, new List<T>(_List));
        }

        public IPersistenceEngineObjectContext ObjectContext
        {
            get { return oc; }
            set { oc = value; }
        }

        public IPersistenceEngineObjectContextAsync ObjectContextAsync
        {
            get { return ocAsync; }
            set { ocAsync = value; }
        }


        public string Role
        {
            get { return _Role; }
            set { _Role = value; }
        }

        public T this[int index]
        {
            get
            {
                EnsureLoaded();
                return ((T)(_List[index]));
            }
            set
            {
                EnsureLoaded();
                if (IsAttached)
                {
                    _Parent.Entity.Remove(_Role, ((IPersistable)_List[index]).Entity);
                    _Parent.Entity.Add(_Role, GetEntity(value));
                }

                _List[index] = value;
            }
        }

        private Entity GetEntity(object item)
        {
            IPersistable persistable = item as IPersistable;

            if (persistable != null)
                return persistable.Entity;
            else
                return oc.Resolver.Resolve(item, oc.Factory.PersistenceEngineFactory.Model);
        }

        public void Add(T item)
        {
            AddItem(item);
        }

        void AddItem(T item)
        {
            EnsureLoaded();
            // if the collection is not autonomous, add the added entity to the parent's role

            if (IsAttached)
            {
                multipleEntry.Add(GetEntity(item));
            }

            _List.Add(item);
        }

        public void AddRange(IEnumerable<T> value)
        {
            EnsureLoaded();
            foreach (T item in value)
            {
                this.Add(item);
            }
        }

        public bool Contains(T value)
        {
            EnsureLoaded();
            return _List.Contains(value);
        }

        public void CopyTo(T[] array, int index)
        {
            EnsureLoaded();
            _List.CopyTo(array, index);
        }

        public int IndexOf(T value)
        {
            EnsureLoaded();
            return _List.IndexOf(value);
        }

        public void Clear()
        {
            EnsureLoaded();

            if (IsAttached)
                _Parent.Entity.Remove(_Role);

            _List.Clear();
        }

        public void Insert(int index, T value)
        {
            EnsureLoaded();
            _List.Insert(index, value);
        }

        public int Count
        {
            get
            {
                EnsureLoaded();
                return _List.Count;
            }
        }

        public void RemoveAt(int index)
        {
            EnsureLoaded();
            _List.RemoveAt(index);
        }

        public bool Remove(T item)
        {
            EnsureLoaded();
            if (IsAttached)
            {
                _Parent.Entity.Remove(_Role, GetEntity(item));

                // mark reference as "loaded"
                if (!_Parent.Entity.InferredReferences.Contains(_Role))
                    _Parent.Entity.InferredReferences.Add(_Role);
            }
            if (_List.Remove(item))
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
                return true;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            EnsureLoaded();
            return new IEnumeratorWrapper<T>(_List.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        //public bool IsSynchronized
        //{
        //    get
        //    {
        //        EnsureLoaded();
        //        return _List.IsSynchronized;
        //    }
        //}

        //public object SyncRoot
        //{
        //    get
        //    {
        //        EnsureLoaded();
        //        return _List.SyncRoot;
        //    }
        //}

        //public bool IsFixedSize
        //{
        //    get
        //    {
        //        EnsureLoaded();
        //        return _List.IsFixedSize;
        //    }
        //}

        public bool IsReadOnly
        {
            get
            {
                EnsureLoaded();
                return _List.IsReadOnly;
            }
        }

#if !SILVERLIGHT
        #region ISerialiazable implementation

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(List<T>));

            List<T> list = new List<T>();

            if (_List != null)
                foreach (T item in _List)
                    list.Add(item);

            System.Reflection.MemberInfo[] memberInfos = FormatterServices.GetSerializableMembers(typeof(List<T>));
            object[] memberValues = FormatterServices.GetObjectData(list, memberInfos);

            for (int i = 0; i < memberInfos.Length; i++)
                info.AddValue(memberInfos[i].Name, memberValues[i]);
        }

        #endregion
#endif

        private class IEnumeratorWrapper<Y> : IEnumerator<Y>
        {
            private IEnumerator _Enumerator;

            public IEnumeratorWrapper(IEnumerator enumerator)
            {
                _Enumerator = enumerator;
            }

            #region IEnumerator<Y> Members

            public Y Current
            {
                get { return (Y)_Enumerator.Current; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                _Enumerator = null;
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return _Enumerator.Current; }
            }

            public bool MoveNext()
            {
                return _Enumerator.MoveNext();
            }

            public void Reset()
            {
                _Enumerator.Reset();
            }

            #endregion
        }

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler<T> CollectionChanged;

        private void OnCollectionChanged(Evaluant.Uss.Utility.NotifyCollectionChangedAction action, T item)
        {
            OnCollectionChanged(action, item, default(T), -1);
        }

        private void OnCollectionChanged(Evaluant.Uss.Utility.NotifyCollectionChangedAction action, IEnumerable<T> item)
        {
            OnCollectionChanged(action, item, null, -1);
        }

        private void OnCollectionChanged(Evaluant.Uss.Utility.NotifyCollectionChangedAction action, IEnumerable<T> newItem, IEnumerable<T> oldItem, int index)
        {
            NotifyCollectionChangedEventArgs<T> te = null;
            //System.Collections.Specialized.NotifyCollectionChangedEventArgs e = null;
            switch (action)
            {
                case Evaluant.Uss.Utility.NotifyCollectionChangedAction.Add:
                case Evaluant.Uss.Utility.NotifyCollectionChangedAction.Remove:
                    te = new NotifyCollectionChangedEventArgs<T>(action, newItem, oldItem, index);
                    //e = new System.Collections.Specialized.NotifyCollectionChangedEventArgs((System.Collections.Specialized.NotifyCollectionChangedAction)action, null, index);
                    break;
                case Evaluant.Uss.Utility.NotifyCollectionChangedAction.Replace:
                    te = new NotifyCollectionChangedEventArgs<T>(action, newItem, oldItem, index);
                    //e = new System.Collections.Specialized.NotifyCollectionChangedEventArgs((System.Collections.Specialized.NotifyCollectionChangedAction)action, newItem, oldItem, index);
                    break;
                case Evaluant.Uss.Utility.NotifyCollectionChangedAction.Reset:
                    te = new NotifyCollectionChangedEventArgs<T>(action);
                    //e = new System.Collections.Specialized.NotifyCollectionChangedEventArgs((System.Collections.Specialized.NotifyCollectionChangedAction)action);
                    break;
            }
            if (CollectionChanged != null)
                CollectionChanged(this, te);
            //if (untypedCollectionChanged != null)
            //    untypedCollectionChanged(this, e);
        }


        private void OnCollectionChanged(Evaluant.Uss.Utility.NotifyCollectionChangedAction action, T newItem, T oldItem, int index)
        {
            NotifyCollectionChangedEventArgs<T> te = null;
            //System.Collections.Specialized.NotifyCollectionChangedEventArgs e = null;
            switch (action)
            {
                case Evaluant.Uss.Utility.NotifyCollectionChangedAction.Add:
                case Evaluant.Uss.Utility.NotifyCollectionChangedAction.Remove:
                    te = new NotifyCollectionChangedEventArgs<T>(action, newItem, index);
                    //e = new System.Collections.Specialized.NotifyCollectionChangedEventArgs((System.Collections.Specialized.NotifyCollectionChangedAction)action, newItem, index);
                    break;
                case Evaluant.Uss.Utility.NotifyCollectionChangedAction.Replace:
                    te = new NotifyCollectionChangedEventArgs<T>(action, newItem, oldItem, index);
                    //e = new System.Collections.Specialized.NotifyCollectionChangedEventArgs((System.Collections.Specialized.NotifyCollectionChangedAction)action, newItem, oldItem, index);
                    break;
                case Evaluant.Uss.Utility.NotifyCollectionChangedAction.Reset:
                    te = new NotifyCollectionChangedEventArgs<T>(action);
                    //e = new System.Collections.Specialized.NotifyCollectionChangedEventArgs((System.Collections.Specialized.NotifyCollectionChangedAction)action);
                    break;
            }
            if (CollectionChanged != null)
                CollectionChanged(this, te);
            //if (untypedCollectionChanged != null)
            //    untypedCollectionChanged(this, e);
        }


        #endregion

        //#region INotifyCollectionChanged Members

        //private event System.Collections.Specialized.NotifyCollectionChangedEventHandler untypedCollectionChanged;

        //event System.Collections.Specialized.NotifyCollectionChangedEventHandler System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged
        //{
        //    add { untypedCollectionChanged += value; }
        //    remove { untypedCollectionChanged -= value; }
        //}

        //#endregion
    }
}
