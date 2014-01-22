using System;
using System.Collections;
using System.Text;

using Evaluant.Uss;
using Evaluant.Uss.Commands;
using Evaluant.Uss.Collections;
using System.Collections.Specialized;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;
using System.Collections.Generic;

namespace Evaluant.Uss.Sync
{
    public enum Peer { Client, Server }

    public class Synchronizer
    {
        private static DateTime MINDATETIME = new DateTime(1901, 1, 1);
        public event EntityEventHandler EntitySynchronized;
        public event ConflictEventHandler Conflict;
        public event ProgressEventHandler Progressed;

        private StringCollection filters = new StringCollection();
        public StringCollection Filters
        {
            get { return filters; }
            set { filters = value; }
        }

        public Synchronizer(TimeSpan tombstone)
        {
            this.tombstone = tombstone;
        }

        public Synchronizer()
        {
            this.tombstone = TimeSpan.MaxValue;
        }

        private TimeSpan tombstone = TimeSpan.MaxValue;

        public TimeSpan Tombstone
        {
            get { return tombstone; }
            set { tombstone = value; }
        }

        public void Synchronize(IPersistenceEngine client, IPersistenceEngine server, SyncDirection syncDirection)
        {
            Synchronize(client, server, syncDirection, ((SyncEngine)client).ClientId);
        }

        /// <summary>
        /// Gets the peer metadata engine.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="peer">The peer.</param>
        /// <returns></returns>
        private IPersistenceEngine GetPeerMetadataEngine(SyncEngine engine, Peer peer)
        {
            if (engine.SecondaryMetadataEngine == null || peer == Peer.Client)
                return engine.MetadataEngine;

            return engine.SecondaryMetadataEngine;
        }

        /// <summary>
        /// Determines whether the specified engine is a central node .
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <returns>
        /// 	<c>true</c> if the specified engine is a central node; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCentralNode(SyncEngine engine)
        {
            return engine.MetadataEngine != null && engine.SecondaryMetadataEngine != null;
        }

        /// <summary>
        /// Gets the last connection information for a given client
        /// </summary>
        /// <param name="clientId">The Id of the client the connection should be returns</param>
        /// <param name="server">The SyncEngine instance for reading connection information </param>
        /// <returns>The last connection information for a given client</returns>
        private Entity GetConnection(string clientId, SyncEngine server)
        {
            Entity connection = null;
            IList<Entity> connections = GetPeerMetadataEngine(server, Peer.Server).Load(String.Format("Evaluant:Uss:Sync:Connection[ClientId = '{0}']", clientId));

            if (connections.Count > 0)
                connection = connections[0];
            else
            {
                connection = new Entity(SyncUtils.CONNECTION);
                connection[SyncUtils.CLIENTID].Value = clientId;
                connection[SyncUtils.TRANSACTION].Value = 0;
            }

            return connection;
        }

        private static readonly char SEPARATOR = '\u0000';

