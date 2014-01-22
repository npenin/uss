using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Evaluant.Uss.SqlMapper.Drivers.File
{
    public class Transaction : IDbTransaction
    {
        private System.IO.StreamWriter writer;

        public Transaction(System.IO.StreamWriter writer)
        {
            this.writer = writer;
        }
        #region IDbTransaction Members

        public void Commit()
        {
            writer.Flush();
        }

        public IDbConnection Connection
        {
            get;
            set;
        }

        public IsolationLevel IsolationLevel
        {
            get;
            set;
        }

        public void Rollback()
        {
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
