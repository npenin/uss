using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Domain;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlMapper
{
    public interface IDialect
    {
        string Render(Expression expression);
        string Render(SqlExpressions.IDbExpression expression);

        SqlExpressions.IDbExpression FindFK(string schema, string fk, SqlExpressions.Mapping.Table table);

        SqlExpressions.IDbExpression SchemaExists(string schema);

        Expression FindTable(SqlExpressions.Mapping.Table table);
    }
}
