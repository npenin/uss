using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Collections;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Services;
using Evaluant.NLinq;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public interface IObjectContextNotifyer : IServiceProvider
    {
        event EussEventHandler ObjectLoaded;

        event EussCancelEventHandler ObjectUpdating;
        event EussEventHandler ObjectUpdated;

        event EussCancelEventHandler ObjectInserting;
        event EussEventHandler ObjectInserted;

        event EussCancelEventHandler ObjectDeleting;
        event EussEventHandler ObjectDeleted;

        event EussCancelReferenceEventHandler ObjectRelationCreating;
        event EussReferenceEventHandler ObjectRelationCreated;

        event EussCancelReferenceEventHandler ObjectRelationRemoving;
        event EussReferenceEventHandler ObjectRelationRemoved;
    }

    public interface IObjectContextBase : IDisposable
    {
        /// <summary>
        /// Gets or sets the parent factory.
        /// </summary>
        /// <value></value>
        ObjectService Factory { get; set; }

        void Attach<T>(T item) where T : class;

        /// <summary>
        /// Detaches the specified graph.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <returns>A cloned object without any relation with a Persistence Engine</returns>
        /// <remarks>Only the loaded relationship are cloned during the process.</remarks>
        T Detach<T>(T graph) where T : class;

        /// <summary>
        /// Determines whether the specified property is null.
        /// </summary>
        /// <param name="item">The entity.</param>
        /// <param name="property">The property.</param>
        /// <returns>
        /// 	<c>true</c> if the specified property is null; otherwise, <c>false</c>.
        /// </returns>
        bool IsNull(object item, string property);

        /// <summary>
        /// Sets a property to null.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="property">The property.</param>
        void SetNull(object item, string property);

        /// <summary>
        /// Gets the id of an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The identifier of the entity</returns>
        string GetId(object entity);

        /// <summary>
        /// Sets the id of an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="id">The id.</param>
        void SetId(object entity, string id);


        #region Services

        void AddService(IService service, params Operation[] operations);

        void RemoveService(IService service, params Operation[] operations);

        #endregion

        #region IServiceProvider Membres

        ServicesCollection GetServices(Operation operation);

        T GetService<T>();

        #endregion

        bool IsLazyLoadingEnabled { get; set; }

        /// <summary>
        /// Deletes the specified object.
        /// </summary>
        /// <param name="item">Object to delete.</param>
        void Delete(object o);

        /// <summary>
        /// Deletes the specified persistable objects.
        /// </summary>
        /// <param name="persistables">Persistable objects.</param>
        void Delete<T>(IEnumerable<T> persistables);
    }

    public interface IObjectContextTransactionalBase : IObjectContextBase
    {
        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="item">Object to serialize.</param>
        void Serialize(object item);

        /// <summary>
        /// Deletes the specified object with the specified Id.
        /// </summary>
        /// <param name="id">Object Id to delete.</param>
        void DeleteWithId<T>(string id);

        /// <summary>
        /// Deletes the specified object with the specified Id.
        /// </summary>
        /// <param name="type">Object type to delete.</param>
        /// <param name="id">Object Id to delete.</param>
        void DeleteWithId(Type type, string id);


        /// <summary>
        /// Serializes the specified persistable objects.
        /// </summary>
        /// <param name="persistables">Persistable objects.</param>
        void Serialize(IEnumerable persistables);

        /// <summary>
        /// Serializes the specified persistable objects.
        /// </summary>
        /// <param name="persistables">Persistable objects.</param>
        void Serialize<T>(IEnumerable<T> persistables);

        /// <summary>
        /// Removes all IPersistable from the cache
        /// </summary>
        /// <remarks>
        /// All removed IPersistable are disconnected from the ObjectContext.
        /// </remarks>
        void Clear();

        /// <summary>
        /// Removes an object from the first-level cache.
        /// </summary>
        /// <param name="persistable">Persistable object.</param>
        void RemoveFromCache(object persistable);

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        void RollBackTransaction();

        /// <summary>
        /// Attaches a detached object.
        /// </summary>
        void SerializeDetached(object item);

        /// <summary>
        /// Attaches a detached object.
        /// </summary>
        void SerializeDetached(object item, string id);
    }

    public interface IObjectContextSyncBase : IObjectContextBase
    {
        /// <summary>
        /// Initializes the repository.
        /// </summary>
        void InitializeRepository();

        /// <summary>
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        List<T> Load<T>(NLinqQuery query);

        /// <summary>
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        List<T> Load<T>(NLinq.Expressions.Expression query);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <returns>the object found</returns>
        T LoadSingle<T>();

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>the object found</returns>
        T LoadSingle<T>(Type type);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>the object found</returns>
        T LoadSingle<T>(string constraint);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>the object found</returns>
        T LoadSingle<T>(NLinq.Expressions.Expression constraint);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>the object found</returns>
        T LoadSingle<T>(NLinqQuery constraint);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>the object found</returns>
        T LoadSingle<T>(Type type, string constraint);

        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        double LoadScalar(string query);


        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        T LoadScalar<T>(string query);


        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        T LoadScalar<T, U>(string query) where T : class, IEnumerable<U>;

        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        double LoadScalar(NLinqQuery query);


        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        T LoadScalar<T>(NLinqQuery query);


        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        T LoadScalar<T, U>(NLinqQuery query) where T : class, IEnumerable<U>;

        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        double LoadScalar(NLinq.Expressions.Expression query);


        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        T LoadScalar<T>(NLinq.Expressions.Expression query);


        /// <summary>
        /// Compute an arithmetic opath expression 
        /// </summary>
        /// <param name="opath">the opath, must start with one of these functions (count(), avg(), sum(), min(), max())</param>
        /// <returns>the result of the expression</returns>
        T LoadScalar<T, U>(NLinq.Expressions.Expression query) where T : class, IEnumerable<U>;

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <returns>The collection of the objects found</returns>
        List<T> Load<T>();

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>The collection of the objects found</returns>
        List<T> Load<T>(string constraint);

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The collection of the objects found</returns>
        List<T> Load<T>(Type type);

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>The collection of the objects found</returns>
        List<T> Load<T>(Type type, string constraint);

        /// <summary>
        /// Loads the parent objects for a given relationship
        /// </summary>
        /// <param name="fromObject">The serialized child object whose parent must be loaded</param>
        /// <param name="role">The role between the parent and the child object</param>
        /// <returns></returns>
        //IList<T> LoadParents<T>(object fromObject, string role);

        /// <summary>
        /// Loads an object from an existing id
        /// </summary>
        /// <param name="type">The type of the object to retrieve</param>
        /// <param name="id">The id of the object to retrieve</param>
        /// <returns>The objects with the corresponding ids</returns>
        T LoadWithId<T>(string id);

        /// <summary>
        /// Loads some objects given their ids
        /// </summary>
        /// <param name="type">The type of the object to retrieve</param>
        /// <param name="ids">The ids of the objects to retrieve</param>
        /// <returns>The objects with the corresponding ids</returns>
        List<T> LoadWithId<T>(string[] ids);

        /// <summary>
        /// Imports the specified entity and its related.
        /// </summary>
        /// <param name="entity">Entity to import.</param>
        void Import<T>(T entity) where T : class, IPersistable;

        /// <summary>
        /// Imports the specified entities and their related.
        /// </summary>
        /// <param name="entities">Entities to import.</param>
        /// <remarks>
        /// Only the related objects which are already loaded will be imported. 
        /// Thus it alows you to import only one node of the object graph.
        /// </remarks>
        void Import<T>(IEnumerable<T> entities) where T : class, IPersistable;

        void CommitChanges();
    }

    public interface IObjectContext : IObjectContextSyncBase, IObjectContextTransactionalBase, IObjectContextWithTracking
    {
        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void CommitTransaction();
    }
}