        /// <summary>
        /// Creates Compound commands if necessary
        /// </summary>
        /// <param name="commands"></param>
        private Command[] OptimizeCommands(SyncEngine engine, IList<Entity> commands)
        {
            if (commands == null)
                throw new ArgumentNullException("commands");

            if (commands.Count == 0)
                return new Command[0];

            HashedList<Command> optimizedCommands = new HashedList<Command>();

            System.Collections.Generic.List<CompoundCreateCommand> createdCommands = new System.Collections.Generic.List<CompoundCreateCommand>();

            int j;

            for (int i = 0; i < commands.Count; i++)
            {
                Entity e = commands[i];

                string currentId = e.GetString(SyncUtils.PARENTID);
                int transaction = e.GetInt32(SyncUtils.TRANSACTION);

                switch (e.Type)
                {
                    case SyncUtils.CREATE_ENTITY:

                        string createType = e.GetString(SyncUtils.TYPE);

                        j = i + 1;

                        CompoundCreateCommand ccc = new CompoundCreateCommand(currentId, createType, new List<AttributeCommand>());

                        CompoundCreateCommand actual = createdCommands.Find(delegate(CompoundCreateCommand toFind)
                        {
                            return toFind.ClientId == ccc.ClientId &&
                                toFind.ParentId == ccc.ParentId &&
                                toFind.Type == ccc.Type;
                        });

                        if (actual == null)
                        {
                            createdCommands.Add(ccc);
                            optimizedCommands.Add(ccc);

                            while (j < commands.Count)
                            {

                                string subType = commands[j].GetString(SyncUtils.PARENTTYPE);
                                string subId = commands[j].GetString(SyncUtils.PARENTID);
                                int subTransaction = commands[j].GetInt32(SyncUtils.TRANSACTION);
                                string subCommand = commands[j].Type;

                                if (commands[j].Type == SyncUtils.CREATE_ATTRIBUTE && subId == currentId && subType == createType)
                                {
                                    if (subTransaction != transaction)
                                        break;

                                    ccc.InnerCommands.Add((AttributeCommand)engine.CreateCommand(commands[j]));
                                    commands.RemoveAt(j);
                                }
                                else
                                {
                                    j++;
                                }
                            }
                        }
                        else
                        {
                            optimizedCommands.Remove(actual);
                            optimizedCommands.Add(ccc);

                            createdCommands.Remove(actual);
                            createdCommands.Add(ccc);
                        }

                        break;

                    case SyncUtils.UPDATE_ATTRIBUTE:

                        string updateType = e.GetString(SyncUtils.PARENTTYPE);

                        j = i + 1;

                        CompoundUpdateCommand cuc = new CompoundUpdateCommand(currentId, updateType, new List<AttributeCommand>());
                        cuc.InnerCommands.Add((AttributeCommand)engine.CreateCommand(commands[i]));
                        optimizedCommands.Add(cuc);

                        while (j < commands.Count)
                        {
                            string subType = commands[j].GetString(SyncUtils.PARENTTYPE);
                            string subId = commands[j].GetString(SyncUtils.PARENTID);
                            int subTransaction = commands[j].GetInt32(SyncUtils.TRANSACTION);
                            string subCommand = commands[j].Type;

                            if (commands[j].Type == SyncUtils.UPDATE_ATTRIBUTE && subId == currentId && subType == updateType)
                            {
                                if (subTransaction != transaction)
                                    break;

                                cuc.InnerCommands.Add((AttributeCommand)engine.CreateCommand(commands[j]));
                                commands.RemoveAt(j);
                            }
                            else
                            {
                                j++;
                            }
                        }

                        break;

                    default:
                        optimizedCommands.Add(engine.CreateCommand(e));
                        break;

                }
            }

            Command[] result = new Command[optimizedCommands.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = (Command)optimizedCommands[i];

            return result;
        }

        public void Cancel()
        {

        }

        /// <summary>
        /// Synchronizes the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="server">The server.</param>
        /// <param name="syncDirection">The sync direction.</param>
        /// <param name="clientId">The client id.</param>
        public void Synchronize(IPersistenceEngine client, IPersistenceEngine server, SyncDirection syncDirection, string clientId)
        {
            switch (syncDirection)
            {
                case SyncDirection.FullDownload:
                    FullDownload(client, server, true);
                    break;
                case SyncDirection.FullDownloadNoBulk:
                    FullDownload(client, server, false);
                    break;
                case SyncDirection.SmartDownload:
                    SmartDownload(client, server as SyncEngine, clientId);
                    break;

                case SyncDirection.SmartUpload:
                    SmartUpload(client as SyncEngine, server);
                    break;

                case SyncDirection.SmartUploadDownload:
                    SmartUploadDownload(client as SyncEngine, server as SyncEngine);
                    break;
            }
        }

        private void SmartDownload(IPersistenceEngine client, SyncEngine server, string clientId)
        {
            if (client == null)
                throw new ArgumentException("The client engine must be not null");

            if (server == null)
                throw new ArgumentException("The server engine must be not null and a SyncProvider");

            Entity connection = GetConnection(clientId, server);
            int lastSync = connection.GetInt32(SyncUtils.TRANSACTION);

            Command[] commands = OptimizeCommands((SyncEngine)client,
                    GetPeerMetadataEngine(server, Peer.Server).Load("from Evaluant.Uss.Sync.Command c where (c.ClientId!='" + client + "' || c.ClientId==null) and c.Transaction>" + lastSync.ToString() + " orderby c.Processed, c.Number", 1, 0)
                    );

            StringCollection dataToProcess = new StringCollection();
            foreach (string filter in filters)
                foreach (Entity e in server.Load(filter))
                    dataToProcess.Add(string.Concat(e.Type, SEPARATOR, e.Id));

            Transaction t = new Transaction(client.Factory.Model);
            foreach (Command c in commands)
            {
                string key = string.Empty;

                CompoundCreateCommand ccc = c as CompoundCreateCommand;
                if (ccc != null)
                    key = string.Concat(ccc.Type, SEPARATOR, ccc.ParentId);

                CompoundUpdateCommand cuc = c as CompoundUpdateCommand;
                if (cuc != null)
                    key = string.Concat(cuc.ParentType, SEPARATOR, cuc.ParentId);

                if (key != null && key != string.Empty && filters.Count > 0 && !dataToProcess.Contains(key))
                    continue;

                c.IgnoreMetadata = true;
                t.PushCommand(c);
            }
            t.Commit(client, false);

            Entity info = GetPeerMetadataEngine(server, Peer.Server).Load(SyncUtils.INFO)[0];
            connection[SyncUtils.TRANSACTION].Value = info.GetInt32(SyncUtils.TRANSACTION);

            IPersistenceEngine peerMetadataEngine = GetPeerMetadataEngine(server, Peer.Server);

            t = new Transaction(peerMetadataEngine.Factory.Model);

            t.Serialize(connection);
            t.Commit(peerMetadataEngine, false);
        }

        private void SmartUpload(SyncEngine client, IPersistenceEngine server)
        {
            if (server == null)
                throw new ArgumentException("The server engine must be not null");

            if (client == null)
                throw new ArgumentException("The client engine must be not null and a SyncProvider");

            IList<Entity> commands = GetPeerMetadataEngine(client, Peer.Client).Load("from " + SyncUtils.COMMAND + " c orderby c.Processed, c.Number select c", 1, 0);

            EntitySet commandsToRemove = ((EntitySet)commands).Clone();

            Command[] optimized = OptimizeCommands(client, commands);

            Transaction t = new Transaction(client.MetadataEngine.Factory.Model);
            foreach (Command c in optimized)
            {
                c.ClientId = client.ClientId;

                CompoundCreateCommand ccc = c as CompoundCreateCommand;
                if (ccc != null)
                    foreach (Command cmd in ccc.InnerCommands)
                        cmd.ClientId = client.ClientId;

                t.PushCommand(c);
            }
            t.Commit(server, false);

            // Deletes the metadata once they are processed
            t = new Transaction(client.MetadataEngine.Factory.Model);
            t.Delete(commandsToRemove);
            t.Commit(client.MetadataEngine, false);
        }

        private void SmartUploadDownload(SyncEngine client, SyncEngine server)
        {
            Transaction t;

            #region Download

            Entity connection = GetConnection(client.ClientId, server);

            int lastTransactionId = GetLastTransaction(connection);

            IList<Entity> serverCommands = GetPeerMetadataEngine(server, Peer.Server).Load(String.Format("from {2} c where (c.ClientId != '{0}' || c.ClientId==null)) && Transaction > {1} orderby c.Processed, c.Number", client.ClientId, lastTransactionId, SyncUtils.COMMAND), 1, 0);
            IList<Entity> clientCommands = GetPeerMetadataEngine(client, Peer.Client).Load("from " + SyncUtils.COMMAND + " c orderby Processed, Number select c", 1, 0);

            // Detecting ClientCreateOrUpdateClientDelete to delete the UpdateCommands which occured before a delete
            IPersistenceEngine peerMetadataEngine = GetPeerMetadataEngine(client, Peer.Client);
            t = new Transaction(peerMetadataEngine.Factory.Model);
            for (int i = clientCommands.Count - 1; i >= 0; i--)
            {
                Entity ce = clientCommands[i];

                if (ce.Type == SyncUtils.DELETE_ENTITY)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        {
                            Entity se = clientCommands[j];

                            if (ApplyOnTheSameEntity(ce, se))
                            {
                                clientCommands.RemoveAt(j);
                                t.Delete(se);
                                i--;

                                if (se.Type == SyncUtils.CREATE_ENTITY)
                                {
                                    clientCommands.Remove(ce);
                                    t.Delete(ce);
                                    i--;
                                }
                            }
                        }
                    }
                }
            }
            t.Commit(peerMetadataEngine, false);

