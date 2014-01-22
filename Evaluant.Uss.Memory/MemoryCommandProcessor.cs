using System.Collections;
using Evaluant.Uss.Commands;
//using Evaluant.Uss.Common;
using System.Threading;
using Evaluant.Uss.Domain;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System;
using Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors;
using System.Collections.Generic;
using Evaluant.Uss.Utility;

namespace Evaluant.Uss.Memory
{
    /// <summary>
    /// Visitor pattern to execute concrete commands
    /// </summary>
    public class MemoryCommandProcessor : BaseCommandProcessor
    {
        private IDictionary<string, Entity> _Entities;
        private ReaderWriterLock _RWL;

        private string _NewId;
        public string NewId
        {
            get { return _NewId; }
            set { _NewId = value; }
        }

        public MemoryCommandProcessor(IDictionary<string, Entity> entities, ReaderWriterLock rwl)
        {
            _Entities = entities;
            _RWL = rwl;
        }

        public override CreateEntityCommand Visit(CreateEntityCommand c)
        {
            Entity entity = new Entity(c.Type);
            entity.Id = c.ParentId;
            entity.State = State.UpToDate;
            //entity.Model = c.ParentEntity.Model;
            entity.MetaData = Entity.LoadMetaData.AttributesLoaded | Entity.LoadMetaData.AttributesLoaded;

            _RWL.AcquireWriterLock();

            try
            {
                _Entities.Add(MemoryEngine.GetCacheKey(c.Type, c.ParentId), entity);
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }
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

        public override DeleteEntityCommand Visit(DeleteEntityCommand c)
        {
            _RWL.AcquireWriterLock();

            try
            {
                Entity processedEntity = GetEntity(c.Type, c.ParentId);
                if (processedEntity == null)
                    return c;

                _Entities.Remove(MemoryEngine.GetCacheKey(c.Type, c.ParentId));

                foreach (Entity entity in _Entities.Values)
                    entity.Remove(processedEntity);
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }

            return c;
        }

        public override CreateAttributeCommand Visit(CreateAttributeCommand c)
        {
            _RWL.AcquireWriterLock();

            try
            {
                Entity entity = GetEntity(c.ParentType, c.ParentId);
                State tmpState = entity.State;
                entity.Add(c.Name, c.Value, c.Type, State.UpToDate);
                entity.State = tmpState;
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }
            return c;

        }

        public override UpdateAttributeCommand Visit(UpdateAttributeCommand c)
        {
            _RWL.AcquireWriterLock();

            try
            {
                Entity entity = GetEntity(c.ParentType, c.ParentId);
                State tmpState = entity.State;
                entity[c.Name].Value = c.Value;
                entity.State = tmpState;
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }
            return c;
        }

        public override DeleteAttributeCommand Visit(DeleteAttributeCommand c)
        {
            _RWL.AcquireWriterLock();

            try
            {
                Entity entity = GetEntity(c.ParentType, c.ParentId);
                State tmpState = entity.State;
                entity.Remove(c.Name);
                entity.State = tmpState;
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }
            return c;
        }

        public override CreateReferenceCommand Visit(CreateReferenceCommand c)
        {
            _RWL.AcquireWriterLock();

            try
            {
                Entity childEntity = GetEntity(c.ChildType, c.ChildId);
                Entity parentEntity = GetEntity(c.ParentType, c.ParentId);

                State tmpState = parentEntity.State;
                if (c.Reference.Cardinality.IsToMany)
                {
                    MultipleEntry me = (MultipleEntry)parentEntity[c.Role ?? c.Reference.Name];
                    if (me == null)
                        parentEntity.Add(me = new MultipleEntry(c.Role ?? c.Reference.Name));
                    me.Add(Entry.Create(c.Role ?? c.Reference.Name, tmpState, childEntity));
                }
                else
                    parentEntity.Add(c.Role ?? c.Reference.Name, childEntity);
                parentEntity.State = tmpState;
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }
            return c;
        }

        public override DeleteReferenceCommand Visit(DeleteReferenceCommand c)
        {
            _RWL.AcquireWriterLock();

            try
            {
                Entity entity = GetEntity(c.ParentType, c.ParentId);

                for (int i = entity.EntityEntries.Count - 1; i >= 0; i--)
                {
                    Entry ee = (Entry)entity.EntityEntries[i];
                    if (ee.IsEntity && ((Entity)ee.Value).Id == c.ChildId)
                        entity.EntityEntries.Remove(ee);
                }
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }
            return c;
        }

        protected Entity GetEntity(string type, string id)
        {
            return (Entity)_Entities[MemoryEngine.GetCacheKey(type, id)];
        }
    }
}
