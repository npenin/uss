using System;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Collections.Generic;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public interface IPersistenceEngineObjectContextAsync : IObjectContextAsync
    {
        #region Events
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

        event EussEventHandler TransactionOpened;
        event EussEventHandler TransactionClosed;
        #endregion

        IPersistenceEngineAsync PersistenceEngineAsync { get; set; }

        IEntityResolver Resolver { get; set; }

        bool EnableEvents { get; set; }

        /// <summary>
        /// Serializes the specified persistable object.
        /// </summary>
        /// <param name="persistable">Persistable object.</param>
        void Serialize(IPersistable persistable);

        List<T> LoadWithEntities<T>(ICollection<Entity> entities);

        /// <summary>
        /// Loads all the referenced objects for a given entity and a role name. This is the Lazy Loading logic.
        /// </summary>
        /// <param name="entity">The Entity object from which the referenced items have to be loaded</param>
        /// <param name="name">The name of the role to load the linked entities from</param>
        /// <returns>The collection of the loaded objects</returns>
        void BeginLoadReference<T>(AsyncCallback callback, Entity entity, string name);

        IList<T> EndLoadReference<T>(IAsyncResult result);

        /// <summary>
        /// Adds a Command object to the current transaction
        /// </summary>
        void ExecuteCommand(params Command[] command);

        /// <summary>
        /// Creates a relationship. Be careful, Parent and Child objects must have their Id property initialized !
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="child">Child.</param>
        /// <param name="role">Role.</param>
        void CreateRelationship(object parent, object child, string role);

        /// <summary>
        /// Removes a relationship.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="child">Child.</param>
        /// <param name="role">Role.</param>
        void RemoveRelationship(object parent, object child, string role);

        /// <summary>
        /// Imports the specified entities and their related.
        /// </summary>
        /// <param name="entities">Entities to import.</param>
        /// <remarks>
        /// Only the related objects which are already loaded will be imported. 
        /// Thus it alows you to import only one node of the object graph.
        /// </remarks>
        void BeginImport<T>(AsyncCallback callback, IEnumerable<T> entities) where T : IPersistable;

        /// <summary>
        /// Imports the specified entity and its related.
        /// </summary>
        /// <param name="entity">Entity to import.</param>
        void BeginImport<T>(AsyncCallback callback, T item);

        void EndImport<T>(IAsyncResult result);
    }

}
