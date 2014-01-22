using System;
using System.Collections;

using Evaluant.Uss.Commands;
using System.Collections.Generic;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Collections;
#if !SILVERLIGHT
using System.Collections.Specialized;
#else
using Evaluant.Uss.Serializer;
#endif//using Evaluant.Uss.Collections;
//using Evaluant.Uss.Common;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    /// <summary>
    /// Represents a local transaction with a <c>IPersistenceEngine</c>.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Transaction : IVisitable<Transaction>
    {
        [field: NonSerialized]
        public event EussEventHandler EntityCreated;
        [field: NonSerialized]
        public event EussCancelEventHandler EntityCreating;
        [field: NonSerialized]
        public event EussEventHandler EntityUpdated;
        [field: NonSerialized]
        public event EussCancelEventHandler EntityUpdating;
        [field: NonSerialized]
        public event EussEventHandler EntityDeleted;
        [field: NonSerialized]
        public event EussCancelEventHandler EntityDeleting;
        [field: NonSerialized]
        public event EussReferenceEventHandler EntityRelationshipCreated;
        [field: NonSerialized]
        public event EussCancelReferenceEventHandler EntityRelationshipCreating;
        [field: NonSerialized]
        public event EussReferenceEventHandler EntityRelationshipRemoved;
        [field: NonSerialized]
        public event EussCancelReferenceEventHandler EntityRelationshipRemoving;

        private IChangesAnalyzer changesAnalyzer;

        private bool _CanAcceptCommand = true;

        [NonSerialized]
        protected HashedList<Entity> _PendingEntities; // All entities on which Serialize() was called

        public HashedList<Entity> ComputedEntities
        {
            get { return _ComputedEntities; }
        }

        [NonSerialized]
        protected HashedList<Entity> _ComputedEntities; // All entities and its dependents
        protected HashedList<Entity> _ConcernedEntities; // All entities not in ComputedEntities but still dealed in the transaction (for relationshipcommands for example)
        protected CommandCollection _PendingCommands; // All commands to be processed
        private IDictionary<string, string> _NewIds;
        public IDictionary<string, string> NewIds
        {
            get { return _NewIds; }
            set { _NewIds = value; }
        }
        private bool _Prepared = false;
        protected const int _InitialCommandStackSize = 20;

        public Transaction(Model.Model model)
        {
            _PendingEntities = new HashedList<Entity>();
            _ComputedEntities = new HashedList<Entity>();
            _ConcernedEntities = new HashedList<Entity>();
            changesAnalyzer = new ChangesAnalyzer(model);
            _PendingCommands = changesAnalyzer.Commands;
        }

        public CommandCollection PendingCommands
        {
            get { return _PendingCommands; }
            set { _PendingCommands = value; }		// used in RemotingPersistenceEngine
        }

        /// <summary>
        /// Generates all the commands for the serialization of an entity
        /// </summary>
        /// <param name="entity">The entity to serialize</param>
        /// <remarks>
        /// This method should not be called inside an IPersistenceEngine. It should only be called 
        /// by the main application.
        /// </remarks>
        public virtual void Serialize(Entity entity)
        {
            if (!_CanAcceptCommand)
                throw new UniversalStorageException("You may not serialize an object with this transaction anymore.");

            if (!_PendingEntities.Contains(entity))
                _PendingEntities.Add(entity);
        }

        /// <summary>
        /// Generates all the commands for the serialization of an EntitySet
        /// </summary>
        /// <param name="entityset">The entities to serialize</param>
        /// <remarks>
        /// This method should not be called inside an IPersistenceEngine. It should only be called 
        /// by the main application.
        /// </remarks>
        public void Serialize(EntitySet entityset)
        {
            foreach (Entity e in entityset)
                Serialize(e);
        }

        /// <summary>
        /// Marks an entity as Deleted. Must be called after BeginEdit
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <remarks>
        /// This method should not be called inside an IPersistenceEngine. It should only be called 
        /// by the main application.
        /// </remarks>
        public void Delete(Entity entity)
        {
            entity.State = State.Deleted;
            Serialize(entity);
        }

        /// <summary>
        /// Marks all entities as Deleted. Must be called after BeginEdit
        /// </summary>
        /// <param name="entityset">Entities.</param>
        /// <remarks>
        /// <remarks>
        /// This method should not be called inside an IPersistenceEngine. It should only be called 
        /// by the main application.
        /// </remarks>
        public void Delete(EntitySet entityset)
        {
            foreach (Entity e in entityset)
                Delete(e);
        }

        /// <summary>
        /// Prepares this transaction by generating all commands from Entities to serialize.
        /// </summary>
        private void Prepare()
        {
            if (_Prepared)
                return;

            // Computes all commands to Process at this moment so that we don't process the same Entity
            // two times in a transaction
            foreach (Entity entity in _PendingEntities)
            {
                changesAnalyzer.ComputeChanges(entity, _ComputedEntities);
            }

            // The CommandCollection is automatically sorted
            _NewIds = new Dictionary<string, string>();

            _Prepared = true;
        }

        /// <summary>
        /// Gets back all new generated Ids during CreateEntityCommand process
        /// </summary>
        private void UpdateIds()
        {
            if (_NewIds.Count == 0)
                return;

            // Changes old Ids with new Ids
            foreach (Entity e in _ComputedEntities)
            {
                if (_NewIds.ContainsKey(e.Id))
                {
                    string oldId = e.Id;
                    e.Id = (string)_NewIds[e.Id];
                }
            }
        }

        /// <summary>
        /// Terminates a transaction by updating Entities
        /// </summary>
        private void Terminate()
        {
            foreach (Entity entity in _PendingEntities)
                entity.AcceptChanges();

            // Frees the references to Entities and Commands so that they can be disposed
            _PendingEntities = null;
            _PendingCommands = null;
        }

        public void Commit(IPersistenceEngine engine)
        {
            Commit(engine, false);
        }

        public void Commit(IPersistenceEngine engine, bool raiseEvents)
        {
            Prepare();

            CommandEventRaiser eventRaiser;
            if (raiseEvents)
            {
                eventRaiser = new CommandEventRaiser();
                RegisterEventsBeforeProcessing(eventRaiser);
                Accept(eventRaiser);
            }

            engine.BeforeProcessCommands(this);
            engine.ProcessCommands(this);
            engine.AfterProcessCommands(this);

            if (raiseEvents)
            {
                eventRaiser = new CommandEventRaiser();
                RegisterEventsAfterProcessing(eventRaiser);
                Accept(eventRaiser);
            }

            UpdateIds();
            Terminate();
        }

        public void PushCommand(Command command)
        {
            if (!_CanAcceptCommand)
                throw new UniversalStorageException("You may not serialize an object with this transaction anymore.");

            _PendingCommands.Add(command);
        }

        public void PushCommand(CommandCollection commands)
        {
            if (!_CanAcceptCommand)
                throw new UniversalStorageException("You may not serialize an object with this transaction anymore.");

            _PendingCommands.AddRange(commands);
        }

        public void PushCommand(Command[] commands)
        {
            if (!_CanAcceptCommand)
                throw new UniversalStorageException("You may not serialize an object with this transaction anymore.");

            _PendingCommands.AddRange(new CommandCollection(commands));
        }

        #region Events

        private void RegisterEventsAfterProcessing(CommandEventRaiser eventRaiser)
        {
            if (EntityCreated != null)
                eventRaiser.Insert += new EussCancelEventHandler(eventRaiser_Inserted);
            if (EntityDeleted != null)
                eventRaiser.Delete += new EussCancelEventHandler(eventRaiser_Deleted);
            if (EntityRelationshipCreated != null)
                eventRaiser.CreateRelationship += new EussCancelReferenceEventHandler(eventRaiser_CreatedRelationship);
            if (EntityRelationshipRemoved != null)
                eventRaiser.RemoveRelationship += new EussCancelReferenceEventHandler(eventRaiser_RemovedRelationship);
            if (EntityUpdated != null)
                eventRaiser.Update += new EussCancelEventHandler(eventRaiser_Updated);
        }

        private void RegisterEventsBeforeProcessing(CommandEventRaiser eventRaiser)
        {
            if (EntityCreating != null)
                eventRaiser.Insert += new EussCancelEventHandler(eventRaiser_Inserting);
            if (EntityDeleting != null)
                eventRaiser.Delete += new EussCancelEventHandler(eventRaiser_Deleting);
            if (EntityRelationshipCreating != null)
                eventRaiser.CreateRelationship += new EussCancelReferenceEventHandler(eventRaiser_CreatingRelationship);
            if (EntityRelationshipRemoving != null)
                eventRaiser.RemoveRelationship += new EussCancelReferenceEventHandler(eventRaiser_RemovingRelationship);
            if (EntityUpdating != null)
                eventRaiser.Update += new EussCancelEventHandler(eventRaiser_Updating);
        }

        #region Before Processing Events

        void eventRaiser_Updating(object sender, CancelEntityEventArgs e)
        {
            if (EntityUpdating != null)
            {
                Entity entity = FindComputedEntityWithId((string)sender);
                EntityUpdating(entity, e);
                if (!e.Cancel && e.EntityChanged)
                {
                    changesAnalyzer.ComputeChanges(entity, true);
                    //PendingCommands.AddRange(Entity.ComputeChanges(entity, true));
                }
            }
        }

        void eventRaiser_RemovingRelationship(object sender, ReferenceCancelEventArgs e)
        {
            if (EntityRelationshipRemoving != null)
            {
                string[] referenceId = (string[])sender;
                string parentType = referenceId[0];
                string parentId = referenceId[1];
                string childType = referenceId[2];
                string childId = referenceId[3];
                //string role = referenceId[4];
                Entity parent = FindComputedEntityWithIdAndType(parentType, parentId);
                Entity child = FindComputedEntityWithIdAndType(childType, childId);
                e.Child = child;
                //e.Role = role;
                EntityRelationshipRemoving(parent, e);
                if (!e.Cancel && (e.ParentEntityChanged || e.ChildEntityChanged))
                {
                    if (e.ParentEntityChanged)
                        changesAnalyzer.ComputeChanges(parent, true);
                    //PendingCommands.AddRange(Entity.ComputeChanges(parent, true));
                    if (e.ChildEntityChanged)
                        changesAnalyzer.ComputeChanges(child, true);
                    //PendingCommands.AddRange(Entity.ComputeChanges(child, true));
                }
            }
        }

        void eventRaiser_CreatingRelationship(object sender, ReferenceCancelEventArgs e)
        {
            if (EntityRelationshipCreating != null)
            {
                string[] referenceId = (string[])sender;
                string parentType = referenceId[0];
                string parentId = referenceId[1];
                string childType = referenceId[2];
                string childId = referenceId[3];
                string role = referenceId[4];
                Entity parent = FindComputedEntityWithIdAndType(parentType, parentId);
                Entity child = FindComputedEntityWithIdAndType(childType, childId);
                e.Child = child;
                //e.Role = role;
                EntityRelationshipCreating(parent, e);
                if (!e.Cancel && (e.ParentEntityChanged || e.ChildEntityChanged))
                {
                    if (e.ParentEntityChanged)
                        changesAnalyzer.ComputeChanges(parent, true);
                    //PendingCommands.AddRange(Entity.ComputeChanges(parent, true));
                    if (e.ChildEntityChanged)
                        changesAnalyzer.ComputeChanges(child, true);
                    //PendingCommands.AddRange(Entity.ComputeChanges(child, true));
                }
            }
        }

        void eventRaiser_Deleting(object sender, CancelEntityEventArgs e)
        {
            if (EntityDeleting != null)
            {
                Entity entity = FindComputedEntityWithId((string)sender);
                EntityDeleting(entity, e);
                if (!e.Cancel && e.EntityChanged)
                    changesAnalyzer.ComputeChanges(entity, true);
                //PendingCommands.AddRange(Entity.ComputeChanges(entity, true));
            }
        }

        void eventRaiser_Inserting(object sender, CancelEntityEventArgs e)
        {
            Entity entity = FindComputedEntityWithId((string)sender);
            if (EntityCreating != null)
                EntityCreating(entity, e);
            if (!e.Cancel && e.EntityChanged)
                changesAnalyzer.ComputeChanges(entity, true);
            //PendingCommands.AddRange(Entity.ComputeChanges(entity, true));
        }

        #endregion

        #region After Processing Events

        void eventRaiser_Updated(object sender, CancelEntityEventArgs e)
        {
            if (EntityUpdated != null)
                EntityUpdated(FindComputedEntityWithId((string)sender), new EntityEventArgs());
        }

        void eventRaiser_RemovedRelationship(object sender, ReferenceCancelEventArgs e)
        {
            if (EntityRelationshipRemoved != null)
            {
                string[] referenceId = (string[])sender;
                string parentType = referenceId[0];
                string parentId = referenceId[1];
                string childType = referenceId[2];
                string childId = referenceId[3];
                Entity parent = FindComputedEntityWithIdAndType(parentType, parentId);
                Entity child = FindComputedEntityWithIdAndType(childType, childId);
                EntityRelationshipRemoved(parent, new ReferenceEventArgs(e.Role, child));
            }
        }

        void eventRaiser_CreatedRelationship(object sender, ReferenceCancelEventArgs e)
        {
            if (EntityRelationshipCreated != null)
            {
                string[] referenceId = (string[])sender;
                string parentType = referenceId[0];
                string parentId = referenceId[1];
                string childType = referenceId[2];
                string childId = referenceId[3];
                Entity parent = FindComputedEntityWithIdAndType(parentType, parentId);
                Entity child = FindComputedEntityWithIdAndType(childType, childId);
                EntityRelationshipCreated(parent, new ReferenceEventArgs(e.Role, child));
            }
        }

        void eventRaiser_Deleted(object sender, CancelEntityEventArgs e)
        {
            if (EntityDeleted != null)
                EntityDeleted(FindComputedEntityWithId((string)sender), EntityEventArgs.Empty);
        }

        void eventRaiser_Inserted(object sender, CancelEntityEventArgs e)
        {
            if (EntityCreated != null)
                EntityCreated(FindComputedEntityWithId((string)sender), EntityEventArgs.Empty);
        }

        #endregion

        #endregion

        public Entity FindComputedEntityWithId(string entityId)
        {
            foreach (Entity e in _ComputedEntities)
            {
                if (e.Id == entityId)
                    return e;
            }
            foreach (Entity e in _ConcernedEntities)
            {
                if (e.Id == entityId)
                    return e;
            }
            return null;
        }


        private Entity FindComputedEntityWithIdAndType(string entityType, string entityId)
        {
            foreach (Entity e in _ComputedEntities)
            {
                if (e.Id == entityId && e.Type == entityType)
                    return e;
            }
            foreach (Entity e in _ConcernedEntities)
            {
                if (e.Id == entityId && e.Type == entityType)
                    return e;
            }
            return null;
        }

        public void AddConcernedEntity(Entity entity)
        {
            if (!_ConcernedEntities.Contains(entity))
                _ConcernedEntities.Add(entity);
        }

        #region IVisitable<Transaction> Members

        public void Accept(IVisitor<Transaction> visitor)
        {
            visitor.Visit(this);
        }

        #endregion

        public void BeginCommit(AsyncCallback callback, IPersistenceEngineAsync engine, bool raiseEvents, object asyncState)
        {
            Prepare();

            if (raiseEvents)
            {
                CommandEventRaiser eventRaiser = new CommandEventRaiser();
                RegisterEventsBeforeProcessing(eventRaiser);
                Accept(eventRaiser);
            }

            engine.BeforeProcessCommands(this);
            engine.BeginProcessCommands(this, callback, asyncState);
        }

        public void EndCommit(IAsyncResult result, IPersistenceEngineAsync engine, bool raiseEvents)
        {
            engine.EndProcessCommands(result);
            engine.AfterProcessCommands(this);

            if (raiseEvents)
            {
                CommandEventRaiser eventRaiser = new CommandEventRaiser();
                RegisterEventsAfterProcessing(eventRaiser);
                Accept(eventRaiser);
            }

            UpdateIds();
            Terminate();
        }
    }
}
