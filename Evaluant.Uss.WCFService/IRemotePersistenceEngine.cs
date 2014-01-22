using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Evaluant.Uss.Domain;
using Evaluant.NLinq;
using Evaluant.Uss.WCFService.MessageContracts;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.WCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    [ServiceKnownType(typeof(EntitySet))]
    [ServiceKnownType(typeof(Entity))]
    [ServiceKnownType(typeof(Entry))]
    [ServiceKnownType(typeof(Entry<short>))]
    [ServiceKnownType(typeof(Entry<int>))]
    [ServiceKnownType(typeof(Entry<long>))]
    [ServiceKnownType(typeof(Entry<ushort>))]
    [ServiceKnownType(typeof(Entry<uint>))]
    [ServiceKnownType(typeof(Entry<ulong>))]
    [ServiceKnownType(typeof(Entry<short?>))]
    [ServiceKnownType(typeof(Entry<int?>))]
    [ServiceKnownType(typeof(Entry<long?>))]
    [ServiceKnownType(typeof(Entry<ushort?>))]
    [ServiceKnownType(typeof(Entry<uint?>))]
    [ServiceKnownType(typeof(Entry<ulong?>))]
    [ServiceKnownType(typeof(Entry<bool>))]
    [ServiceKnownType(typeof(Entry<bool?>))]
    [ServiceKnownType(typeof(Entry<string>))]
    [ServiceKnownType(typeof(Entry<Guid>))]
    [ServiceKnownType(typeof(Entry<byte[]>))]
    [ServiceKnownType(typeof(Entry<double>))]
    [ServiceKnownType(typeof(Entry<double?>))]
    [ServiceKnownType(typeof(Entry<float>))]
    [ServiceKnownType(typeof(Entry<float?>))]
    [ServiceKnownType(typeof(Entry<decimal>))]
    [ServiceKnownType(typeof(Entry<decimal?>))]
    [ServiceKnownType(typeof(Entry<byte>))]
    [ServiceKnownType(typeof(Entry<byte?>))]
    [ServiceKnownType(typeof(Entry<sbyte>))]
    [ServiceKnownType(typeof(Entry<sbyte?>))]
    //[ServiceKnownType(typeof(MultipleEntry))]
    public interface IRemotePersistenceEngine
    {
        [OperationContract(Name = "Load")]
        [PersistenceEngineBehavior()]
        IList<Entity> Load(string query);
        [OperationContract(Name = "LoadWithPaging")]
        [PersistenceEngineBehavior()]
        IList<Entity> Load(string query, int first, int max);

        [OperationContract(Name = "LoadWithId")]
        [PersistenceEngineBehavior()]
        Entity LoadWithId(string type, string id);
        // The only override to be implemented
        [OperationContract(Name="LoadWithIds")]
        [PersistenceEngineBehavior()]
        IList<Entity> LoadWithId(string type, string[] id);

        [OperationContract(Name = "LoadScalar")]
        [PersistenceEngineBehavior()]
        LoadScalarResponse LoadScalar(LoadScalarRequest request);


        /// <summary>
        /// Deletes all the data contained in by the underlying repository
        /// </summary>
        [OperationContract]
        void InitializeRepository();

        /// <summary>
        /// Removes from memory all entities referenced by this engine
        /// </summary>
        void Initialize();

        void BeforeProcessCommands(PersistenceEngine.Contracts.Transaction tx);
        [OperationContract]
        void ProcessCommands(IEnumerable<Surrogates.Command> commands);
        void AfterProcessCommands(PersistenceEngine.Contracts.Transaction tx);

        PersistenceEngine.Contracts.IPersistenceProvider Factory { get; }

        [OperationContract]
        [PersistenceEngineBehavior()]
        void CreateId(Entity e);
    }
}
