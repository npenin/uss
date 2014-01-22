using System;
using System.Text;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.Sync
{
    public class ConflictEventArgs : EventArgs
    {
        public ConflictEventArgs(Command client, Command server, SyncConflict conflict)
        {
            this.client = client;
            this.server = server;
            this.conflict = conflict;
        }

        private Command client;

        public Command Client
        {
            get { return client; }
            set { client = value; }
        }

        private Command server;

        public Command Server
        {
            get { return server; }
            set { server = value; }
        }

        private SyncConflict conflict;

        public SyncConflict Conflict
        {
            get { return conflict; }
            set { conflict = value; }
        }

        private ConflictResolution resolution = ConflictResolution.ClientWins;

        public ConflictResolution Resolution
        {
            get { return resolution; }
            set { resolution = value; }
        }

    }
}
