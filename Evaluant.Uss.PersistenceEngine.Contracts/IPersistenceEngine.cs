using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.Domain;
using Evaluant.NLinq;
using System.Globalization;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public delegate void EussEventHandler(object sender, EntityEventArgs e);
    public delegate void EussCancelEventHandler(object sender, CancelEntityEventArgs e);
    public delegate void EussReferenceEventHandler(object sender, ReferenceEventArgs e);
    public delegate void EussCancelReferenceEventHandler(object sender, ReferenceCancelEventArgs e);

    public interface IPersistenceEngine : IPersistenceEngineAsync
    {
        IList<Entity> Load(NLinqQuery query);
        IList<Entity> Load(string query);
        IList<Entity> Load(string query, int first, int max);

        // The only override to be implemented
        IList<Entity> Load(NLinqQuery query, int first, int max);

        Entity LoadWithId(string type, string id);
        // The only override to be implemented
        IList<Entity> LoadWithId(string type, string[] id);

        void LoadReference(string reference, params Entity[] entity);
        void LoadReference(string reference, IEnumerable<Domain.Entity> entities, NLinq.NLinqQuery queryUsedToLoadEntities);

        double LoadScalar(NLinqQuery query);
        T LoadScalar<T>(NLinqQuery query);
        T LoadScalar<T, U>(NLinqQuery query) where T : class, IEnumerable<U>;
        T LoadScalar<T, U>(NLinqQuery query, int first, int max) where T : class, IEnumerable<U>;
        double LoadScalar(string query);
        T LoadScalar<T>(string query);
        T LoadScalar<T, U>(string query, int first, int max) where T : class, IEnumerable<U>;


        /// <summary>
        /// Deletes all the data contained in by the underlying repository
        /// </summary>
        void InitializeRepository();

        /// <summary>
        /// Removes from memory all entities referenced by this engine
        /// </summary>
        void ProcessCommands(Transaction tx);

        IPersistenceProvider Factory { get; }
    }
}
