using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlMapper.Dialects
{
    public class DialectProxy : IDialect
    {
        public DialectProxy(IDialect dialect)
        {
            Dialect = dialect;
        }

        public IDialect Dialect { get; private set; }

        #region IDialect Members

        public virtual string Render(Evaluant.NLinq.Expressions.Expression expression)
        {
            return Dialect.Render(expression);
        }

        public string Render(SqlExpressions.IDbExpression expression)
        {
            return Dialect.Render(expression);
        }

        #endregion


        public SqlExpressions.IDbExpression FindFK(string schema, string fk, SqlExpressions.Mapping.Table table)
        {
            return Dialect.FindFK(schema, fk, table);
        }


        public SqlExpressions.IDbExpression SchemaExists(string schema)
        {
            return Dialect.SchemaExists(schema);
        }


        public NLinq.Expressions.Expression FindTable(SqlExpressions.Mapping.Table table)
        {
            return Dialect.FindTable(table);
        }
    }
}
