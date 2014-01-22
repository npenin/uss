using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Evaluant.Uss.Data.FileClient
{
    public class FileTransaction : IDbTransaction
    {
        FileConnection connection;

        List<FileCommand> commands = new List<FileCommand>();

        public List<FileCommand> Commands
        {
            get { return commands; }
        }

        public FileTransaction(FileConnection connection)
        {
            this.connection = connection;
        }

        #region IDbTransaction Members

        public void Commit()
        {
            foreach (FileCommand fc in commands)
            {
                connection.ExecuteCommand(fc);
            }
        }

        public IDbConnection Connection
        {
            get { return connection; }
        }

        public IsolationLevel IsolationLevel
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void Rollback()
        {
            commands.Clear();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            commands.Clear();
        }

        #endregion
    }
}
