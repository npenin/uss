using System;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.NLinq;
using Evaluant.Uss.Collections;
using Evaluant.Uss.Utility;
using Evaluant.Uss.Services;
using Evaluant.Uss.Domain;
using System.Runtime.CompilerServices;
using Evaluant.Uss.Commands;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using Evaluant.NLinq.Expressions;
using System.IO;
using Evaluant.Uss.Serializer;
using Evaluant.Uss.ObjectContext.Contracts.Visitors;
using Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public class PersistenceEngineObjectContextImpl : PersistenceEngineObjectContextAsyncImpl, IPersistenceEngineObjectContext, IObjectContextWithTracking
    {
        static MethodInfo load;

        static PersistenceEngineObjectContextImpl()
        {
            load = typeof(ObjectContextImpl).GetMethod("Load", new Type[] { typeof(NLinqQuery) });
        }

        public IPersistenceEngine PersistenceEngine
        {
            get;
            set;
        }

        protected internal PersistenceEngineObjectContextImpl(ObjectService factory, IPersistenceEngine engine)
            : base(factory, engine)
        {
            PersistenceEngine = engine;
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
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        public List<T> Load<T>(NLinqQuery query)
        {
            Factory.AddAssembly(typeof(T).Assembly);

            bool isAnonymous = typeof(T).IsGenericType
                && typeof(T).Name.StartsWith("<>f__AnonymousType");

            if (!isAnonymous
                && !PersistenceEngine.Factory.Model.Entities.ContainsKey(typeof(T).FullName))
                return PersistenceEngine.LoadScalar<List<T>, T>(query);

            IList<Entity> result = PersistenceEngine.Load(query);

            if (isAnonymous)
                foreach (var entity in result)
                {
                    entity.Type = typeof(T).FullName;
                }

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
        public override void BeginTransaction()
        {
            EnsureTransactionDisposed();

            _CurrentTransaction = new Transaction(PersistenceEngine.Factory.Model);

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
                    //PersistenceEngine.CreateId(e);
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
        public override void RollBackTransaction()
        {
            _CurrentTransaction = null;
            OnTransactionClosed();
        }

        /// <summary>
        /// Adds a set of Command object to the current transaction
        /// </summary>
        public void ExecuteCommand(params Command[] commands)
        {
            EnsureTransactionCreated();

            _CurrentTransaction.PushCommand(commands);
        }

        private void CreateRelationship(Entity parent, Entity child, string role)
        {
            EnsureTransactionCreated();

            CreateReferenceCommand command = new CreateReferenceCommand(PersistenceEngine.Factory.Model.Entities[parent.Type].References[role], parent, child);
            _CurrentTransaction.PushCommand(command);
            _CurrentTransaction.AddConcernedEntity(parent);
            _CurrentTransaction.AddConcernedEntity(child);
        }

        /// <summary>
        /// Creates a relationship. Be careful, Parent and Child objects must have their Id property initialized !
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="child">Child.</param>
        /// <param name="role">Role.</param>
        public void CreateRelationship(object parent, object child, string role)
        {
            try
            {
                enableLazyLoading = false;     // to be sure not to load unloaded references if not needed

                Dictionary<object, Entity> localProcessed = new Dictionary<object, Entity>();

                Entity parentEntity = resolver.Resolve(parent, Factory.PersistenceEngineFactory.Model);

                Entity childEntity = resolver.Resolve(child, Factory.PersistenceEngineFactory.Model);

                //TrackLocallyProcessed(localProcessed);

                //TrackObject(parentEntity, parent);
                //TrackObject(childEntity, child);

                CreateRelationship(parentEntity, childEntity, role);
            }
            finally
            {
                enableLazyLoading = true;
            }
        }

        private void RemoveRelationship(Entity parent, Entity child, string role)
        {
            EnsureTransactionCreated();

            DeleteReferenceCommand command = new DeleteReferenceCommand(PersistenceEngine.Factory.Model.Entities[parent.Type].References[role], parent, child);
            _CurrentTransaction.PushCommand(command);
            _CurrentTransaction.AddConcernedEntity(parent);
            _CurrentTransaction.AddConcernedEntity(child);
        }

        /// <summary>
        /// Removes a relationship.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="child">Child.</param>
        /// <param name="role">Role.</param>
        public void RemoveRelationship(object parent, object child, string role)
        {
            Dictionary<object, Entity> localProcessed = new Dictionary<object, Entity>();

            Entity parentEntity = resolver.Resolve(parent, Factory.PersistenceEngineFactory.Model);

            Entity childEntity = resolver.Resolve(child, Factory.PersistenceEngineFactory.Model);

            RemoveRelationship(parentEntity, childEntity, role);
        }

        /// <summary>
        /// Imports the specified entity and its related.
        /// </summary>
        /// <param name="entity">Entity to import.</param>
        public void Import(object entity)
        {
            Import(entity as IPersistable);
        }

        /// <summary>
        /// Imports the specified entities and their related.
        /// </summary>
        /// <param name="entities">Entities to import.</param>
        /// <remarks>
        /// Only the related objects which are already loaded will be imported. 
        /// Thus it alows you to import only one node of the object graph.
        /// </remarks>
        public void Import(IEnumerable entities)
        {
            foreach (IPersistable entity in entities)
                Import(entity);
        }

        #region IObjectContext Members


        public List<T> Load<T>(Evaluant.NLinq.Expressions.Expression query)
        {
            return Load<T>(new NLinqQuery(query));
        }

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

            // If no entity was found, return an empty array
            if (entities.Count == 0)
                return new List<T>();

            // Create an empty array (initialized with the number of found entities)
            List<T> result = new List<T>();

            // Use a for loop as the Enumerator looses the order
            foreach (Entity entity in entities)
            {
                //Entity entity = entities[i];
                if (entity.Type == null)
                    entity.Type = typeof(T).FullName;

                // Gets a wrapper over this entity (a new one or from the cache)
                T persistable = resolver.Resolve<T>(entity, Factory.PersistenceEngineFactory.Model);

                // If nothing went wrong add it to the result array
                if (persistable != null)
                {
                    // Try to convert the entity in the desired type. If not possible, just ignore it
                    if (persistable is T)
                    {
                        OnLoaded<T>((T)persistable);
                        result.Add((T)persistable);
                    }
                }
            }

            // Give back a strongly typed collection
            return result;
        }

        public T LoadSingle<T>(Evaluant.NLinq.Expressions.Expression constraint)
        {
            if (constraint.ExpressionType == ExpressionTypes.Binary)
                return LoadSingle<T>(CreateQuery(typeof(T)).Where(constraint));
            return LoadSingle<T>(new NLinqQuery(constraint));
        }

        public T LoadSingle<T>(NLinqQuery constraint)
        {
            Factory.AddAssembly(typeof(T).Assembly);

            if (!PersistenceEngine.Factory.Model.Entities.ContainsKey(typeof(T).FullName))
                return PersistenceEngine.LoadScalar<T>(constraint);

            return LoadWithEntity<T>(PersistenceEngine.LoadScalar<Entity>(constraint));
        }

        public T LoadScalar<T>(string query)
        {
            return LoadScalar<T>(new NLinqQuery(query));
        }

        public T LoadScalar<T, U>(string query) where T : class, IEnumerable<U>
        {
            return LoadScalar<T, U>(new NLinqQuery(query));
        }

        public double LoadScalar(NLinqQuery query)
        {
            return PersistenceEngine.LoadScalar(query);
        }

        public T LoadScalar<T>(NLinqQuery query)
        {
            return PersistenceEngine.LoadScalar<T>(query);
        }

        public T LoadScalar<T, U>(NLinqQuery query) where T : class, IEnumerable<U>
        {
            return PersistenceEngine.LoadScalar<T, U>(query);
        }

        public double LoadScalar(Evaluant.NLinq.Expressions.Expression query)
        {
            return LoadScalar(new NLinqQuery(query));
        }

        public T LoadScalar<T>(Evaluant.NLinq.Expressions.Expression query)
        {
            return LoadScalar<T>(new NLinqQuery(query));
        }

        public T LoadScalar<T, U>(Evaluant.NLinq.Expressions.Expression query) where T : class, IEnumerable<U>
        {
            return LoadScalar<T, U>(new NLinqQuery(query));
        }

        public IList<T> LoadReference<T>(Entity entity, string name)
        {
            if (!IsLazyLoadingEnabled)
                return new List<T>();
            var query = new NLinqQuery("from e0 in context select e0." + name);
            ((QueryExpression)query.Expression).From.Type = entity.Type;

            var entityModel = PersistenceEngine.Factory.Model.Entities[entity.Type];
            BinaryExpression constraint = null;
            foreach (Model.Attribute attribute in entityModel.Attributes.Values)
            {
                if (!attribute.IsId)
                    continue;
                constraint = new BinaryExpression(BinaryExpressionType.And, constraint,
                    new BinaryExpression(BinaryExpressionType.Equal, new MemberExpression(new Identifier(attribute.Name), new Identifier("e0")), new Parameter(attribute.Name)));
                query.Parameters.Add(attribute.Name, entity[attribute.Name].Value);
            }
            ((QueryExpression)query.Expression).QueryBody.Clauses.Add(new WhereClause(constraint));
            return Load<T>(query);
        }

        public void Infer<T>(IEnumerable<T> items, string name)
        {
            List<Entity> entities = new List<Entity>();
            foreach (T item in items)
            {
                IPersistable p = item as IPersistable;
                if (p != null)
                    entities.Add(p.Entity);
            }

            PersistenceEngine.LoadReference(name, entities.ToArray());
        }

        #endregion

        #region IObjectContextSyncBase Members


        public void Import<T>(T entity) where T : class, IPersistable
        {
            Import((IPersistable)entity);
        }

        #endregion

        #region IObjectContextSyncBase Members


        public void CommitChanges()
        {
            BeginTransaction();
            Serialize(resolver.GetAll());
            CommitTransaction();
        }

        #endregion
    }
}