            // Detecting ClientDeleteServerDelete to ignore the Delete commands from the Client
            for (int i = clientCommands.Count - 1; i >= 0; i--)
            {
                Entity ce = clientCommands[i];

                if (ce.Type == SyncUtils.DELETE_ENTITY)
                {
                    // A delete occured on the client, verify if this command is already on the server
                    for (int j = serverCommands.Count - 1; j >= 0; j--)
                    {
                        Entity se = serverCommands[j];

                        if (se.Type == SyncUtils.DELETE_ENTITY
                                && se.GetString(SyncUtils.PARENTID) == ce.GetString(SyncUtils.PARENTID))
                        {
                            clientCommands.Remove(ce);
                            serverCommands.Remove(se);
                        }
                    }
                }
            }

            // Detecting ClientDeleteServerUpdate to delete the UpdateCommands which occured later on the server
            t = new Transaction(GetPeerMetadataEngine(server, Peer.Server).Factory.Model);
            for (int i = clientCommands.Count - 1; i >= 0; i--)
            {
                Entity ce = clientCommands[i];

                if (ce.Type == SyncUtils.DELETE_ENTITY)
                {
                    // A delete occured on the client, verify no update command where set on the server
                    for (int j = serverCommands.Count - 1; j >= 0; j--)
                    {
                        Entity se = serverCommands[j];

                        if (se.Type == SyncUtils.UPDATE_ATTRIBUTE
                                && se.GetString(SyncUtils.PARENTID) == ce.GetString(SyncUtils.PARENTID))
                        {
                            serverCommands.Remove(se);
                            t.Delete(se);
                        }
                    }
                }
            }
            t.Commit(GetPeerMetadataEngine(server, Peer.Server), false);

