using System;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Collections.Generic;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public interface IPersistenceEngineObjectContext : IPersistenceEngineObjectContextAsync, IObjectContext
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

        IPersistenceEngine PersistenceEngine { get; set; }

        /// <summary>
        /// Loads all the referenced objects for a given entity and a role name. This is the Lazy Loading logic.
        /// </summary>
        /// <param name="entity">The Entity object from which the referenced items have to be loaded</param>
        /// <param name="name">The name of the role to load the linked entities from</param>
        /// <returns>The collection of the loaded objects</returns>
        IList<T> LoadReference<T>(Entity entity, string name);

        /// <summary>
        /// Loads all the referenced objects for a given entity and a role name. This is the Lazy Loading logic.
        /// </summary>
        /// <param name="entity">The Entity object from which the referenced items have to be loaded</param>
        /// <param name="name">The name of the role to load the linked entities from</param>
        /// <returns>The collection of the loaded objects</returns>
        void Infer<T>(IEnumerable<T> entities, string name);

        /// <summary>
        /// Imports the specified entity and its related.
        /// </summary>
        /// <param name="entity">Entity to import.</param>
        void Import(IPersistable item);
    }


}
