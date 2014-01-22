using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Services;
using Evaluant.Uss.Domain;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Runtime.CompilerServices;
using System.Collections;
using Evaluant.Uss.MetaData;
using Evaluant.NLinq;
using Evaluant.Uss.Collections;
using Evaluant.Uss.Commands;
//using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using Evaluant.Uss.Utility;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.Serializer;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public abstract class ObjectContextImpl : IObjectContextTransactionalBase, IObjectContextNotifyer
    {
        public event EussEventHandler ObjectLoaded;

        public event EussCancelEventHandler ObjectUpdating;
        public event EussEventHandler ObjectUpdated;

        public event EussCancelEventHandler ObjectInserting;
        public event EussEventHandler ObjectInserted;

        public event EussCancelEventHandler ObjectDeleting;
        public event EussEventHandler ObjectDeleted;

        public event EussCancelReferenceEventHandler ObjectRelationCreating;
        public event EussReferenceEventHandler ObjectRelationCreated;

        public event EussCancelReferenceEventHandler ObjectRelationRemoving;
        public event EussReferenceEventHandler ObjectRelationRemoved;

        public event EussEventHandler TransactionOpened;
        public event EussEventHandler TransactionClosed;

        protected IdentityMap resolver;

        public IEntityResolver Resolver
        {
            get { return resolver; }
            set { resolver = new IdentityMap(value); }
        }

        /// <summary>
        /// Gets or sets the parent factory.
        /// </summary>
        /// <value></value>
        public ObjectService Factory
        {
            get;
            set;
        }

        public bool EnableEvents
        {
            get;
            set;
        }

        protected bool enableLazyLoading;

        protected HashedList<object> _PendingObjects;

        public bool IsLazyLoadingEnabled { get { return enableLazyLoading; } set { enableLazyLoading = value; } }

        public Entity ExtractEntity(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            return resolver.Resolve(item, Factory.PersistenceEngineFactory.Model);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
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

        public static NLinqQuery CreateQuery(Type type)
        {
            return new NLinqQuery("from " + type.FullName + " e in context select e");
        }


        #region Events

        #region Raising

        protected void OnLoaded<T>(T item)
        {
            ServiceContext context = new ServiceContext(Operation.Loaded, EntityEventArgs.Empty);
            GetServices(Operation.Loaded).Visit(item, context);
            if (ObjectLoaded != null)
                ObjectLoaded(item, EntityEventArgs.Empty);
        }
        protected void OnInserting<T>(T item, CancelEntityEventArgs e)
        {
            ServiceContext context = new ServiceContext(Operation.Inserting, e);
            GetServices(Operation.Inserting).Visit(item, context);
            if (ObjectInserting != null)
                ObjectInserting(item, e);
        }
        protected void OnInserted<T>(T item)
        {
            ServiceContext context = new ServiceContext(Operation.Inserted, EntityEventArgs.Empty);
            GetServices(Operation.Inserted).Visit(item, context);
            if (ObjectInserted != null)
                ObjectInserted(item, EntityEventArgs.Empty);
        }
        protected void OnUpdating<T>(T item, CancelEntityEventArgs e)
        {
            ServiceContext context = new ServiceContext(Operation.Updating, e);
            GetServices(Operation.Updating).Visit(item, context);
            if (ObjectUpdating != null)
                ObjectUpdating(item, e);
        }
        protected void OnUpdated<T>(T item)
        {
            ServiceContext context = new ServiceContext(Operation.Updated, EntityEventArgs.Empty);
            GetServices(Operation.Updated).Visit(item, context);
            if (ObjectUpdated != null)
                ObjectUpdated(item, EntityEventArgs.Empty);
        }
        protected void OnDeleting<T>(T item, CancelEntityEventArgs e)
        {
            ServiceContext context = new ServiceContext(Operation.Deleting, e);
            GetServices(Operation.Deleting).Visit(item, context);
            if (ObjectDeleting != null)
                ObjectDeleting(item, e);
        }
        protected void OnDeleted<T>(T item)
        {
            ServiceContext context = new ServiceContext(Operation.Deleted, EntityEventArgs.Empty);
            GetServices(Operation.Deleted).Visit(item, context);
            if (ObjectDeleted != null)
                ObjectDeleted(item, EntityEventArgs.Empty);
        }

        protected void OnRelationshipRemoving<T>(T item, ReferenceCancelEventArgs e)
        {
            GetServices(Operation.RemovingRelationship).Visit(item, e.Child, ReferenceServiceContext.Create(Operation.RemovingRelationship, e.Child, e));
            if (ObjectRelationRemoving != null)
                ObjectRelationRemoving(item, e);
        }

        protected void OnRelationshipRemoved<T>(T item, ReferenceEventArgs e)
        {
            GetServices(Operation.RemovedRelationship).Visit(item, e.Child, ReferenceServiceContext.Create(Operation.RemovedRelationship, e.Child, e));
            if (ObjectRelationRemoved != null)
                ObjectRelationRemoved(item, e);
        }

        protected void OnRelationshipCreating<T>(T item, ReferenceCancelEventArgs e)
        {
            GetServices(Operation.CreatingRelationship).Visit(item, e.Child, ReferenceServiceContext.Create(Operation.CreatingRelationship, e.Child, e));
            if (ObjectRelationCreating != null)
                ObjectRelationCreating(item, e);
        }

        protected void OnRelationshipCreated<T>(T item, ReferenceEventArgs e)
        {
            GetServices(Operation.CreatedRelationship).Visit(item, e.Child, ReferenceServiceContext.Create(Operation.CreatedRelationship, e.Child, e));
            if (ObjectRelationCreated != null)
                ObjectRelationCreated(item, e);
        }

        protected void OnTransactionOpened(Transaction t)
        {
            if (TransactionOpened != null)
                TransactionOpened(t, EntityEventArgs.Empty);
        }

        protected void OnTransactionClosed()
        {
            if (TransactionClosed != null)
                TransactionClosed(this, EntityEventArgs.Empty);
        }

        #endregion

        #endregion

        public void Attach<T>(T item) where T : class
        {
            resolver.Attach<T>(item, Factory.PersistenceEngineFactory.Model);
        }

        /// <summary>
        /// Detaches the specified graph.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <returns>A cloned object without any relation with a Persistence Engine</returns>
        /// <remarks>Only the loaded relationship are cloned during the process.</remarks>
        public T Detach<T>(T graph) where T : class
        {
            // By using BinarySerialization the graph is cloned.
            // Moreover as the ISerializable.GetObjectData() method is defined on each Proxy type,
            // the serialization type is overriden to be replaced by the original type.
            // Thus deserializing the stream generates an original type array.
            // IPersistableCollection is also ISerializable to replace its instances by ArrayList.

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, graph);
                ms.Position = 0;
                T obj = (T)formatter.Deserialize(ms);
                return obj;
            }

        }

        /// <summary>
        /// Determines whether the specified property is null.
        /// </summary>
        /// <param name="item">The entity.</param>
        /// <param name="property">The property.</param>
        /// <returns>
        /// 	<c>true</c> if the specified property is null; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNull(object item, string property)
        {
            IPersistable persistable = item as IPersistable;

            if (persistable == null)
                throw new ObjectContextException("You can only use ObjectContext loaded objects");

            return persistable.Entity.IsNull(property);
        }

        /// <summary>
        /// Sets a property to null.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="property">The property.</param>
        public void SetNull(object item, string property)
        {
            IPersistable persistable = item as IPersistable;

            if (persistable == null)
                throw new ObjectContextException("You can only use ObjectContext loaded objects");

            persistable.Entity.SetValue(property, null);
        }

        /// <summary>
        /// Gets the id of an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The identifier of the entity</returns>
        public string GetId(object entity)
        {
            IPersistable persistable = entity as IPersistable;

            if (persistable != null)
            {
                return persistable.Entity.Id;
            }
            else if (resolver.Contains(RuntimeHelpers.GetHashCode(entity)))
            {
                return resolver.Resolve(entity, Factory.PersistenceEngineFactory.Model).Id;
            }
            else
                throw new ObjectContextException("You can only use ObjectContext loaded objects");
            //TODO : Should be done in the resolver
            //else
            //{
            //    Evaluant.Uss.ObjectContext.Descriptors.PropertyDescriptor pd = Factory.PersistentDescriptor.GetIdDescriptor(entity.GetType());

            //    if (pd != null)
            //    {
            //        return Convert.ToString(pd.GetValue(entity.GetType(), entity));
            //    }

            //    throw new PersistenceManagerException("You can only use ObjectContext loaded objects");
            //}
        }

        /// <summary>
        /// Sets the id of an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="id">The id.</param>
        public void SetId(object entity, string id)
        {
            IPersistable persistable = entity as IPersistable;

            if (persistable != null)
            {
                persistable.Entity.Id = id;
            }
            else if (resolver.Contains(RuntimeHelpers.GetHashCode(entity)))
            {
                resolver.Resolve(entity, Factory.PersistenceEngineFactory.Model).Id = id;
            }
            else
            {
                throw new ObjectContextException("You can only use ObjectContext loaded objects");
            }
        }

        /// <summary>
        /// Imports the specified entities and their related.
        /// </summary>
        /// <param name="entities">Entities to import.</param>
        /// <remarks>
        /// Only the related objects which are already loaded will be imported. 
        /// Thus it alows you to import only one node of the object graph.
        /// </remarks>
        public void Import<T>(IEnumerable<T> entities) where T : class, IPersistable
        {
            foreach (IPersistable entity in entities)
                Import(entity);
        }

        /// <summary>
        /// Imports the specified entity and its related.
        /// </summary>
        /// <param name="entity">Entity to import.</param>
        public void Import(IPersistable item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            resolver.Import(item, Factory.PersistenceEngineFactory.Model);
            item.Entity.State = State.New;
        }

        #region Services

        protected IDictionary<Operation, ServicesCollection> services = new Dictionary<Operation, ServicesCollection>();

        public void AddService(IService service, params Operation[] operations)
        {
            if (operations.Length == 0)
                operations = EnumHelper.GetValues<Operation>();
            foreach (Operation operation in operations)
            {
                if (services.ContainsKey(operation))
                    services[operation].Add(service);
                else
                {
                    ServicesCollection servicesCollection = new ServicesCollection();
                    servicesCollection.Add(service);
                    services.Add(operation, servicesCollection);
                }
            }
        }

        public void RemoveService(IService service, params Operation[] operations)
        {
            if (operations.Length > 0)
            {
                foreach (Operation operation in operations)
                {
                    if (services.ContainsKey(operation))
                        services[operation].Add(service);
                    else
                    {
                        ServicesCollection servicesCollection = new ServicesCollection();
                        servicesCollection.Add(service);
                        services.Add(operation, servicesCollection);
                    }
                }
            }
            else
            {
                foreach (ServicesCollection collection in services.Values)
                {
                    if (collection.Contains(service))
                        collection.Remove(service);
                }
            }
        }

        #endregion

        #region IServiceProvider Membres

        public object GetService(Type serviceType)
        {
            foreach (ServicesCollection collection in services.Values)
            {
                if (collection.Contains(serviceType))
                    return collection[serviceType];
            }
            return null;
        }

        public ServicesCollection GetServices(Operation operation)
        {
            if (services.ContainsKey(operation))
                return services[operation];
            return null;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        #endregion



        #region IObjectContextTransactionalBase Members

        public abstract void Serialize(object item);

        public abstract void Delete(object o);

        public abstract void DeleteWithId<T>(string id);

        public abstract void DeleteWithId(Type type, string id);

        public abstract void Serialize(IEnumerable persistables);

        public abstract void Serialize<T>(IEnumerable<T> persistables);

        public abstract void Delete<T>(IEnumerable<T> persistables);

        public abstract void BeginTransaction();

        public abstract void RollBackTransaction();

        public abstract void SerializeDetached(object item);

        public abstract void SerializeDetached(object item, string id);

        #endregion
    }
}