            // Detecting ServerDeleteClientUpdate to delete the UpdateCommands which occured later on the client
            t = new Transaction(GetPeerMetadataEngine(server, Peer.Client).Factory.Model);
            for (int i = serverCommands.Count - 1; i >= 0; i--)
            {
                Entity se = serverCommands[i];

                if (se.Type == SyncUtils.DELETE_ENTITY)
                {

                    // A delete occured on the server, verify no update command where set on the client
                    for (int j = clientCommands.Count - 1; j >= 0; j--)
                    {
                        Entity ce = clientCommands[j];

                        if (ce.Type == SyncUtils.UPDATE_ATTRIBUTE
                                && ce.GetString(SyncUtils.PARENTID) == se.GetString(SyncUtils.PARENTID))
                        {
                            clientCommands.Remove(ce);
                            t.Delete(ce);
                        }
                    }
                }
            }
            t.Commit(GetPeerMetadataEngine(client, Peer.Client), false);

            // Detecting ServerUpdateClientUpdate to delete the UpdateCommands which occured later on the client
            Transaction ct = new Transaction(GetPeerMetadataEngine(server, Peer.Client).Factory.Model);
            Transaction st = new Transaction(GetPeerMetadataEngine(server, Peer.Server).Factory.Model);
            for (int i = serverCommands.Count - 1; i >= 0; i--)
            {
                Entity se = serverCommands[i];

                if (se.Type == SyncUtils.UPDATE_ATTRIBUTE)
                {
                    for (int j = clientCommands.Count - 1; j >= 0; j--)
                    {
                        Entity ce = clientCommands[j];

                        if (ce.Type == SyncUtils.UPDATE_ATTRIBUTE
                            && ce.GetString(SyncUtils.PARENTID) == se.GetString(SyncUtils.PARENTID)
                            && ce.GetString(SyncUtils.NAME) == se.GetString(SyncUtils.NAME))
                        {
                            ConflictEventArgs args = new ConflictEventArgs(client.CreateCommand(ce), client.CreateCommand(se), SyncConflict.ClientUpdateServerUpdate);
                            if (Conflict != null)
                                Conflict(this, args);

                            if (args.Resolution == ConflictResolution.ClientWins)
                            {
                                serverCommands.Remove(se);
                                st.Delete(se);
                            }
                            else
                            {
                                clientCommands.Remove(ce);
                                ct.Delete(ce);
                            }
                        }
                    }
                }
            }
            ct.Commit(GetPeerMetadataEngine(client, Peer.Client), false);
            st.Commit(GetPeerMetadataEngine(server, Peer.Server), false);

