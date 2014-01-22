using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Evaluant.Uss.SqlExpressions
{
    public interface IDbStatement : IDbExpression
    {
        StatementType StatementType { get; }

        IDictionary<string, DbParameter> Parameters { get; set; }

        void Add(DbParameter parameter);
    }

    public abstract class DbStatement : DbExpression, IDbStatement
    {
        public DbStatement()
        {
            Parameters = new Dictionary<string,DbParameter>();
        }

        public void Add(DbParameter parameter)
        {
            Parameters.Add(parameter.Name, parameter);
        }

        #region IDbStatement Members

        public abstract StatementType StatementType { get; }

        #endregion

        public IDictionary<string, DbParameter> Parameters { get; set; }
    }
}
