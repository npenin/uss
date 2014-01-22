using System;
using System.Collections;
using System.IO;

//using Evaluant.Uss.Commands;
//using Evaluant.Uss.Models;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;
using System.Collections.Generic;
using Evaluant.NLinq;

namespace Evaluant.Uss.Remoting
{
    /// <summary>
    /// Simple Delegate Engine implementation to show the interception capabilities of the framework
    /// </summary>
    public class RemoteController : MarshalByRefObject
    {
        private IPersistenceEngine _Delegator;

        public RemoteController()
        {
#if DEBUG
            _Delegator = XmlConfigLoader.LoadXmlConfig("..\\..\\test.engines.config").CreatePersistenceEngine();
#else
			_Delegator = XmlConfigLoader.LoadXmlConfig("engines.config").CreatePersistenceEngine();
#endif
            System.Diagnostics.Trace.WriteLine("Persistence engine : " + _Delegator.ToString());
        }

        //		public RemoteController(string configFileName)
        //		{
        //			if(!File.Exists(configFileName))
        //				throw new PersistenceEngineException("Configuration file was not found to init the server:\n" + configFileName);
        //
        //		}

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public IList<Entity> Load(string opath)
        {
            return _Delegator.Load(opath);
        }

        public IList<Entity> Load(string query, int first, int max)
        {
            return _Delegator.Load(query, first, max);
        }

        public Entity LoadWithId(string type, string id)
        {
            return _Delegator.LoadWithId(type, id);
        }

        public IList<Entity> LoadWithId(string type, string[] ids)
        {
            return _Delegator.LoadWithId(type, ids);
        }

        public void LoadReference(Entity entity)
        {
            _Delegator.LoadReference(entity);
        }

        public Entity LoadReference(Entity entity, string[] references)
        {
            _Delegator.LoadReference(entity, references);
            return entity;
        }

        public IEnumerable<Entity> LoadReference(IEnumerable<Entity> entities)
        {
            _Delegator.LoadReference(entities);


            return entities;
        }

        public IEnumerable<Entity> LoadReference(IEnumerable<Entity> entities, string[] references)
        {
            _Delegator.LoadReference(entities, references);

            return entities;
        }

        public object LoadScalar(string query)
        {
            return _Delegator.LoadScalar(query);
        }

        public object LoadScalar(NLinqQuery opath)
        {
            return _Delegator.LoadScalar(opath);
        }

        public void InitializeRepository()
        {
            _Delegator.InitializeRepository();
        }

        public void Initialize()
        {
            _Delegator.Initialize();
        }

        public Transaction BeforeProcessCommands(Transaction tx)
        {
            _Delegator.BeforeProcessCommands(tx);
            return tx;
        }

        public Transaction ProcessCommands(Transaction tx)
        {
            _Delegator.ProcessCommands(tx);
            return tx;
        }

        public Transaction AfterProcessCommands(Transaction tx)
        {
            _Delegator.AfterProcessCommands(tx);
            return tx;
        }

        public Models.Model Model
        {
            get
            {
                return _Delegator.Factory.Model;
            }
        }
    }
}
