using System;
using System.Collections;
using System.Globalization;

using Evaluant.Uss;
//using Evaluant.Uss.Models;
using Evaluant.Uss.Commands;
using Evaluant.Uss.Collections;
using Evaluant.NLinq;
using System.Collections.Generic;
using Evaluant.Uss.Domain;
using System.ComponentModel;
using Evaluant.Uss.Utility;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    /// <summary>
    /// Description résumée de PersistenceEngineImplementation.
    /// </summary>
    public abstract class PersistenceEngineImplementation : PersistenceEngineAsyncImplementation, IPersistenceEngine
    {
        public PersistenceEngineImplementation(IPersistenceProvider factory) :
            this(factory, factory as IPersistenceProviderAsync)
        {
        }

        public PersistenceEngineImplementation(IPersistenceProvider factory, IPersistenceProviderAsync asyncFactory) :
            base(asyncFactory)
        {
            Factory = factory;
        }

        public IList<Entity> Load(string query)
        {
            return Load(new NLinqQuery(query));
        }

        public virtual IList<Entity> Load(string query, int first, int max)
        {
            return Load(new NLinqQuery(query), first, max);
        }

        public virtual IList<Entity> Load(NLinqQuery query)
        {
            return LoadMany<IList<Entity>, Entity>(query);
        }

        public virtual IList<Entity> Load(NLinqQuery query, int first, int max)
        {
            return Load(query.Page(first, max));
        }

        public Entity LoadWithId(string type, string id)
        {
            IList<Entity> result = LoadWithId(type, new string[] { id });
            return result.Count > 0 ? result[0] : null;
        }

        public abstract IList<Entity> LoadWithId(string type, string[] id);

        public void LoadReference(string reference, params Entity[] entities)
        {
            LoadReference(reference, entities, GetQuery(entities));
        }

        public abstract void LoadReference(string reference, IEnumerable<Entity> entities, NLinqQuery query);

        public virtual void InitializeRepository()
        {
            Initialize();
        }

        public override void BeforeProcessCommands(Transaction tx)
        {
            foreach (Domain.Entity entity in tx.ComputedEntities)
            {
                CreateId(entity);
            }

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

        public virtual double LoadScalar(NLinqQuery query)
        {
            return LoadScalar<double>(query);
        }

        public virtual T LoadScalar<T>(NLinqQuery query)
        {
            if (typeof(IEnumerable<>).IsAssignableFrom(typeof(T)))
            {
                throw new NotImplementedException();
                //return LoadMany<T>(query);
            }
            return LoadSingle<T>(query);
        }

        public virtual T LoadScalar<T, U>(NLinqQuery query)
            where T : class, IEnumerable<U>
        {
            return LoadMany<T, U>(query);
        }

        public virtual T LoadScalar<T, U>(NLinqQuery query, int first, int max)
            where T : class, IEnumerable<U>
        {
            return LoadScalar<T, U>(query.Page(first, max));
        }

        protected abstract T LoadSingle<T>(NLinqQuery query);
        protected abstract T LoadMany<T, U>(NLinqQuery query) where T : class, IEnumerable<U>;




        #region IPersistenceEngine Members


        public double LoadScalar(string query)
        {
            return LoadScalar<double>(query);
        }

        public T LoadScalar<T>(string query)
        {
            return LoadScalar<T>(new NLinqQuery(query));
        }

        public T LoadScalar<T, U>(string query, int first, int max) where T : class, IEnumerable<U>
        {
            return LoadScalar<T, U>(new NLinqQuery(query), first, max);

        }

        #endregion

        #region IPersistenceEngineAsync Members



        #endregion

        #region IPersistenceEngine Members


        public IPersistenceProvider Factory
        {
            get;
            internal set;
        }


        protected override CommandCollection ComputeCascadeDeleteCommands(string id, string type, IDictionary<string, Command> processed)
        {
            CommandCollection result = new CommandCollection();

            foreach (Model.Reference reference in Factory.Model.GetInheritedReferences(type))
            {
                // Search for composition relationships
                if (!reference.IsComposition)
                    continue;

                Entity e = new Entity(type);
                e.Id = id;

                this.LoadReference(reference.Name, e);

                foreach (Entry<Entity> subEntity in ((MultipleEntry)e[reference.Name]))
                {
                    string subKey = GetCommandKey(subEntity.TypedValue.Id, subEntity.TypedValue.Type);

                    if (!processed.ContainsKey(subKey))
                    {
                        DeleteEntityCommand nec = new DeleteEntityCommand(subEntity.TypedValue);
                        result.Add(nec);
                        processed.Add(subKey, nec);

                        // Recursive call
                        result.AddRange(ComputeCascadeDeleteCommands(subEntity.TypedValue.Id, subEntity.TypedValue.Type, processed));
                    }
                }
            }

            return result;
        }

        #endregion

        public override void BeginLoadWithIds(AsyncCallback callback, string type, string[] id, object asyncState)
        {
            DelegateCaller.BeginInvoke(new LoadWithIdAction(LoadWithId), new object[] { type, id }, callback, asyncState);
        }

        public override IList<Entity> EndLoadWithIds(IAsyncResult result)
        {
            return DelegateCaller.EndInvoke<IList<Entity>>(result);
        }

        protected override void BeginLoadSingle<T>(AsyncCallback callback, NLinqQuery query, object asyncState)
        {
            DelegateCaller.BeginInvoke(new LoadSingleAction<T>(LoadSingle<T>), new object[] { query }, callback, asyncState);
        }

        protected override void BeginLoadMany<T, U>(AsyncCallback callback, NLinqQuery query, object asyncState)
        {
            DelegateCaller.BeginInvoke(new LoadAction<T, U>(LoadMany<T, U>), new object[] { query }, callback, asyncState);
        }

        protected override T EndLoadSingle<T>(IAsyncResult result)
        {
            return DelegateCaller.EndInvoke<T>(result);
        }

        protected override T EndLoadMany<T, U>(IAsyncResult result)
        {
            return DelegateCaller.EndInvoke<T>(result);
        }

        public override void BeginLoadReference(AsyncCallback callback, string reference, IEnumerable<Entity> entities, NLinqQuery query, object asyncState)
        {
            DelegateCaller.BeginInvoke(new LoadReferencesAction(LoadReference), new object[] { reference, entities, query }, callback, asyncState);
        }

        public override void EndLoadReference(IAsyncResult result)
        {
            DelegateCaller.EndInvoke(result);
        }

        public override void BeginInitializeRepository(AsyncCallback callback, object asyncState)
        {
            DelegateCaller.BeginInvoke(new Action(InitializeRepository), null, callback, asyncState);
        }

        public override void EndInitializeRepository(IAsyncResult result)
        {
            DelegateCaller.EndInvoke(result);
        }

        #region IPersistenceEngine Members


        public abstract void ProcessCommands(Transaction tx);

        #endregion

        public override void BeginProcessCommands(Transaction tx, AsyncCallback callback, object asyncState)
        {
            DelegateCaller.BeginInvoke(new Action<Transaction>(ProcessCommands), new object[] { tx }, callback, asyncState);
        }

        public override void EndProcessCommands(IAsyncResult result)
        {
            DelegateCaller.EndInvoke(result);
        }
    }
}
