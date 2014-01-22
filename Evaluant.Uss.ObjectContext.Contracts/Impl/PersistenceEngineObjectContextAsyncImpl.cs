using System;
using Evaluant.Uss.Collections;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.NLinq;
using System.Reflection;
using Evaluant.Uss.Utility;
using Evaluant.Uss.Services;
using Evaluant.Uss.Domain;
using System.Collections.Generic;
namespace Evaluant.Uss.ObjectContext.Contracts
{
    public class PersistenceEngineObjectContextAsyncImpl : ObjectContextTransactionalImpl, IPersistenceEngineObjectContextAsync
    {
        public IPersistenceEngineAsync PersistenceEngineAsync
        {
            get;
            set;
        }

        protected internal PersistenceEngineObjectContextAsyncImpl(ObjectService factory, IPersistenceEngineAsync engine)
        {
            Factory = factory;
            PersistenceEngineAsync = engine;
            enableLazyLoading = true;
            foreach (Operation operation in EnumHelper.GetValues<Operation>())
            {
                ServicesCollection collection = new ServicesCollection();
                ServicesCollection parentCollection = factory.GetServices(operation);
                if (parentCollection != null)
                    collection.Add(parentCollection);
                services.Add(operation, collection);
            }
        }


        /// <summary>
        /// Begins the transaction.
        /// </summary>
        public override void BeginTransaction()
        {
            EnsureTransactionDisposed();

            _CurrentTransaction = new Transaction(PersistenceEngineAsync.FactoryAsync.Model);

            if (EnableEvents)
            {
                RegisterTransactionEvents();
            }

            _PendingObjects = new HashedList<object>();
            if (EnableEvents)
            {
                OnTransactionOpened(_CurrentTransaction);
            }
        }


        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        public override void RollBackTransaction()
        {
            _CurrentTransaction = null;
            OnTransactionClosed();
        }

        #region IObjectContextAsync Members


        public void BeginInitializeRepository()
        {
            PersistenceEngineAsync.BeginInitializeRepository(EndInitializeRepository, null);
        }


        public void BeginInitializeRepository(AsyncCallback callback)
        {
            PersistenceEngineAsync.BeginInitializeRepository(callback, null);
        }

        public void EndInitializeRepository(IAsyncResult result)
        {
            PersistenceEngineAsync.EndInitializeRepository(result);
        }


        public void BeginLoad<T>(AsyncCallback callback, NLinqQuery query)
        {
            BeginLoad<T>(callback, query, null);
        }

        public void BeginLoad<T>(AsyncCallback callback, NLinq.Expressions.Expression query)
        {
            BeginLoad<T>(callback, new NLinqQuery(query));
        }

        public void BeginLoadSingle<T>(AsyncCallback callback)
        {
            BeginLoadSingle<T>(callback, typeof(T));
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, object asyncState)
        {
            BeginLoadSingle<T>(callback, typeof(T), asyncState);
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, Type type)
        {
            BeginLoadSingle<T>(callback, CreateQuery(type));
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, Type type, object asyncState)
        {
            BeginLoadSingle<T>(callback, CreateQuery(type), asyncState);
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, string constraint)
        {
            BeginLoadSingle<T>(callback, new NLinqQuery(constraint));
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, NLinq.Expressions.Expression constraint)
        {
            BeginLoadSingle<T>(callback, new NLinqQuery(constraint));
        }

        public IList<T> EndLoad<T>(IAsyncResult result)
        {
            return LoadWithEntities<T>(PersistenceEngineAsync.EndLoad(result));
        }

        public T EndLoadSingle<T>(IAsyncResult result)
        {
            if (MetaData.TypeResolver.IsPrimitive(typeof(T)))
                return PersistenceEngineAsync.EndLoadScalar<T>(result);
            return LoadWithEntity<T>(PersistenceEngineAsync.EndLoadScalar<Entity>(result));
        }

        public T EndLoadWithId<T>(IAsyncResult result)
        {
            return LoadWithEntity<T>(PersistenceEngineAsync.EndLoadWithId(result));
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, NLinqQuery constraint)
        {
            BeginLoadSingle<T>(callback, constraint, null);
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, NLinqQuery constraint, object asyncState)
        {
            PersistenceEngineAsync.BeginLoadScalar<T>(callback, constraint, asyncState);
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, Type type, string constraint)
        {
            BeginLoadSingle<T>(callback, CreateQuery(type).Where(constraint));
        }

        public void BeginLoad<T>(AsyncCallback callback)
        {
            PersistenceEngineAsync.BeginLoad(callback, CreateQuery(typeof(T)), null);
        }

        public void BeginLoad<T>(AsyncCallback callback, string constraint)
        {
            PersistenceEngineAsync.BeginLoad(callback, new NLinqQuery(constraint), null);
        }

