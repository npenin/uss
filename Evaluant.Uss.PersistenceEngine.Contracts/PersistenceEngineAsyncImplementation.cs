using System;
using System.Collections.Generic;
using Evaluant.Uss.Domain;
using Evaluant.NLinq;
using System.Globalization;
using Evaluant.Uss.Commands;
using Evaluant.NLinq.Expressions;


namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public abstract class PersistenceEngineAsyncImplementation : IPersistenceEngineAsync
    {
        public PersistenceEngineAsyncImplementation(IPersistenceProviderAsync asyncFactory)
        {
            FactoryAsync = asyncFactory;
        }

        public virtual IPersistenceProviderAsync FactoryAsync { get; protected set; }

        private CultureInfo _Culture;
        public CultureInfo Culture
        {
            get { return _Culture; }
            set { _Culture = value; }
        }


        #region IPersistenceEngineAsync Members

        public void BeginLoad(AsyncCallback callback, string query, object asyncState)
        {
            BeginLoad(callback, new NLinqQuery(query), asyncState);
        }

        public void BeginLoad(AsyncCallback callback, string query, int first, int max, object asyncState)
        {
            BeginLoad(callback, new NLinqQuery(query), first, max, asyncState);
        }

        public void BeginLoad(AsyncCallback callback, NLinqQuery query, int first, int max, object asyncState)
        {
            BeginLoad(callback, query.Page(first, max), asyncState);
        }

        public IList<Entity> EndLoad(IAsyncResult result)
        {
            return EndLoadMany<IList<Entity>, Entity>(result);
        }

        public void BeginLoadWithId(AsyncCallback callback, string type, string id, object asyncState)
        {
            BeginLoadWithIds(callback, type, new string[] { id }, asyncState);
        }

        public abstract void BeginLoadWithIds(AsyncCallback callback, string type, string[] id, object asyncState);

        public Entity EndLoadWithId(IAsyncResult result)
        {
            IList<Entity> entities = EndLoadWithIds(result);
            if (entities != null && entities.Count > 0)
                return entities[0];
            return null;
        }

        public abstract IList<Entity> EndLoadWithIds(IAsyncResult result);

        public void BeginLoadReference(AsyncCallback callback, string reference, params Entity[] entities)
        {
            BeginLoadReference(callback, reference, entities, null);
        }

        public void BeginLoadScalar<T>(AsyncCallback callback, NLinqQuery query)
        {
            BeginLoadScalar<T>(callback, query, null);
        }

        public void BeginLoadScalar<T, U>(AsyncCallback callback, NLinqQuery query) where T : class, IEnumerable<U>
        {
            BeginLoadScalar<T, U>(callback, query, null);
        }

        public void BeginLoadScalar<T, U>(AsyncCallback callback, NLinqQuery query, int first, int max) where T : class, IEnumerable<U>
        {
            BeginLoadScalar<T, U>(callback, query.Page(first, max));
        }


        public void BeginLoadScalar<T>(AsyncCallback callback, string query, object asyncState)
        {
            BeginLoadScalar<T>(callback, new NLinqQuery(query), asyncState);
        }

        public void BeginLoadScalar<T>(AsyncCallback callback, string query)
        {
            BeginLoadScalar<T>(callback, query, null);
        }

        public void BeginLoadScalar<T, U>(AsyncCallback callback, string query, int first, int max) where T : class, IEnumerable<U>
        {
            BeginLoadScalar<T>(callback, new NLinqQuery(query).Page(first, max));
        }

        public void BeginLoadScalar<T>(AsyncCallback callback, NLinqQuery query, object asyncState)
        {
            BeginLoadSingle<T>(callback, query, asyncState);
        }

        public void BeginLoadScalar<T, U>(AsyncCallback callback, NLinqQuery query, object asyncState)
            where T : class,IEnumerable<U>
        {
            BeginLoadMany<T, U>(callback, query, asyncState);
        }

        public T EndLoadScalar<T>(IAsyncResult result)
        {
            return EndLoadSingle<T>(result);
        }

        public T EndLoadScalar<T, U>(IAsyncResult result) where T : class,IEnumerable<U>
        {
            return EndLoadMany<T, U>(result);
        }

        #endregion

        #region IPersistenceEngineAsync Members
        public virtual void Initialize()
        {
        }

        public virtual void BeforeProcessCommands(Transaction tx)
        {
            IDictionary<string, Command> processed = new Dictionary<string, Command>();

            foreach (Command c in tx.PendingCommands)
            {
                if (c.ProcessOrder == 2)
                {
                    DeleteEntityCommand command = c as DeleteEntityCommand;

                    // Checks whether it's really a DeleteEntityCommand
                    if (command == null)
                        continue;

                    processed.Add(GetCommandKey(command.ParentId, command.Type), command);
                }
            }

            CommandCollection newCommands = new CommandCollection();
            foreach (Command c in tx.PendingCommands)
            {
                if (c.ProcessOrder == 2)
                {
                    DeleteEntityCommand command = c as DeleteEntityCommand;

                    // Checks whether it's really a DeleteEntityCommand
                    if (command == null)
                        continue;

                    newCommands.AddRange(ComputeCascadeDeleteCommands(command.ParentId, command.Type, processed));
                }
            }

            tx.PendingCommands.AddRange(newCommands);
        }

        protected string GetCommandKey(string id, string type)
        {
            return String.Concat(type, ".", id);
        }

        protected abstract CommandCollection ComputeCascadeDeleteCommands(string id, string type, IDictionary<string, Command> processed);
        //{
        //    CommandCollection result = new CommandCollection();

        //    foreach (Model.Reference reference in FactoryAsync.Model.GetInheritedReferences(type))
        //    {
        //        // Search for composition relationships
        //        if (!reference.IsComposition)
        //            continue;

        //        Entity e = new Entity(type);
        //        e.Id = id;

        //        this.LoadReference(e, new string[] { reference.Name });

        //        foreach (Entry<Entity> subEntity in ((MultipleEntry)e[reference.Name]))
        //        {
        //            string subKey = GetCommandKey(subEntity.TypedValue.Id, subEntity.TypedValue.Type);

        //            if (!processed.ContainsKey(subKey))
        //            {
        //                DeleteEntityCommand nec = new DeleteEntityCommand(subEntity.TypedValue.Id, subEntity.TypedValue.Type);
        //                result.Add(nec);
        //                processed.Add(subKey, nec);

        //                // Recursive call
        //                result.AddRange(ComputeCascadeDeleteCommands(subEntity.TypedValue.Id, subEntity.TypedValue.Type, processed));
        //            }
        //        }
        //    }

        //return result;
        //}


        /// <summary>
        /// Execute the commands here
        /// </summary>
        /// <param name="commands"></param>
        public abstract void BeginProcessCommands(Transaction tx, AsyncCallback callback, object asyncState);

        /// <summary>
        /// Execute the commands here
        /// </summary>
        /// <param name="commands"></param>
        public abstract void EndProcessCommands(IAsyncResult result);

        public virtual void AfterProcessCommands(Transaction tx)
        {
            foreach (Entity entity in tx.ComputedEntities)
            {
                entity.AcceptChanges();
            }
        }

        #endregion

        #region IPersistenceEngineAsync Members


        public abstract void CreateId(Entity e);

        #endregion

        #region IPersistenceEngineAsync Members


        public void BeginLoadScalar<T, U>(AsyncCallback callback, string query, int first, int max, object asyncState) where T : class, IEnumerable<U>
        {
            BeginLoadScalar<T, U>(callback, new NLinqQuery(query), first, max, asyncState);
        }

        public void BeginLoadScalar<T, U>(AsyncCallback callback, NLinqQuery query, int first, int max, object asyncState) where T : class, IEnumerable<U>
        {
            BeginLoadMany<T, U>(callback, query.Page(first, max), asyncState);
        }

        protected abstract void BeginLoadSingle<T>(AsyncCallback callback, NLinqQuery query, object asyncState);
        protected abstract void BeginLoadMany<T, U>(AsyncCallback callback, NLinqQuery query, object asyncState) where T : class, IEnumerable<U>;

        protected abstract T EndLoadSingle<T>(IAsyncResult result);
        protected abstract T EndLoadMany<T, U>(IAsyncResult result) where T : class, IEnumerable<U>;

        public void BeginLoad(AsyncCallback callback, NLinqQuery query, object asyncState)
        {
            BeginLoadMany<IList<Entity>, Entity>(callback, query, asyncState);
        }

        public void BeginLoadReference(AsyncCallback callback, string reference, object asyncState, params Entity[] entities)
        {
            BeginLoadReference(callback, reference, entities, GetQuery(reference, GetQuery(entities)), asyncState);
        }

        public static NLinqQuery GetQuery(IPersistenceProviderAsync engineFactory, params Entity[] entities)
        {
            BinaryExpression constraint = null;
            var query = new NLinqQuery("from e0 in context select e0");
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                ((QueryExpression)query.Expression).From.Type = entity.Type;

                var entityModel = engineFactory.Model.Entities[entity.Type];
                BinaryExpression entityConstraint = null;
                foreach (Model.Attribute attribute in entityModel.Attributes.Values)
                {
                    if (!attribute.IsId)
                        continue;
                    entityConstraint = new BinaryExpression(BinaryExpressionType.And, entityConstraint,
                        new BinaryExpression(BinaryExpressionType.Equal, new MemberExpression(new Identifier(attribute.Name), new Identifier("e0")), new Parameter(attribute.Name + i)));
                    query.Parameters.Add(attribute.Name + i, entity[attribute.Name].Value);
                }
                if (constraint == null)
                    constraint = entityConstraint;
                else
                    constraint = new BinaryExpression(BinaryExpressionType.Or, constraint, entityConstraint);
            }
            ((QueryExpression)query.Expression).QueryBody.Clauses.Add(new WhereClause(constraint));
            return query;
        }

        protected virtual NLinqQuery GetQuery(string reference, NLinqQuery entityQuery)
        {
            return new NLinqQuery(
                new QueryExpression(
                    new FromClause(null, new Identifier("#e0"), entityQuery.Expression),
                    new QueryBody(
                        new ClauseList(),
                        new SelectClause(
                            new MemberExpression(
                                new NLinqQuery(reference).Expression,
                                new Identifier("#e0"))),
                        null)), entityQuery.Parameters);


        }

        protected NLinqQuery GetQuery(params Entity[] entities)
        {
            return GetQuery(FactoryAsync, entities);
        }

        public abstract void BeginLoadReference(AsyncCallback callback, string reference, IEnumerable<Entity> entities, NLinqQuery query, object asyncState);

        public abstract void EndLoadReference(IAsyncResult result);

        public virtual void BeginInitializeRepository(AsyncCallback callback, object asyncState)
        {
            Initialize();
        }

        public virtual void EndInitializeRepository(IAsyncResult result)
        {
        }

        #endregion
    }
}
