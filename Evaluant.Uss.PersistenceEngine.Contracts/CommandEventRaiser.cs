using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Commands;
using System.ComponentModel;
using Evaluant.Uss.Collections;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public class CommandEventRaiser : ICommandProcessor, IVisitor<Transaction>
    {
        private CancelEntityEventArgs cancelEventArgs;
        private HashedList<string> processedEntityIds = new HashedList<string>();

        #region Events

        public event EussCancelEventHandler Insert;
        public event EussCancelEventHandler Update;
        public event EussCancelEventHandler Delete;
        public event EussCancelReferenceEventHandler CreateRelationship;
        public event EussCancelReferenceEventHandler RemoveRelationship;

        private void RaiseInsert(string entityId)
        {
            if (Insert != null)
                Insert(entityId, cancelEventArgs);
        }
        private void RaiseUpdate(string entityId)
        {
            if (Update != null)
                Update(entityId, cancelEventArgs);
        }
        private void RaiseDelete(string entityId)
        {
            if (Delete != null)
                Delete(entityId, cancelEventArgs);
        }
        private void RaiseCreateRelationship(Model.Reference reference, string[] ReferenceId)
        {
            ReferenceCancelEventArgs rcea = new ReferenceCancelEventArgs();
            rcea.Role = reference;
            rcea.Cancel = cancelEventArgs.Cancel;
            if (CreateRelationship != null)
                CreateRelationship(ReferenceId, rcea);
            cancelEventArgs = rcea;
        }
        private void RaiseRemoveRelationship(Model.Reference reference, string[] ReferenceId)
        {
            ReferenceCancelEventArgs rcea = new ReferenceCancelEventArgs();
            rcea.Role = reference;
            rcea.Cancel = cancelEventArgs.Cancel;
            if (RemoveRelationship != null)
                RemoveRelationship(ReferenceId, rcea);
            cancelEventArgs = rcea;
        }

        #endregion

        #region ICommandProcessor Membres

        public string NewId
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Command Visit(Command command)
        {
            return command;
        }

        public CreateEntityCommand Visit(CreateEntityCommand command)
        {
            RaiseInsert(command.ParentId);
            return command;
        }

        public CompoundCreateCommand Visit(CompoundCreateCommand command)
        {
            Visit((CreateEntityCommand)command);
            foreach (Command c in command.InnerCommands)
                Visit(c);
            return command;
        }

        public CompoundUpdateCommand Visit(CompoundUpdateCommand command)
        {
            foreach (Command c in command.InnerCommands)
                Visit(c);
            return command;
        }

        public DeleteEntityCommand Visit(DeleteEntityCommand command)
        {
            RaiseDelete(command.ParentId);
            return command;
        }

        public CreateAttributeCommand Visit(CreateAttributeCommand command)
        {
            if (processedEntityIds.Contains(command.ParentId))
                return command;
            RaiseInsert(command.ParentId);
            processedEntityIds.Add(command.ParentId);
            return command;
        }

        public UpdateAttributeCommand Visit(UpdateAttributeCommand command)
        {
            if (processedEntityIds.Contains(command.ParentId))
                return command;
            RaiseUpdate(command.ParentId);
            processedEntityIds.Add(command.ParentId);
            return command;
        }

        public DeleteAttributeCommand Visit(DeleteAttributeCommand command)
        {
            return command;
        }

        public CreateReferenceCommand Visit(CreateReferenceCommand command)
        {
            RaiseCreateRelationship(command.Reference, new string[] { command.ParentType, command.ParentId, command.ChildType, command.ChildId });
            return command;
        }

        public DeleteReferenceCommand Visit(DeleteReferenceCommand command)
        {
            RaiseRemoveRelationship(command.Reference, new string[] { command.ParentType, command.ParentId, command.ChildType, command.ChildId });
            return command;
        }

        #endregion

        public Transaction Visit(Transaction transaction)
        {
            CommandCollection commandCollection = new CommandCollection();
            CommandCollection transactionCommands = transaction.PendingCommands;
            transaction.PendingCommands = commandCollection;
            foreach (Command c in transactionCommands)
            {
                cancelEventArgs = new CancelEntityEventArgs(false);
                Visit(c);
                if (!cancelEventArgs.Cancel && !cancelEventArgs.EntityChanged)
                    commandCollection.Add(c);
            }
            return transaction;
        }


        #region IVisitor<EntityCommand> Members

        public EntityCommand Visit(EntityCommand item)
        {
            if (item is CreateEntityCommand)
                return Visit((CreateEntityCommand)item);
            if (item is DeleteEntityCommand)
                return Visit((DeleteEntityCommand)item);
            throw new NotSupportedException();
        }

        #endregion

        #region IVisitor<AttributeCommand> Members

        public AttributeCommand Visit(AttributeCommand item)
        {
            if (item is CreateAttributeCommand)
                return Visit((CreateAttributeCommand)item);
            if (item is DeleteAttributeCommand)
                return Visit((DeleteAttributeCommand)item);
            if (item is UpdateAttributeCommand)
                return Visit((UpdateAttributeCommand)item);
            throw new NotSupportedException();
        }

        #endregion

        #region IVisitor<ReferenceCommand> Members

        public ReferenceCommand Visit(ReferenceCommand item)
        {
            if (item is CreateReferenceCommand)
                return Visit((CreateReferenceCommand)item);
            if (item is DeleteReferenceCommand)
                return Visit((DeleteReferenceCommand)item);
            throw new NotSupportedException();
        }

        #endregion
    }
}
