using Evaluant.Uss;
using Evaluant.Uss.Commands;
using System.Threading;
using Evaluant.Uss.Domain;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors;

namespace Evaluant.Uss.Cache
{
    /// <summary>
    /// Visitor pattern to execute concrete commands
    /// </summary>
    class CacheCommandProcessor : BaseCommandProcessor
    {
        ReaderWriterLock _RWL;

        private IdentityMap _Entities;

        private string _NewId;
        public string NewId
        {
            get { return _NewId; }
            set { _NewId = value; }
        }

        public CacheCommandProcessor(IdentityMap entities, ReaderWriterLock rwl)
        {
            _Entities = entities;
            _RWL = rwl;
        }

        public override CreateEntityCommand Visit(CreateEntityCommand c)
        {
            Entity entity = new Entity(c.Type);
            entity.Id = c.ParentId;
            entity.State = State.UpToDate;
            entity.MetaData = Entity.LoadMetaData.AttributesLoaded | Entity.LoadMetaData.AttributesLoaded;

            _RWL.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _Entities.Add(CacheEngine.GetCacheKey(c.Type, c.ParentId), entity);
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
            _RWL.AcquireWriterLock(Timeout.Infinite);

            try
            {
                Entity processedEntity = _Entities[CacheEngine.GetCacheKey(c.Type, c.ParentId)];
                if (processedEntity == null)
                    return c;

                // Removes the entity from the cache
                _Entities.Remove(processedEntity);

                //// Removes also all the references to this entity
                //foreach (Entity e in _Entities)
                //{
                //    if (e != null)
                //        e.RemoveReference(processedEntity);
                //}
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }

            return c;
        }

        public override CreateAttributeCommand Visit(CreateAttributeCommand c)
        {
            _RWL.AcquireWriterLock(Timeout.Infinite);

            try
            {
                Entity entity = _Entities[CacheEngine.GetCacheKey(c.ParentType, c.ParentId)];
                if (entity == null)
                    return c;

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
            _RWL.AcquireWriterLock(Timeout.Infinite);

            try
            {
                Entity entity = _Entities[CacheEngine.GetCacheKey(c.ParentType, c.ParentId)];
                if (entity == null)
                    return c;

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
            _RWL.AcquireWriterLock(Timeout.Infinite);

            try
            {
                Entity entity = _Entities[CacheEngine.GetCacheKey(c.ParentType, c.ParentId)];
                if (entity == null)
                    return c;

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
            _RWL.AcquireWriterLock(Timeout.Infinite);

            try
            {
                Entity parentEntity = _Entities[CacheEngine.GetCacheKey(c.ParentType, c.ParentId)];
                if (parentEntity == null)
                    return c;

                Entity childEntity = new Entity(c.ChildType);
                childEntity.Id = c.ChildId;

                State tmpState = parentEntity.State;
                parentEntity.Add(c.Role, childEntity, typeof(Entity), State.UpToDate);
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
            _RWL.AcquireWriterLock(Timeout.Infinite);

            try
            {
                Entity entity = _Entities[CacheEngine.GetCacheKey(c.ParentType, c.ParentId)];
                if (entity == null)
                    return c;

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
    }
}
