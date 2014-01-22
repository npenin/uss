using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Reflection;
using Evaluant.Uss.Configuration;
using Evaluant.NLinq;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.WCFService
{
    public class WcfPersistenceEngine : PersistenceEngine.Contracts.ProxyPersistenceEngine, IRemotePersistenceEngine
    {
        static WcfPersistenceEngine()
        {
            foreach (MethodInfo mi in typeof(IRemotePersistenceEngine).GetMethods())
            {
                ParameterInfo[] parameters = mi.GetParameters();
                if (parameters != null && parameters.Length == 1 && mi.IsGenericMethodDefinition && parameters[0].ParameterType == typeof(string))
                    LoadSingle = mi;
                if (parameters != null && parameters.Length == 3 && mi.IsGenericMethodDefinition && parameters[0].ParameterType == typeof(string))
                    LoadMany = mi;
            }
            provider = ConfigLoader.LoadConfig();
        }

        public WcfPersistenceEngine()
            : base(provider.CreatePersistenceEngine(), provider)
        {

        }

        private static MethodInfo LoadSingle, LoadMany;
        private static PersistenceEngine.Contracts.IPersistenceProvider provider;

        #region IPersistenceEngine Members


        public MessageContracts.LoadScalarResponse LoadScalar(MessageContracts.LoadScalarRequest request)
        {
            Type returnType = Type.GetType(request.ExpectedType);
            var response = new MessageContracts.LoadScalarResponse();
            if (request.First == -1)
                response.result = LoadSingle.Invoke(this, new object[] { request.Query });
            else
                response.result = LoadMany.Invoke(this, new object[] { request.Query, request.First, request.Max });

            response.returnType = response.result.GetType().FullName;
            return response;
        }

        #endregion

        #region IRemotePersistenceEngine Members


        public void ProcessCommands(IEnumerable<Surrogates.Command> commands)
        {
            Transaction tx = new Transaction(Factory.Model);
            foreach (var command in commands)
            {
                switch (command.Type)
                {
                    case Evaluant.Uss.Commands.CommandTypes.CompoundCreate:
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.CompoundUpdate:
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.CreateAttribute:
                        tx.PendingCommands.Add(new CreateAttributeCommand());
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.CreateEntity:
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.CreateReference:
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.DeleteAttribute:
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.DeleteEntity:
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.DeleteReference:
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.UpdateAttribute:
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
