using System;
using System.Collections;
using Evaluant.Uss.Commands;
//using Evaluant.Uss.Common;
using System.Globalization;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.NLinq;
using System.Collections.Generic;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Sync
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// The ids that are returned are those from the default persistence engine. Thus a TraceEngine can't be used 
    /// as a Default one.
    /// </remarks>
    public class SyncEngine : ProxyPersistenceEngine
    {
        private string clientId;

        public string ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }

        private IPersistenceEngine engine;
        public IPersistenceEngine Engine
        {
            get { return engine; }
            set { engine = value; }
        }

        private IPersistenceEngine metadataEngine;

        public IPersistenceEngine MetadataEngine
        {
            get { return metadataEngine; }
            set { metadataEngine = value; }
        }

        private IPersistenceEngine secondaryMetadataEngine;

        public IPersistenceEngine SecondaryMetadataEngine
        {
            get { return secondaryMetadataEngine; }
            set { secondaryMetadataEngine = value; }
        }

        private bool ignoreClientMetadata;

        public bool IgnoreClientMetadata
        {
            get { return ignoreClientMetadata; }
            set { ignoreClientMetadata = value; }
        }

        private int lastTransactionId;

        public int LastTransactionId
        {
            get { return lastTransactionId; }
        }

        public SyncEngine(IPersistenceEngine engine, IPersistenceEngine metadataEngine, IPersistenceEngine secondaryMetadataEngine, string clientId, SyncProvider provider)
            : base(engine, provider)
        {
            this.metadataEngine = metadataEngine;
            this.secondaryMetadataEngine = secondaryMetadataEngine;
            this.clientId = clientId;
        }


        /// <summary>
        /// Initializes information about the last transaction number
        /// </summary>
        /// <param name="engine"></param>
        private void InitializeInfo(IPersistenceEngine engine)
        {
            Entity e = new Entity(SyncUtils.INFO);
            e.SetValue(SyncUtils.TRANSACTION, 0);
            e.SetValue(SyncUtils.CLIENTID, String.Empty);

            Transaction t = new Transaction(engine.Factory.Model);
            t.Serialize(e);
            t.Commit(engine, false);
        }

        private void BeginInitializeInfo(AsyncCallback callback, IPersistenceEngineAsync engine)
        {
            Entity e = new Entity(SyncUtils.INFO);
            e.SetValue(SyncUtils.TRANSACTION, 0);
            e.SetValue(SyncUtils.CLIENTID, String.Empty);

            Transaction t = new Transaction(engine.FactoryAsync.Model);
            t.Serialize(e);
            t.BeginCommit(callback, engine, false, t);
        }

        private void EndInitalizeInfo(IAsyncResult result, IPersistenceEngineAsync engine)
        {
            ((Transaction)result.AsyncState).EndCommit(result, engine, false);
        }

        public override void InitializeRepository()
        {
            base.InitializeRepository();
            metadataEngine.InitializeRepository();
            InitializeInfo(metadataEngine);

            if (secondaryMetadataEngine != null)
            {
                secondaryMetadataEngine.InitializeRepository();
                InitializeInfo(secondaryMetadataEngine);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            metadataEngine.Initialize();

            if (secondaryMetadataEngine != null)
                secondaryMetadataEngine.Initialize();
        }

        /// <summary>
        /// Generates a TimeStamp like number representing an ordered transaction number
        /// </summary>
        /// <param name="metadataengine"></param>
        private void GenerateTransactionId(IPersistenceEngine engine)
        {
            Entity info;
            string guid;

            IList<Entity> es = engine.Load(SyncUtils.INFO);
            if (es.Count == 0)
            {
                InitializeInfo(engine);
                es = engine.Load(SyncUtils.INFO);
            }

            info = es[0];
            guid = Guid.NewGuid().ToString();

            do
            {
                lastTransactionId = info.GetInt32(SyncUtils.TRANSACTION);
                info.SetValue(SyncUtils.TRANSACTION, ++lastTransactionId);
                info.SetValue(SyncUtils.CLIENTID, guid);

                Transaction t = new Transaction(engine.Factory.Model);
                t.Serialize(info);
                t.Commit(engine, false);

                es = engine.Load(SyncUtils.INFO);
                if (es.Count == 0)
                {
                    InitializeInfo(engine);
                    es = engine.Load(SyncUtils.INFO);
                }

                info = es[0];

            } while (info.GetInt32(SyncUtils.TRANSACTION) != lastTransactionId || info.GetString(SyncUtils.CLIENTID) != guid);

        }

        public override void ProcessCommands(Transaction tx)
        {
            base.ProcessCommands(tx);

            Transaction t;
            SyncCommandProcessor syncCommandProcessor;

            GenerateTransactionId(metadataEngine);

            t = new Transaction(metadataEngine.Factory.Model);
            syncCommandProcessor = new SyncCommandProcessor(this, t, lastTransactionId);

            foreach (Command c in tx.PendingCommands)
            {
                // Ignores commands in metadata
                if (c.IgnoreMetadata || ignoreClientMetadata)
                    continue;

                syncCommandProcessor.Visit(c);
            }

            t.Commit(metadataEngine, false);

            if (secondaryMetadataEngine != null)
            {
                GenerateTransactionId(secondaryMetadataEngine);

                t = new Transaction(secondaryMetadataEngine.Factory.Model);
                syncCommandProcessor = new SyncCommandProcessor(this, t, lastTransactionId);

                foreach (Command c in tx.PendingCommands)
                {
                    syncCommandProcessor.Visit(c);
                }

                t.Commit(secondaryMetadataEngine, false);
            }
        }

        internal Command CreateCommand(Entity e)
        {
            switch (e.Type)
            {
                case SyncUtils.CREATE_ENTITY:
                    CreateEntityCommand ce = new CreateEntityCommand(
                        e.GetString(SyncUtils.PARENTID),
                        e.GetString(SyncUtils.TYPE)
                        );
                    return ce;

                case SyncUtils.DELETE_ENTITY:
                    DeleteEntityCommand de = new DeleteEntityCommand(
                        e.GetString(SyncUtils.PARENTID),
                        e.GetString(SyncUtils.TYPE)
                        );
                    return de;

                case SyncUtils.CREATE_ATTRIBUTE:
                    CreateAttributeCommand ca = new CreateAttributeCommand(
                        e.GetString(SyncUtils.PARENTID),
                        e.GetString(SyncUtils.PARENTTYPE),
                        e.GetString(SyncUtils.NAME),
                        MetaData.TypeResolver.GetType(e.GetString(SyncUtils.TYPE)),
                        Factory.Serializer.Unserialize(e.GetString(SyncUtils.VALUE))
                        );
                    return ca;

                case SyncUtils.DELETE_ATTRIBUTE:
                    DeleteAttributeCommand da = new DeleteAttributeCommand(
                        e.GetString(SyncUtils.PARENTID),
                        e.GetString(SyncUtils.PARENTTYPE),
                        e.GetString(SyncUtils.NAME),
                        MetaData.TypeResolver.GetType(e.GetString(SyncUtils.TYPE)),
                        null
                        );
                    return da;

                case SyncUtils.UPDATE_ATTRIBUTE:
                    UpdateAttributeCommand ua = new UpdateAttributeCommand(
                        e.GetString(SyncUtils.PARENTID),
                        e.GetString(SyncUtils.PARENTTYPE),
                        e.GetString(SyncUtils.NAME),
                        MetaData.TypeResolver.GetType(e.GetString(SyncUtils.TYPE)),
                        Factory.Serializer.Unserialize(e.GetString(SyncUtils.VALUE))
                        );
                    return ua;

                case SyncUtils.CREATE_REFERENCE:
                    CreateReferenceCommand cr = new CreateReferenceCommand(
                        e.GetString(SyncUtils.ROLE),
                        e.GetString(SyncUtils.PARENTID),
                        e.GetString(SyncUtils.PARENTTYPE),
                        e.GetString(SyncUtils.CHILDID),
                        e.GetString(SyncUtils.CHILDTYPE)
                        );
                    return cr;

                case SyncUtils.DELETE_REFERENCE:
                    DeleteReferenceCommand dr = new DeleteReferenceCommand(
                        e.GetString(SyncUtils.ROLE),
                        e.GetString(SyncUtils.PARENTID),
                        e.GetString(SyncUtils.PARENTTYPE),
                        e.GetString(SyncUtils.CHILDID),
                        e.GetString(SyncUtils.CHILDTYPE)
                        );
                    return dr;

                default:
                    throw new UniversalStorageException("Unexpected command type");
            }
        }

        public override void BeginInitializeRepository(AsyncCallback callback, object asyncState)
        {
            WaitForAll waitForAllCallback = new WaitForAll(callback, asyncState);
            base.BeginInitializeRepository(waitForAllCallback.Callback("engine"), asyncState);
            metadataEngine.BeginInitializeRepository(waitForAllCallback.Callback("metadata"), asyncState);
            BeginInitializeInfo(waitForAllCallback.Callback("metadata-transaction"), metadataEngine);

            if (secondaryMetadataEngine != null)
            {
                secondaryMetadataEngine.BeginInitializeRepository(waitForAllCallback.Callback("metadata2"), asyncState);
                BeginInitializeInfo(waitForAllCallback.Callback("metadata2-transaction"), secondaryMetadataEngine);
            }

        }

        public override void EndInitializeRepository(IAsyncResult result)
        {
            WaitForAll waitForAllCallback = (WaitForAll)result;
            base.EndInitializeRepository(waitForAllCallback["engine"]);
            metadataEngine.EndInitializeRepository(waitForAllCallback["metadata"]);
            EndInitalizeInfo(waitForAllCallback["metadata-transaction"], metadataEngine);
            if (secondaryMetadataEngine != null)
            {
                secondaryMetadataEngine.EndInitializeRepository(waitForAllCallback["engine"]);
                EndInitalizeInfo(waitForAllCallback["metadata2-transaction"], secondaryMetadataEngine);
            }
            base.EndInitializeRepository(waitForAllCallback["engine"]);
        }

        public override void BeginProcessCommands(Transaction tx, AsyncCallback callback, object asyncState)
        {
            WaitForAll waitForAllCallback = new WaitForAll(callback, asyncState);
            base.BeginProcessCommands(tx, waitForAllCallback.Callback("engine"), asyncState);

            Transaction t;
            SyncCommandProcessor syncCommandProcessor;

            GenerateTransactionId(metadataEngine);

            t = new Transaction(metadataEngine.Factory.Model);
            syncCommandProcessor = new SyncCommandProcessor(this, t, lastTransactionId);

            foreach (Command c in tx.PendingCommands)
            {
                // Ignores commands in metadata
                if (c.IgnoreMetadata || ignoreClientMetadata)
                    continue;

                syncCommandProcessor.Visit(c);
            }

            t.BeginCommit(waitForAllCallback.Callback("metadata"), metadataEngine, false, t);

            if (secondaryMetadataEngine != null)
            {
                GenerateTransactionId(secondaryMetadataEngine);

                t = new Transaction(secondaryMetadataEngine.Factory.Model);
                syncCommandProcessor = new SyncCommandProcessor(this, t, lastTransactionId);

                foreach (Command c in tx.PendingCommands)
                {
                    syncCommandProcessor.Visit(c);
                }

                t.BeginCommit(waitForAllCallback.Callback("metadata2"), secondaryMetadataEngine, false, t);
            }
        }

        public override void EndProcessCommands(IAsyncResult result)
        {
            WaitForAll waitForAllCallback = (WaitForAll)result;
            base.EndProcessCommands(waitForAllCallback["engine"]);
            result = waitForAllCallback["metadata"];
            Transaction t = (Transaction)result.AsyncState;
            t.EndCommit(result, metadataEngine, false);
            if (secondaryMetadataEngine != null)
            {
                result = waitForAllCallback["metadata2"];
                t = (Transaction)result.AsyncState;
                t.EndCommit(result, secondaryMetadataEngine, false);
            }
        }
    }
}