            EntitySet clientCommandsToRemove = ((EntitySet)clientCommands).Clone();
            EntitySet serverCommandsToRemove = ((EntitySet)serverCommands).Clone();

            Command[] optimizedClientCommands = OptimizeCommands(client, clientCommands);
            Command[] optimizedServerCommands = OptimizeCommands(server, serverCommands);

            StringCollection dataToProcess = new StringCollection();
            foreach (string filter in filters)
                foreach (Entity e in server.Load(filter))
                    dataToProcess.Add(string.Concat(e.Type, SEPARATOR, e.Id));

            t = new Transaction(client.Factory.Model);
            foreach (Command c in optimizedServerCommands)
            {
                string key = string.Empty;

                CompoundCreateCommand ccc = c as CompoundCreateCommand;
                if (ccc != null)
                    key = string.Concat(ccc.Type, SEPARATOR, ccc.ParentId);

                CompoundUpdateCommand cuc = c as CompoundUpdateCommand;
                if (cuc != null)
                    key = string.Concat(cuc.ParentType, SEPARATOR, cuc.ParentId);

                DeleteEntityCommand dec = c as DeleteEntityCommand;
                if (dec != null)
                    key = string.Concat(dec.Type, SEPARATOR, dec.ParentId);

                DeleteAttributeCommand dac = c as DeleteAttributeCommand;
                if (dac != null)
                    key = string.Concat(dac.ParentType, SEPARATOR, dac.ParentId);

                CreateReferenceCommand crc = c as CreateReferenceCommand;
                if (crc != null)
                    key = string.Concat(crc.ParentType, SEPARATOR, crc.ParentId);

                DeleteReferenceCommand drc = c as DeleteReferenceCommand;
                if (drc != null)
                    key = string.Concat(drc.ParentType, SEPARATOR, drc.ParentId);

                if (key != null && key != string.Empty && filters.Count > 0 && !dataToProcess.Contains(key))
                    continue;

                c.IgnoreMetadata = true;
                t.PushCommand(c);
            }
            t.Commit(client, false);

            #endregion

            #region Upload

            if (optimizedClientCommands.Length > 0)
            {
                t = new Transaction(server.Factory.Model);
                t.PushCommand(optimizedClientCommands);
                t.Commit(server, false);

            }

            // Updates the last transaction id for the client
            Entity info = GetPeerMetadataEngine(server, Peer.Server).Load(SyncUtils.INFO)[0];
            connection[SyncUtils.TRANSACTION].Value = info.GetInt32(SyncUtils.TRANSACTION);

            t = new Transaction(GetPeerMetadataEngine(server, Peer.Server).Factory.Model);

            t.Serialize(connection);
            t.Commit(GetPeerMetadataEngine(server, Peer.Server), false);

            // Deletes the metadata once they are processed
            if (clientCommands.Count > 0)
            {
                t = new Transaction(GetPeerMetadataEngine(server, Peer.Client).Factory.Model);
                t.Delete(clientCommandsToRemove);
                t.Commit(GetPeerMetadataEngine(client, Peer.Client), false);
            }

