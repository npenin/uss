using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class UpdateStatement : DbStatement
    {
        public UpdateStatement()
        {
            Set = new Dictionary<ColumnExpression, DbParameter>();
        }

        public override System.Data.StatementType StatementType
        {
            get { return System.Data.StatementType.Update; }
        }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Update; }
        }

        public FromClause From { get; set; }

        public WhereClause Where { get; set; }

        public IDictionary<ColumnExpression, DbParameter> Set { get; set; }
    }
}
