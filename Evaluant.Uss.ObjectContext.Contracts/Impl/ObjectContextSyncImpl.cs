using System;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Collections;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public class ObjectContextSyncImpl : ObjectContextImpl, IObjectContextSyncBase
    {
        private Transaction _CurrentTransaction;

        /// <summary>
        /// Serializes the specified persistable object.
        /// </summary>
        /// <param name="persistable">Persistable object.</param>
        public void Serialize(IPersistable persistable)
        {
            if (persistable == null)
                throw new ArgumentNullException("persistable");

            if (persistable.Entity == null)
                throw new NullReferenceException("Entity cannot be null");

            EnsureTransactionCreated();

            _CurrentTransaction.Serialize(persistable.Entity);

            // Adds the Entity to the cache after Serialize for it can throw an exception or change the Id's value
            //TrackObject(persistable);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="item">Object to serialize.</param>
        public void Serialize(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            EnsureTransactionCreated();

            // Stores the object in a temporary list to extract its entity during CommitTransaction
            if (!_PendingObjects.Contains(item))
            {
                if (resolver.Contains(item.GetHashCode()))
                    resolver.Resolve(item, Factory.PersistenceEngineFactory.Model).State = State.Modified;
                _PendingObjects.Add(item);
            }
        }

        /// <summary>
        /// Deletes the specified object.
        /// </summary>
        /// <param name="item">Object to delete.</param>
        public void Delete(object o)
        {


            if (o == null)
                throw new ArgumentNullException("o");

            IPersistable persistable = o as IPersistable;

            if (persistable == null && !resolver.Contains(RuntimeHelpers.GetHashCode(o)))
                throw new ArgumentException("This object is not persisted.");

            Entity e = persistable == null ? (Entity)resolver.Resolve(o, Factory.PersistenceEngineFactory.Model) : persistable.Entity;

            EnsureTransactionCreated();

            resolver.Clean(o);
            _CurrentTransaction.Delete(e);
        }


        /// <summary>
        /// Deletes the specified object with the specified Id.
        /// </summary>
        /// <param name="id">Object Id to delete.</param>
        public void DeleteWithId<T>(string id)
        {
            DeleteWithId(typeof(T), id);
        }

        /// <summary>
        /// Deletes the specified object with the specified Id.
        /// </summary>
        /// <param name="type">Object type to delete.</param>
        /// <param name="id">Object Id to delete.</param>
        public void DeleteWithId(Type type, string id)
        {
            EnsureTransactionCreated();

            string eussNamespace = Evaluant.Uss.MetaData.TypeResolver.ConvertNamespaceDomainToEuss(type);

            //UnTrackObject(eussNamespace, id);
            DeleteEntityCommand dec = new DeleteEntityCommand(id, eussNamespace);
            _CurrentTransaction.PushCommand(dec);
        }


        /// <summary>
        /// Serializes the specified persistable objects.
        /// </summary>
        /// <param name="persistables">Persistable objects.</param>
        public void Serialize(IEnumerable persistables)
        {
            if (persistables == null)
                throw new ArgumentNullException("persistables");

            foreach (object persistable in persistables)
                Serialize(persistable);
        }

        /// <summary>
        /// Serializes the specified persistable objects.
        /// </summary>
        /// <param name="persistables">Persistable objects.</param>
        public void Serialize<T>(IEnumerable<T> persistables)
        {
            if (persistables == null)
                throw new ArgumentNullException("persistables");

            foreach (T persistable in persistables)
                Serialize(persistable);
        }

        /// <summary>
        /// Deletes the specified persistable objects.
        /// </summary>
        /// <param name="persistables">Persistable objects.</param>
        public void Delete<T>(IEnumerable<T> persistables)
        {
            if (persistables == null)
                throw new ArgumentNullException("persistables");

            foreach (T persistable in persistables)
                Delete(persistable);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        /// Initializes the repository.
        /// </summary>
        public void InitializeRepository()
        {
            PersistenceEngine.InitializeRepository();

            // Empties the cache
            Clear();
        }

        /// <summary>
        /// Removes all IPersistable from the cache
        /// </summary>
        /// <remarks>
        /// All removes IPersistable are disconnected from the ObjectContext.
        /// </remarks>
        public void Clear()
        {
            resolver.Clean();
        }

        /// <summary>
        /// Removes an object from the first-level cache.
        /// </summary>
        /// <param name="persistable">Persistable object.</param>
        public void RemoveFromCache(object persistable)
        {
            if (persistable == null)
                throw new ArgumentNullException("persistable");

            resolver.Clean(persistable);
        }

        /// <summary>
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        public List<T> Load<T>(NLinqQuery query)
        {
            Factory.AddAssembly(typeof(T).Assembly);

            IList<Entity> result = PersistenceEngine.Load(query);

            return LoadWithEntities<T>(result);
        }

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <returns>the object found</returns>
        public T LoadSingle<T>()
        {
            return LoadSingle<T>(typeof(T));
        }

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>the object found</returns>
        public T LoadSingle<T>(Type type)
        {
            return LoadSingle<T>(type, String.Empty);
        }

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>the object found</returns>
        public T LoadSingle<T>(string constraint)
        {
            return LoadSingle<T>(new NLinqQuery(constraint));
        }

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>the object found</returns>
        public T LoadSingle<T>(Type type, string constraint)
        {
            NLinqQuery query = new NLinqQuery(new QueryExpression(new FromClause(type.FullName, new Identifier("e"), new Identifier("context")), new QueryBody(null, new SelectClause(new Identifier("e")), null)), null);
            query = query.Where(constraint);
            query = query.Page(1, 1);
            return LoadSingle<T>(query);
        }

        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        public double LoadScalar(string query)
        {
            return PersistenceEngine.LoadScalar(query);
        }

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <returns>The collection of the objects found</returns>
        public List<T> Load<T>()
        {
            return Load<T>(typeof(T));
        }

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>The collection of the objects found</returns>
        public List<T> Load<T>(string constraint)
        {
            return Load<T>(new NLinqQuery(constraint));
        }

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The collection of the objects found</returns>
        public List<T> Load<T>(Type type)
        {
            return Load<T>(CreateQuery(type));
        }

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>The collection of the objects found</returns>
        public List<T> Load<T>(Type type, string constraint)
        {
            Factory.AddAssembly(typeof(T).Assembly);

            return LoadWithEntities<T>(PersistenceEngine.Load(CreateQuery(type).Where(constraint)));
        }

        public static NLinqQuery CreateQuery(Type type)
        {
            return new NLinqQuery("from " + type.FullName + " e in context select e");
        }

        ///// <summary>
        ///// Loads the parent objects for a given relationship
        ///// </summary>
        ///// <param name="fromObject">The serialized child object whose parent must be loaded</param>
        ///// <param name="role">The role between the parent and the child object</param>
        ///// <returns></returns>
        //public IList<T> LoadParents<T>(object fromObject, string role)
        //    where T : class
        //{
        //    string id = String.Empty;

        //    IPersistable persistable = fromObject as IPersistable;

        //    if (persistable == null)
        //    {
        //        if (_GlobalProcessedObjects.ContainsKey(RuntimeHelpers.GetHashCode(fromObject)))
        //        {
        //            id = _GlobalProcessedObjects[RuntimeHelpers.GetHashCode(fromObject)].Id;
        //        }
        //        else
        //        {
        //            throw new ArgumentException("The argument is either null or is not a dynamic proxy", "fromObject");
        //        }
        //    }
        //    else
        //    {
        //        id = persistable.Entity.Id;
        //    }

        //    return Load<T>(typeof(T), String.Concat("[exists(", role, "[id('", id, "')])]"));
        //}

        /// <summary>
        /// Loads an object from an existing id
        /// </summary>
        /// <param name="type">The type of the object to retrieve</param>
        /// <param name="id">The id of the object to retrieve</param>
        /// <returns>The objects with the corresponding ids</returns>
        public T LoadWithId<T>(string id)
        {
            // Load the entities corresponding to the id
            List<T> entities = LoadWithId<T>(new string[] { id });

            // If not entity was found, return an empty array
            if (entities.Count == 0)
                return default(T);

            return entities[0];
        }

        /// <summary>
        /// Loads some objects given their ids
        /// </summary>
        /// <param name="type">The type of the object to retrieve</param>
        /// <param name="ids">The ids of the objects to retrieve</param>
        /// <returns>The objects with the corresponding ids</returns>
        public List<T> LoadWithId<T>(string[] ids)
        {
            Factory.AddAssembly(typeof(T).Assembly);
            return LoadWithEntities<T>(PersistenceEngine.LoadWithId(Evaluant.Uss.MetaData.TypeResolver.ConvertNamespaceDomainToEuss(typeof(T)), ids));
        }

        ///// <summary>
        ///// Loads all the referenced objects for a given entity and a role name. This is the Lazy Loading logic.
        ///// </summary>
        ///// <param name="entity">The Entity object from which the referenced items have to be loaded</param>
        ///// <param name="name">The name of the role to load the linked entities from</param>
        ///// <returns>The collection of the loaded objects</returns>
        //public IList LoadReference(Entity entity, string name)
        //{
        //    // Use the underlying Persistence Engine to Lazy Load the reference
        //    Entity container = new Entity(entity.Type);
        //    container.Id = entity.Id;
        //    _PersistenceEngine.LoadReference(new Entity[] { container }, new string[] { name });

        //    // Mark reference as "loaded"
        //    if (!entity.InferredReferences.Contains(name))
        //        entity.InferredReferences.Add(name);

        //    // If no entity was found, return an empty array
        //    if (container.MemberCount == 0)
        //        return new HashedList();

        //    // Create an empty array (initialized with the number of found entities)
        //    HashedList result = new HashedList();

        //    State state = entity.State;
        //    foreach (EntityEntry entry in container)
        //    {
        //        Entity e = (Entity)entry.Value;

        //        // Search this entity into the cache or create a new wrapper
        //        object instance = GetInstance(e);

        //        // Add the cached entry
        //        IPersistable persistable = instance as IPersistable;

        //        if (persistable != null)
        //        {
        //            entity.AddValue(name, persistable.Entity, typeof(Entity), State.UpToDate);
        //        }

        //        // If nothing went wrong add it to the result array
        //        result.Add(instance);
        //    }
        //    entity.State = state;

        //    // Give back a strongly typed collection
        //    return result;
        //}


        /// <summary>
        /// Begins the transaction.
        /// </summary>
        public void BeginTransaction()
        {
            EnsureTransactionDisposed();

            _CurrentTransaction = new Transaction(PersistenceEngine);

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
        /// Ensures the transaction is created.
        /// </summary>
        private void EnsureTransactionCreated()
        {
            if (_CurrentTransaction == null)
                throw new UniversalStorageException("BeginTransaction() must be called before");
        }

        /// <summary>
        /// Ensures the transaction is disposed.
        /// </summary>
        private void EnsureTransactionDisposed()
        {
            if (_CurrentTransaction != null)
                throw new UniversalStorageException("CommitTransaction() or RollbackTransaction() must be called before");
        }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        public void CommitTransaction()
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
                    Entity e = resolver.Resolve(item, Factory.PersistenceEngineFactory.Model);

                    //TrackLocallyProcessed(local);
                    _CurrentTransaction.Serialize(e);
                }
            }
            finally
            {
                enableLazyLoading = true;
            }

            try
            {
                _CurrentTransaction.Commit(PersistenceEngine, EnableEvents);

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

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        public void RollBackTransaction()
        {
            _CurrentTransaction = null;
            OnTransactionClosed();
        }


    }
}
