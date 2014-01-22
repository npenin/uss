using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlMapper.Dialects
{
    public class Oracle : Dialect
    {
        public Oracle()
            : base("\"", "\"")
        {

        }

        public override SqlExpressions.IDbExpression FindFK(string schema, string fk, SqlExpressions.Mapping.Table table)
        {
            throw new NotImplementedException();
        }

        public override SqlExpressions.IDbExpression SchemaExists(string schema)
        {
            throw new NotImplementedException();
        }

        public override NLinq.Expressions.Expression FindTable(SqlExpressions.Mapping.Table table)
        {
            throw new NotImplementedException();
        }
    }
}
