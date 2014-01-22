using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Commands;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public interface ICommandProcessor :
        IVisitor<Transaction>,
        IVisitor<Command>,
        IVisitor<CreateEntityCommand>,
        IVisitor<DeleteEntityCommand>,
        IVisitor<CompoundCreateCommand>,
        IVisitor<CompoundUpdateCommand>,
        IVisitor<CreateAttributeCommand>,
        IVisitor<DeleteAttributeCommand>,
        IVisitor<UpdateAttributeCommand>,
        IVisitor<CreateReferenceCommand>,
        IVisitor<DeleteReferenceCommand>
    {
    }
}
