using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions.Functions
{
    public class Exec : Function, IDbStatement
    {
        public Exec(IDbStatement statement)
            : base("EXEC", (Expression)statement)
        {
        }
        public override FunctionType FunctionType
        {
            get { return SqlExpressions.FunctionType.Exec; }
        }

        public System.Data.StatementType StatementType
        {
            get { return System.Data.StatementType.Batch; }
        }

        IDictionary<string, DbParameter> IDbStatement.Parameters
        {
            get;
            set;
        }

        public void Add(DbParameter parameter)
        {
            ((IDbStatement)this).Parameters.Add(parameter.Name, parameter);
        }
    }
}