            // Deletes all tombstoned server commands
            if (tombstone != TimeSpan.MaxValue)
            {
                serverCommands = GetPeerMetadataEngine(server, Peer.Server).Load("from Evaluant.Uss.Sync.Command c where c.Processed > #" + DateTime.Now.Add(-tombstone) + "# select c");
                t = new Transaction(GetPeerMetadataEngine(server, Peer.Server).Factory.Model);
                t.Delete(serverCommandsToRemove);
                t.Commit(GetPeerMetadataEngine(server, Peer.Server), false);
            }

            #endregion
        }

        /// <summary>
        /// Gets the last transaction.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>The value of the last transaction a client participated to</returns>
        private int GetLastTransaction(Entity connection)
        {
            int lastTransactionId = connection.GetInt32(SyncUtils.TRANSACTION);
            return lastTransactionId;
        }

        private void FullDownload(IPersistenceEngine client, IPersistenceEngine server, bool bulk)
        {
            if (client is SyncEngine)
            {
                ((SyncEngine)client).IgnoreClientMetadata = true;
            }

            // Contains id translation table
            Hashtable translationTable = new Hashtable();
            Hashtable reverseTable = new Hashtable();

            Transaction t = new Transaction(client.Factory.Model);

            StringCollection queries = new StringCollection();

            // Creates the list of items to load depending whether filters are set or not
            if (filters.Count > 0)
            {
                queries = filters;
            }
            else
            {
                foreach (Model.Entity entity in server.Factory.Model.Entities.Values)
                {
                    // Process only root types as it will load all children
                    if (entity.Inherit != String.Empty && entity.Inherit != null && server.Factory.Model.Entities.ContainsKey(entity.Inherit))
                        continue;

                    queries.Add("from " + entity.Type + " e select e");
                }
            }

            if (Progressed != null)
            {
                // Returns opaths.Count * 2 so that half the processed is reached once entities are processed
                Progressed(this, new ProgressEventArgs(0, "Synchronization started", queries.Count * 2));
            }

            int position = 0;

            // Import all entities
            foreach (string filter in queries)
            {

                if (Progressed != null)
                {
                    // Returns opaths.Count * 2 so that half the processed is reached once entities are processed
                    ProgressEventArgs args = new ProgressEventArgs(position++, "Processing " + filter, queries.Count * 2);
                    Progressed(this, args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }

                int count = Convert.ToInt32(server.LoadScalar(String.Concat("(", filter, ").Count()")));
                //                System.Diagnostics.Trace.WriteLine("Processing " + opaths + "(" + count.ToString() + ")");

                // Loads a set of entities (span)
                if (count > 0)
                {
                    t = new Transaction(client.Factory.Model);

                    IList<Entity> entities = server.Load(filter);

                    foreach (Entity e in entities)
                    {
                        e.State = State.New;
                        t.Serialize(e);
                    }

                    t.Commit(client, false);

                    // Retrieves the translated ids
                    foreach (KeyValuePair<string, string> de in t.NewIds)
                    {
                        translationTable.Add(String.Concat(entities[0].Type, ".", de.Key), de.Value);
                        reverseTable.Add(String.Concat(entities[0].Type, ".", de.Value), de.Key);
                    }
                }
            }

            if (bulk)
            {
                t = new Transaction(client.Factory.Model);
            }

            position = 0;

            // Import all relationships
            foreach (Model.Entity entity in client.Factory.Model.Entities.Values)
            {
                if (Progressed != null)
                {
                    ProgressEventArgs args = new ProgressEventArgs(queries.Count + position++, "Processing " + entity.Type + " relationships", queries.Count + client.Factory.Model.Entities.Count);
                    Progressed(this, args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }

                // Process only root types as it will load all children
                if (entity.Inherit != String.Empty && entity.Inherit != null)
                    continue;

                foreach (Entity parent in client.Load(entity.Type))
                {
                    foreach (Model.Reference reference in server.Factory.Model.GetInheritedReferences(parent.Type))
                    {
                        string uniqueParentKey = String.Concat(entity.Type, ".", parent.Id);
                        string reversedParentId = reverseTable.Contains(uniqueParentKey) ? reverseTable[uniqueParentKey].ToString() : parent.Id;

                        string query = String.Concat(entity.Type, "[id('", reversedParentId, "')].", reference.Name);
                        int count = Convert.ToInt32(server.LoadScalar(String.Concat("count(", query, ")")));

                        // Loads a set of entities (span)
                        if (count > 0)
                        {
                            if (!bulk)
                            {
                                t = new Transaction(client.Factory.Model);
                            }

                            IList<Entity> entities = server.Load(query);

                            foreach (Entity e in entities)
                            {
                                string uniqueChildKey = String.Concat(e.Type, ".", e.Id);
                                string childId = translationTable.Contains(uniqueChildKey) ? translationTable[uniqueChildKey].ToString() : e.Id;

                                t.PushCommand(new Commands.CreateReferenceCommand(reference, parent, e));
                            }

                            if (!bulk)
                            {
                                t.Commit(client, false);
                            }
                        }
                    }
                }
            }

            if (bulk)
            {
                t.Commit(client, false);
            }

            // Creates a Connection token if the providers permit it
            if (client is SyncEngine && server is SyncEngine)
            {
                Entity connection = GetConnection(((SyncEngine)client).ClientId, (SyncEngine)server);

                Transaction it;
                Entity info = null;
                IPersistenceEngine engine = GetPeerMetadataEngine((SyncEngine)server, Peer.Server);
                IList<Entity> infos = engine.Load("from " + SyncUtils.INFO + "i select i");

                if (infos.Count > 0)
                {
                    info = infos[0];
                }
                else
                {
                    info = new Entity(SyncUtils.INFO);
                    info.SetValue(SyncUtils.TRANSACTION, 0);
                    info.SetValue(SyncUtils.CLIENTID, String.Empty);

                    it = new Transaction(engine.Factory.Model);
                    it.Serialize(info);
                    it.Commit(engine, false);
                }

                connection.SetValue(SyncUtils.TRANSACTION, info.GetInt32(SyncUtils.TRANSACTION));

                it = new Transaction(engine.Factory.Model);
                it.Serialize(connection);
                it.Commit(engine, false);

                ((SyncEngine)client).IgnoreClientMetadata = false;
            }
        }

        protected string[] CreateEntityKeys(Entity command)
        {
            switch (command.Type)
            {
                case SyncUtils.DELETE_ENTITY:
                case SyncUtils.CREATE_ENTITY:
                    return new string[] { String.Concat(command.GetString(SyncUtils.TYPE), SEPARATOR, command.GetString(SyncUtils.PARENTID)) };
                case SyncUtils.CREATE_ATTRIBUTE:
                case SyncUtils.DELETE_ATTRIBUTE:
                case SyncUtils.UPDATE_ATTRIBUTE:
                    return new string[] { String.Concat(command.GetString(SyncUtils.PARENTTYPE), SEPARATOR, command.GetString(SyncUtils.PARENTID)) };
                case SyncUtils.CREATE_REFERENCE:
                case SyncUtils.DELETE_REFERENCE:
                    return new string[] { 
                        String.Concat(command.GetString(SyncUtils.PARENTTYPE), SEPARATOR, command.GetString(SyncUtils.PARENTID)),
                        String.Concat(command.GetString(SyncUtils.CHILDTYPE), SEPARATOR, command.GetString(SyncUtils.CHILDID))
                    };
                default:
                    throw new UniversalStorageException("Unexpected command type");
            }
        }

        protected bool ApplyOnTheSameEntity(Entity c1, Entity c2)
        {
            string[] keys1 = CreateEntityKeys(c1);
            string[] keys2 = CreateEntityKeys(c2);

            foreach (string k1 in keys1)
                foreach (string k2 in keys2)
                    if (k1 == k2)
                        return true;

            return false;

        }
    }
}
