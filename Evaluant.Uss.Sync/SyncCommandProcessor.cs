using System;
using System.Diagnostics;

using Evaluant.Uss;
//using Evaluant.Uss.Common;
using Evaluant.Uss.Commands;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors;

namespace Evaluant.Uss.Sync
{
    /// <summary>
    /// Visitor pattern to execute concrete commands
    /// </summary>
    class SyncCommandProcessor : BaseCommandProcessor
    {
        private int order = 0;
        private Transaction transaction;
        private int transactionId = 0;
        private SyncEngine engine;

        public SyncCommandProcessor(SyncEngine engine, Transaction transaction, int transactionId)
        {
            this.transaction = transaction;
            this.transactionId = transactionId;
            this.engine = engine;
        }

        DateTime previous = DateTime.MinValue;

        private void PopulateDefaults(Entity e, Command c)
        {
            e.SetValue(SyncUtils.CLIENTID, c.ClientId);
            e.SetValue(SyncUtils.PROCESSED, DateTime.Now);
            e.SetValue(SyncUtils.NUMBER, order++);
            e.SetValue(SyncUtils.TRANSACTION, transactionId);
        }

        public override CreateEntityCommand Visit(CreateEntityCommand c)
        {
            Entity e = new Entity(SyncUtils.CREATE_ENTITY);

            PopulateDefaults(e, c);

            e.SetValue(SyncUtils.PARENTID, c.ParentId);
            e.SetValue(SyncUtils.TYPE, c.Type);

            transaction.Serialize(e);

            return c;
        }

        public override DeleteEntityCommand Visit(DeleteEntityCommand c)
        {
            Entity e = new Entity(SyncUtils.DELETE_ENTITY);
            PopulateDefaults(e, c);

            e.SetValue(SyncUtils.PARENTID, c.ParentId);
            e.SetValue(SyncUtils.TYPE, c.Type);

            transaction.Serialize(e);

            return c;
        }

        public override CreateAttributeCommand Visit(CreateAttributeCommand c)
        {
            Entity e = new Entity(SyncUtils.CREATE_ATTRIBUTE);
            PopulateDefaults(e, c);

            e.SetValue(SyncUtils.PARENTID, c.ParentId);
            e.SetValue(SyncUtils.PARENTTYPE, c.ParentType);
            e.SetValue(SyncUtils.TYPE, c.Type.Name);
            e.SetValue(SyncUtils.NAME, c.Name);
            e.SetValue(SyncUtils.VALUE, engine.Factory.Serializer.SerializeToString(c.Value));

            transaction.Serialize(e);

            return c;
        }

        public override CompoundCreateCommand Visit(CompoundCreateCommand c)
        {
            Visit((CreateEntityCommand)c);
            foreach (CreateAttributeCommand cc in c.InnerCommands)
                Visit(cc);

            return c;
        }

        public override CompoundUpdateCommand Visit(CompoundUpdateCommand c)
        {
            foreach (Command cc in c.InnerCommands)
                Visit(cc);

            return c;
        }

        public override UpdateAttributeCommand Visit(UpdateAttributeCommand c)
        {
            Entity e = new Entity(SyncUtils.UPDATE_ATTRIBUTE);
            PopulateDefaults(e, c);

            e.SetValue(SyncUtils.PARENTID, c.ParentId);
            e.SetValue(SyncUtils.PARENTTYPE, c.ParentType);
            e.SetValue(SyncUtils.TYPE, c.Type.Name);
            e.SetValue(SyncUtils.NAME, c.Name);
            e.SetValue(SyncUtils.VALUE, engine.Factory.Serializer.SerializeToString(c.Value));

            transaction.Serialize(e);

            return c;
        }

        public override DeleteAttributeCommand Visit(DeleteAttributeCommand c)
        {
            Entity e = new Entity(SyncUtils.DELETE_ATTRIBUTE);
            PopulateDefaults(e, c);

            e.SetValue(SyncUtils.PARENTID, c.ParentId);
            e.SetValue(SyncUtils.PARENTTYPE, c.ParentType);
            e.SetValue(SyncUtils.NAME, c.Name);
            e.SetValue(SyncUtils.TYPE, c.Type.Name);

            transaction.Serialize(e);

            return c;
        }

        public override CreateReferenceCommand Visit(CreateReferenceCommand c)
        {
            Entity e = new Entity(SyncUtils.CREATE_REFERENCE);
            PopulateDefaults(e, c);

            e.SetValue(SyncUtils.PARENTID, c.ParentId);
            e.SetValue(SyncUtils.PARENTTYPE, c.ParentType);
            e.SetValue(SyncUtils.ROLE, c.Role);
            e.SetValue(SyncUtils.CHILDID, c.ChildId);
            e.SetValue(SyncUtils.CHILDTYPE, c.ChildType);

            transaction.Serialize(e);

            return c;
        }

        public override DeleteReferenceCommand Visit(DeleteReferenceCommand c)
        {
            Entity e = new Entity(SyncUtils.DELETE_REFERENCE);
            PopulateDefaults(e, c);

            e.SetValue(SyncUtils.PARENTID, c.ParentId);
            e.SetValue(SyncUtils.PARENTTYPE, c.ParentType);
            e.SetValue(SyncUtils.ROLE, c.Role);
            e.SetValue(SyncUtils.CHILDID, c.ChildId);
            e.SetValue(SyncUtils.CHILDTYPE, c.ChildType);

            transaction.Serialize(e);

            return c;
        }

        private string _NewId;
        public string NewId
        {
            get { return _NewId; }
            set { _NewId = value; }
        }
    }
}
