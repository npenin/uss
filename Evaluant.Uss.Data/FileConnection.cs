using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using Evaluant.Uss.SqlMapper;

namespace Evaluant.Uss.Data.FileClient
{
    public class FileConnection : IDbConnection
    {
        protected StreamWriter sw;

        internal StreamWriter StreamWriter
        {
            get { return sw; }
        }

        protected IDialect dialect;

        public IDialect Dialect
        {
            get { return dialect; }
            set { dialect = value; }
        }

        /// <summary>
        /// Transforms to SQL by replacing parameters if any.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public string TransformToSql(FileCommand command)
        {
            string query = command.CommandText;

            foreach (FileParameter param in command.Parameters)
            {
                SqlExpressions.Constant c = new SqlExpressions.Constant(param.Value, param.DbType);
                query = query.Replace(param.ParameterName, dialect.Render((SqlExpressions.IDbExpression)c));
            }

            return query;
        }

        internal virtual void ExecuteCommand(FileCommand command)
        {
            if (state == ConnectionState.Closed || sw == null)
                Open();

            if (!string.IsNullOrEmpty(command.CommandText))
                sw.WriteLine(TransformToSql(command));

            sw.Flush();
        }

        #region IDbConnection Members

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return BeginTransaction();
        }

        FileTransaction currentTransaction;

        public IDbTransaction BeginTransaction()
        {
            state = ConnectionState.Executing;
            return currentTransaction = new FileTransaction(this);
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual void Close()
        {
            state = ConnectionState.Closed;

            if (sw != null)
            {
                sw.Flush();
                sw.Close();
                sw.Dispose();
                sw = null;
            }
        }

        protected string filename;
        private string connectionString;

        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
            set
            {
                connectionString = value;
                ParseConnectionString();
            }
        }

        protected virtual void ParseConnectionString()
        {
            // Seek "filename"
            int index = connectionString.IndexOf("filename");

            // Seek next "="
            index = connectionString.IndexOf("=", index) + 1;

            // Seek end of filename
            int end = connectionString.IndexOf(";", index);
            if (end == -1)
            {
                end = connectionString.Length - 1;
            }

            filename = connectionString.Substring(index, end - index + 1);
        }

        public int ConnectionTimeout
        {
            get { return System.Threading.Timeout.Infinite; }
        }

        public IDbCommand CreateCommand()
        {
            FileCommand command = new FileCommand(this);
            command.Transaction = currentTransaction;

            return command;
        }

        public string Database
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public virtual void Open()
        {
            state = ConnectionState.Open;
            sw = new StreamWriter(filename, true);
        }

        protected ConnectionState state = ConnectionState.Closed;

        public ConnectionState State
        {
            get { return state; }

        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (state != ConnectionState.Closed)
            {
                Close();
            }
        }

        #endregion
    }
}
