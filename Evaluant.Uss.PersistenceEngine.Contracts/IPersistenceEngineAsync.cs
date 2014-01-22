using System;
using Evaluant.NLinq;
using System.Collections.Generic;
using Evaluant.Uss.Domain;
using System.Globalization;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public delegate T LoadAction<T, U>(NLinqQuery query) where T : class, IEnumerable<U>;
    public delegate T LoadSingleAction<T>(NLinqQuery query);
    public delegate IList<Entity> LoadWithIdAction(string type, string[] ids);
    public delegate void LoadReferencesAction(string reference, IEnumerable<Domain.Entity> entities, NLinq.NLinqQuery queryUsedToLoadEntities);

    public delegate void Action();

    public interface IPersistenceEngineAsync
    {
        void BeginLoad(AsyncCallback callback, NLinqQuery query, object asyncState);
        void BeginLoad(AsyncCallback callback, string query, object asyncState);
        void BeginLoad(AsyncCallback callback, string query, int first, int max, object asyncState);


        // The only override to be implemented
        void BeginLoad(AsyncCallback callback, NLinqQuery query, int first, int max, object asyncState);
        IList<Entity> EndLoad(IAsyncResult result);

        void BeginLoadWithId(AsyncCallback callback, string type, string id, object asyncState);
        // The only override to be implemented
        void BeginLoadWithIds(AsyncCallback callback, string type, string[] id, object asyncState);

        Entity EndLoadWithId(IAsyncResult result);
        IList<Entity> EndLoadWithIds(IAsyncResult result);

        void BeginLoadReference(AsyncCallback callback, string reference, object asyncState, params Entity[] entities);
        void BeginLoadReference(AsyncCallback callback, string reference, IEnumerable<Domain.Entity> entities, NLinqQuery query, object asyncState);

        void BeginLoadScalar<T>(AsyncCallback callback, NLinqQuery query, object asyncState);
        void BeginLoadScalar<T, U>(AsyncCallback callback, NLinqQuery query, object asyncState) where T : class, IEnumerable<U>;
        void BeginLoadScalar<T, U>(AsyncCallback callback, NLinqQuery query, int first, int max, object asyncState) where T : class, IEnumerable<U>;
        void BeginLoadScalar<T>(AsyncCallback callback, string query, object asyncState);
        void BeginLoadScalar<T, U>(AsyncCallback callback, string query, int first, int max, object asyncState) where T : class, IEnumerable<U>;

        T EndLoadScalar<T>(IAsyncResult result);

        /// <summary>
        /// Deletes all the data contained in by the underlying repository
        /// </summary>
        void BeginInitializeRepository(AsyncCallback callback, object asyncState);
        void EndInitializeRepository(IAsyncResult result);

        /// <summary>
        /// Removes from memory all entities referenced by this engine
        /// </summary>
        void Initialize();

        void BeforeProcessCommands(Transaction tx);
        void BeginProcessCommands(Transaction tx, AsyncCallback callback, object asyncState);
        void EndProcessCommands(IAsyncResult result);
        void AfterProcessCommands(Transaction tx);

        IPersistenceProviderAsync FactoryAsync { get; }

        CultureInfo Culture { get; set; }

        void CreateId(Entity e);

    }
}