        public void BeginLoad<T>(AsyncCallback callback, Type type)
        {
            PersistenceEngineAsync.BeginLoad(callback, CreateQuery(typeof(T)), null);
        }

        public void BeginLoad<T>(AsyncCallback callback, Type type, string constraint)
        {
            PersistenceEngineAsync.BeginLoad(callback, CreateQuery(type).Where(constraint), null);
        }

        public void BeginLoadWithId<T>(AsyncCallback callback, string id)
        {
            PersistenceEngineAsync.BeginLoadWithId(callback, MetaData.TypeResolver.ConvertNamespaceDomainToEuss(typeof(T)), id, null);
        }

        public void BeginLoadWithId<T>(AsyncCallback callback, string[] ids)
        {
            PersistenceEngineAsync.BeginLoadWithIds(callback, MetaData.TypeResolver.ConvertNamespaceDomainToEuss(typeof(T)), ids, null);
        }

        public void BeginCommitTransaction()
        {
            BeginCommitTransaction(EndCommitTransaction);
        }


        public void BeginCommitTransaction(AsyncCallback callback)
        {
            BeginCommitTransaction(callback, null);
        }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        public void BeginCommitTransaction(AsyncCallback callback, object asyncState)
        {
            EnsureTransactionCreated();
            object entityStateBeforeSerialization = new object();
            try
            {
                enableLazyLoading = false;
                object removedFromSerialization = new object();

                foreach (object item in _PendingObjects)
                {
                    // Creates a new entity representing this "nude" object
                    Dictionary<object, Entity> local = new Dictionary<object, Entity>();
                    Entity e = resolver.Resolve(item, Factory.AsyncPersistenceEngineFactory.Model);

                    //TrackLocallyProcessed(local);
                    _CurrentTransaction.Serialize(e);
                }
            }
            finally
            {
                enableLazyLoading = true;
            }

            _CurrentTransaction.BeginCommit(callback, PersistenceEngineAsync, EnableEvents, asyncState);
        }

        public void EndCommitTransaction(IAsyncResult result)
        {
            try
            {
                _CurrentTransaction.EndCommit(result, PersistenceEngineAsync, EnableEvents);
                foreach (object item in _PendingObjects)
                {
                    Entity e;
                    if (item is IPersistable)
                        e = ((IPersistable)item).Entity;
                    else
                        e = resolver.Resolve(item, Factory.PersistenceEngineFactory.Model);
                }
                //TODO : NewIds
                //if (_CurrentTransaction.NewIds.Count > 0)
                //{
                //    foreach (object item in _PendingObjects)
                //    {
                //        Type targetType = item.GetType();

                //        FieldInfo idFieldInfo = null;

                //        if (_FieldsInfoCache.ContainsKey(targetType))
                //        {
                //            idFieldInfo = _FieldsInfoCache[targetType];
                //        }

                //        if (idFieldInfo == null)
                //        {

                //            Descriptors.PropertyDescriptor idPropertyDesc = _Factory.PersistentDescriptor.GetIdDescriptor(targetType);
                //            if (idPropertyDesc != null)
                //            {
                //                idFieldInfo = targetType.GetField(idPropertyDesc.FieldName,
                //                    BindingFlags.NonPublic | BindingFlags.Instance);

                //                if (idFieldInfo != null)
                //                {
                //                    _FieldsInfoCache.Add(targetType, idFieldInfo);
                //                }
                //            }
                //        }

                //        if (idFieldInfo != null)
                //        {
                //            Entity e = _GlobalProcessedObjects[RuntimeHelpers.GetHashCode(item)];
                //            idFieldInfo.SetValue(item, Convert.ChangeType(e.Id, idFieldInfo.FieldType));
                //        }
                //    }
                //}

            }
            finally
            {
                _CurrentTransaction = null;
                _PendingObjects.Clear();
                OnTransactionClosed();
            }

        }

        public void BeginImport(object entity)
        {
            throw new NotImplementedException();
        }

