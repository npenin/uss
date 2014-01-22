using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors
{
    public abstract class BaseCommandProcessor : ICommandProcessor
    {
        public BaseCommandProcessor() { }

        #region IVisitor<Transaction> Members

        public virtual Transaction Visit(Transaction item)
        {
            foreach (var command in item.PendingCommands)
                Visit(command);
            return item;
        }

        #endregion

        #region IVisitor<Command> Members

        public Evaluant.Uss.Commands.Command Visit(Evaluant.Uss.Commands.Command item)
        {
            switch (item.CommandType)
            {
                case Evaluant.Uss.Commands.CommandTypes.CompoundCreate:
                    return Visit((CompoundCreateCommand)item);
                case Evaluant.Uss.Commands.CommandTypes.CompoundUpdate:
                    return Visit((CompoundUpdateCommand)item);
                case Evaluant.Uss.Commands.CommandTypes.CreateAttribute:
                    return Visit((CreateAttributeCommand)item);
                case Evaluant.Uss.Commands.CommandTypes.CreateEntity:
                    return Visit((CreateEntityCommand)item);
                case Evaluant.Uss.Commands.CommandTypes.CreateReference:
                    return Visit((CreateReferenceCommand)item);
                case Evaluant.Uss.Commands.CommandTypes.DeleteAttribute:
                    return Visit((DeleteAttributeCommand)item);
                case Evaluant.Uss.Commands.CommandTypes.DeleteEntity:
                    return Visit((DeleteEntityCommand)item);
                case Evaluant.Uss.Commands.CommandTypes.DeleteReference:
                    return Visit((DeleteReferenceCommand)item);
                case Evaluant.Uss.Commands.CommandTypes.UpdateAttribute:
                    return Visit((UpdateAttributeCommand)item);
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion


        #region IVisitor<CreateEntityCommand> Members

        public abstract CreateEntityCommand Visit(CreateEntityCommand item);

        #endregion

        #region IVisitor<DeleteEntityCommand> Members

        public abstract DeleteEntityCommand Visit(DeleteEntityCommand item);

        #endregion

        #region IVisitor<CompoundCreateCommand> Members

        public abstract CompoundCreateCommand Visit(CompoundCreateCommand item);

        #endregion

        #region IVisitor<CompoundUpdateCommand> Members

        public abstract CompoundUpdateCommand Visit(CompoundUpdateCommand item);

        #endregion

        #region IVisitor<CreateAttributeCommand> Members

        public abstract CreateAttributeCommand Visit(CreateAttributeCommand item);

        #endregion

        #region IVisitor<DeleteAttributeCommand> Members

        public abstract DeleteAttributeCommand Visit(DeleteAttributeCommand item);

        #endregion

        #region IVisitor<UpdateAttributeCommand> Members

        public abstract UpdateAttributeCommand Visit(UpdateAttributeCommand item);

        #endregion

        #region IVisitor<CreateReferenceCommand> Members

        public abstract CreateReferenceCommand Visit(CreateReferenceCommand item);

        #endregion

        #region IVisitor<DeleteReferenceCommand> Members

        public abstract DeleteReferenceCommand Visit(DeleteReferenceCommand item);

        #endregion
    }
}
