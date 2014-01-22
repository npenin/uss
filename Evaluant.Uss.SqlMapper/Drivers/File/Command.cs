using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Evaluant.Uss.SqlMapper.Drivers.File
{
    public class Command : IDbCommand
    {
        private System.IO.StreamWriter writer;

        public Command(System.IO.StreamWriter writer)
        {
            this.writer = writer;
        }
        #region IDbCommand Members

        public void Cancel()
        {
        }

        public string CommandText
        {
            get;
            set;
        }

        public int CommandTimeout
        {
            get;
            set;
        }

        public CommandType CommandType
        {
            get;
            set;
        }

        public IDbConnection Connection
        {
            get;
            set;
        }

        public IDbDataParameter CreateParameter()
        {
            throw new NotSupportedException();
        }

        public int ExecuteNonQuery()
        {
            writer.WriteLine(CommandText);
            return 1;
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader()
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public IDataParameterCollection Parameters
        {
            get { throw new NotImplementedException(); }
        }

        public void Prepare()
        {
        }

        public IDbTransaction Transaction
        {
            get;
            set;
        }

        public UpdateRowSource UpdatedRowSource
        {
            get;
            set;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            writer = null;
        }

        #endregion
    }
}