        public void BeginImport<T>(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IPersistenceEngineObjectContextAsync Members

        public T LoadWithEntity<T>(Entity entity)
        {
            if (entity == null)
                return default(T);

            T persistable = resolver.Resolve<T>(entity, Factory.PersistenceEngineFactory.Model);

            // If nothing went wrong add it to the result array
            if (persistable != null)
            {
                // Try to convert the entity in the desired type. If not possible, just ignore it
                if (persistable is T)
                {
                    OnLoaded<T>((T)persistable);
                }
            }

            return persistable;
        }

        public List<T> LoadWithEntities<T>(ICollection<Entity> entities)
        {
            List<T> result = new List<T>();
            foreach (Entity entity in entities)
            {
                result.Add(LoadWithEntity<T>(entity));
            }
            return result;
        }

        public void BeginLoadReference<T>(AsyncCallback callback, Entity entity, string name)
        {
            PersistenceEngineAsync.BeginLoadReference(callback, name, null, entity);
        }

        public IList<T> EndLoadReference<T>(IAsyncResult result)
        {
            return LoadWithEntities<T>(PersistenceEngineAsync.EndLoad(result));
        }

        public void ExecuteCommand(params Commands.Command[] command)
        {
            throw new NotImplementedException();
        }

        public void CreateRelationship(object parent, object child, string role)
        {
            throw new NotImplementedException();
        }

        public void RemoveRelationship(object parent, object child, string role)
        {
            throw new NotImplementedException();
        }

        public void BeginImport<T>(AsyncCallback callback, IEnumerable<T> entities) where T : IPersistable
        {
            throw new NotImplementedException();
        }

        public void BeginImport<T>(AsyncCallback callback, T item)
        {
            throw new NotImplementedException();
        }

        public void EndImport<T>(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        #region Events

        private void RegisterTransactionEvents()
        {
            EnsureTransactionCreated();
            _CurrentTransaction.EntityCreated += new EussEventHandler(Transaction_EntityCreated);
            _CurrentTransaction.EntityCreating += new EussCancelEventHandler(Transaction_EntityCreating);
            _CurrentTransaction.EntityDeleted += new EussEventHandler(Transaction_EntityDeleted);
            _CurrentTransaction.EntityDeleting += new EussCancelEventHandler(Transaction_EntityDeleting);
            _CurrentTransaction.EntityRelationshipCreated += new EussReferenceEventHandler(Transaction_EntityRelationshipCreated);
            _CurrentTransaction.EntityRelationshipCreating += new EussCancelReferenceEventHandler(Transaction_EntityRelationshipCreating);
            _CurrentTransaction.EntityRelationshipRemoved += new EussReferenceEventHandler(Transaction_EntityRelationshipRemoved);
            _CurrentTransaction.EntityRelationshipRemoving += new EussCancelReferenceEventHandler(Transaction_EntityRelationshipRemoving);
            _CurrentTransaction.EntityUpdated += new EussEventHandler(Transaction_EntityUpdated);
            _CurrentTransaction.EntityUpdating += new EussCancelEventHandler(Transaction_EntityUpdating);
        }

        void Transaction_EntityUpdating(object sender, CancelEntityEventArgs e)
        {
            Entity entity = (Entity)sender;
            object o = sender;
            if (o != null)
            {
                o = resolver.Resolve(entity, Factory.PersistenceEngineFactory.Model); ;
                if (o == null)
                    o = sender;
            }
            OnUpdating(o, e);
            if (e.EntityChanged && !e.Cancel)
            {
                if (!(o is IPersistable))
                {
                    resolver.Clean(o);
                    resolver.Resolve(o, Factory.PersistenceEngineFactory.Model);
                    //UnTrackObject(entity.Type, entity.Id);
                    //TrackObject(ExtractEntity(o), o);
                }
                else
                    ExtractEntity(o);
            }
        }

        void Transaction_EntityUpdated(object sender, EntityEventArgs e)
        {
            Entity entity = (Entity)sender;
            object o = sender;
            if (o != null)
            {
                o = resolver.Resolve(entity, Factory.PersistenceEngineFactory.Model);
                if (o == null)
                    o = sender;
            }
            OnUpdated(o);
        }

        void Transaction_EntityRelationshipRemoving(object sender, ReferenceCancelEventArgs e)
        {
            Entity parent = sender as Entity;
            object o = sender;
            if (sender != null)
                o = resolver.Resolve(parent, Factory.PersistenceEngineFactory.Model);
            Entity child = e.Child as Entity;
            if (child != null)
                e.Child = resolver.Resolve(child, Factory.PersistenceEngineFactory.Model);
            OnRelationshipRemoving(o, e);
        }

        void Transaction_EntityRelationshipRemoved(object sender, ReferenceEventArgs e)
        {
            Entity parent = sender as Entity;
            object o = sender;
            if (sender != null)
                o = resolver.Resolve(parent, Factory.PersistenceEngineFactory.Model);
            Entity child = e.Child as Entity;
            if (child != null)
                e.Child = resolver.Resolve(child, Factory.PersistenceEngineFactory.Model);
            OnRelationshipRemoved(o, e);
        }

        void Transaction_EntityRelationshipCreating(object sender, ReferenceCancelEventArgs e)
        {
            Entity parent = sender as Entity;
            object o = sender;
            if (sender != null)
                o = resolver.Resolve(parent, Factory.PersistenceEngineFactory.Model);
            Entity child = e.Child as Entity;
            if (child != null)
                e.Child = resolver.Resolve(child, Factory.PersistenceEngineFactory.Model);
            OnRelationshipCreating(o, e);
        }

        void Transaction_EntityRelationshipCreated(object sender, ReferenceEventArgs e)
        {
            Entity parent = sender as Entity;
            object o = sender;
            if (sender != null)
                o = resolver.Resolve(parent, Factory.PersistenceEngineFactory.Model);
            Entity child = e.Child as Entity;
            if (child != null)
                e.Child = resolver.Resolve(child, Factory.PersistenceEngineFactory.Model);
            OnRelationshipCreated(o, e);
        }

        void Transaction_EntityDeleting(object sender, CancelEntityEventArgs e)
        {
            Entity entity = (Entity)sender;
            object o = sender;
            if (o != null)
            {
                o = resolver.Resolve(entity, Factory.PersistenceEngineFactory.Model);
                if (o == null)
                    o = sender;
            }
            OnDeleting(o, e);
        }

        void Transaction_EntityDeleted(object sender, EntityEventArgs e)
        {
            Entity entity = (Entity)sender;
            object o = sender;
            if (o != null)
            {
                o = resolver.Resolve(entity, Factory.PersistenceEngineFactory.Model);
                if (o == null)
                    o = sender;
            }
            OnDeleted(o);
        }

        void Transaction_EntityCreating(object sender, CancelEntityEventArgs e)
        {
            Entity entity = (Entity)sender;
            object o = sender;
            if (o != null)
            {
                o = resolver.Resolve(entity, Factory.PersistenceEngineFactory.Model);
                if (o == null)
                    o = sender;
            }
            OnInserting(o, e);
            if (e.EntityChanged && !e.Cancel)
            {
                if (!(o is IPersistable))
                    resolver.Clean(o);
                ExtractEntity(o);
            }
        }

        void Transaction_EntityCreated(object sender, EntityEventArgs e)
        {
            Entity entity = (Entity)sender;
            object o = sender;
            if (o != null)
            {
                o = resolver.Resolve(entity, Factory.PersistenceEngineFactory.Model);
                if (o == null)
                    o = sender;
            }
            OnInserted(o);
        }

        #endregion

        #endregion

        #region IObjectContextAsync Members



        public void BeginLoad<T>(Action<IList<T>> callback, NLinqQuery query)
        {
            BeginLoad<T>(EndLoadInternal<T>, query, callback);
        }

        private void EndLoadInternal<T>(IAsyncResult result)
        {
            ((Action<IList<T>>)result.AsyncState)(EndLoad<T>(result));
        }

        private void EndLoadSingleInternal<T>(IAsyncResult result)
        {
            ((Action<T>)result.AsyncState)(EndLoadSingle<T>(result));
        }

        public void BeginLoad<T>(Action<IList<T>> callback, NLinq.Expressions.Expression query)
        {
            BeginLoad<T>(EndLoadInternal<T>, new NLinqQuery(query), callback);
        }

        public void BeginLoadSingle<T>(Action<T> callback)
        {
            BeginLoadSingle<T>(EndLoadSingleInternal<T>, callback);
        }

        public void BeginLoadSingle<T>(Action<T> callback, Type type)
        {
            BeginLoadSingle<T>(EndLoadInternal<T>, type, callback);
        }

        public void BeginLoadSingle<T>(Action<T> callback, string constraint)
        {
            BeginLoadSingle<T>(EndLoadInternal<T>, constraint, callback);
        }

        public void BeginLoadSingle<T>(Action<T> callback, NLinq.Expressions.Expression constraint)
        {
            BeginLoadSingle<T>(EndLoadSingleInternal<T>, constraint, callback);
        }

        #endregion

        #region IObjectContextAsync Members


        public void BeginInitializeRepository(AsyncCallback callback, object asyncState)
        {
            PersistenceEngineAsync.BeginInitializeRepository(callback, asyncState);
        }

        public void BeginLoad<T>(AsyncCallback callback, NLinqQuery query, object asyncState)
        {
            PersistenceEngineAsync.BeginLoad(callback, query, asyncState);
        }

        public void BeginLoad<T>(AsyncCallback callback, NLinq.Expressions.Expression query, object asyncState)
        {
            BeginLoad<T>(callback, new NLinqQuery(query), asyncState);
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, string constraint, object asyncState)
        {
            PersistenceEngineAsync.BeginLoadScalar<T>(callback, constraint, asyncState);
        }

        public void BeginLoadSingle<T>(AsyncCallback callback, NLinq.Expressions.Expression constraint, object asyncState)
        {
            if (MetaData.TypeResolver.IsPrimitive(typeof(T)))
                PersistenceEngineAsync.BeginLoadScalar<T>(callback, new NLinqQuery(constraint), asyncState);
            else
                PersistenceEngineAsync.BeginLoadScalar<Entity>(callback, new NLinqQuery(constraint), asyncState);
        }

        #endregion
    }
}
