using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

namespace Evaluant.Uss.SqlMapper.Drivers.File
{
    public class Connection : IDbConnection
    {
        private StreamWriter writer;


        #region IDbConnection Members

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return BeginTransaction();
        }

        public IDbTransaction BeginTransaction()
        {
            return new Transaction(writer);
        }

        public void ChangeDatabase(string databaseName)
        {
        }

        public void Close()
        {
            writer.Close();
        }

        public string ConnectionString
        {
            get;
            set;
        }

        public int ConnectionTimeout
        {
            get { throw new NotImplementedException(); }
        }

        public IDbCommand CreateCommand()
        {
            return new Command(writer);
        }

        public string Database
        {
            get { throw new NotImplementedException(); }
        }

        public void Open()
        {
            writer = System.IO.File.CreateText(ConnectionString);
        }

        public ConnectionState State
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        public Connection(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
    }
}
