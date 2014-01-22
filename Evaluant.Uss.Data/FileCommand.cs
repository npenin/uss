using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Evaluant.Uss.Data.FileClient
{
    public class FileCommand : System.Data.IDbCommand
    {
        public FileCommand()
        {
        }

        public FileCommand(FileConnection connection)
            : this(connection, String.Empty)
        {
        }

        public FileCommand(FileConnection connection, string commandText)
        {
            this.connection = connection;
            this.commandText = commandText;
        }

        #region IDbCommand Members

        public void Cancel()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        string commandText;

        public string CommandText
        {
            get
            {
                return commandText;
            }
            set
            {
                commandText = value;
            }
        }

        int commandTimeout;

        public int CommandTimeout
        {
            get
            {
                return commandTimeout;
            }
            set
            {
                commandTimeout = value;
            }
        }

        CommandType commandType;

        public System.Data.CommandType CommandType
        {
            get
            {
                return commandType;
            }
            set
            {
                commandType = value;
            }
        }

        FileConnection connection;

        public System.Data.IDbConnection Connection
        {
            get
            {
                return connection;
            }
            set
            {
                connection = value as FileConnection;
            }
        }

        public System.Data.IDbDataParameter CreateParameter()
        {
            return new FileParameter();
        }

        public int ExecuteNonQuery()
        {
            if (transaction != null)
            {
                transaction.Commands.Add(this);
            }
            else
            {
                connection.ExecuteCommand(this);
            }

            return 0;
        }

        public System.Data.IDataReader ExecuteReader(System.Data.CommandBehavior behavior)
        {
            return ExecuteReader();
        }

        public System.Data.IDataReader ExecuteReader()
        {
            return new FileDataReader();
        }

        public object ExecuteScalar()
        {
            return DBNull.Value;
        }

        FileParameterCollection parameters = new FileParameterCollection();

        public System.Data.IDataParameterCollection Parameters
        {
            get { return parameters; }
        }

        public void Prepare()
        {
        }

        FileTransaction transaction;

        public System.Data.IDbTransaction Transaction
        {
            get
            {
                return transaction;
            }
            set
            {
                transaction = value as FileTransaction;
            }
        }

        UpdateRowSource updatedRowSource;

        public System.Data.UpdateRowSource UpdatedRowSource
        {
            get
            {
                return updatedRowSource;
            }
            set
            {
                updatedRowSource = value;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
