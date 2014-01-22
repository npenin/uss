using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class DeleteStatement : DbStatement
    {
        public DeleteStatement(FromClause from, WhereClause where)
        {
            From = from;
            Where = where;
        }

        public FromClause From { get; set; }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Delete; }
        }

        public override StatementType StatementType
        {
            get { return StatementType.Delete; }
        }

        public WhereClause Where { get; set; }
    }
}
