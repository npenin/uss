using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.Dialects
{
    public abstract class Dialect : DbExpressionWriter, IDialect
    {
        public Dialect(string beginEscape, string endEscape)
            : base(beginEscape, endEscape)
        {

        }

        Evaluant.Uss.SqlExpressions.DbExpressionType type;

        public override string Render(SqlExpressions.IDbExpression expression)
        {
            type = Evaluant.Uss.SqlExpressions.DbExpressionType.Unknown;
            TraceHelper.TraceEvent(SqlMapperProvider.TraceSource, System.Diagnostics.TraceEventType.Start, 1, "Rendering");
            string sql = base.Render(expression);
            TraceHelper.TraceEvent(SqlMapperProvider.TraceSource, System.Diagnostics.TraceEventType.Stop, 1, "Rendering");
            TraceHelper.TraceData(SqlMapperProvider.TraceSource, System.Diagnostics.TraceEventType.Information, (int)type, sql);
#if TRACE
            SqlExpressions.IDbStatement statement = expression as SqlExpressions.IDbStatement;
            if (statement != null)
            {
                foreach (SqlExpressions.ValuedParameter parameter in statement.Parameters.Values)
                    TraceHelper.TraceData(SqlMapperProvider.TraceSource, System.Diagnostics.TraceEventType.Information, (int)type, parameter.Name + "=" + parameter.Value);
            }
#endif
            return sql;
        }

        public override string Render(Evaluant.NLinq.Expressions.Expression expression)
        {
            type = SqlExpressions.DbExpressionType.Unknown;
            TraceHelper.TraceEvent(SqlMapperProvider.TraceSource, System.Diagnostics.TraceEventType.Start, 1, "Rendering");
            string sql = base.Render(expression);
            TraceHelper.TraceEvent(SqlMapperProvider.TraceSource, System.Diagnostics.TraceEventType.Stop, 1, "Rendering");
            TraceHelper.TraceData(SqlMapperProvider.TraceSource, System.Diagnostics.TraceEventType.Information, (int)type, sql);
            return sql;
        }

        protected override SqlExpressions.IDbExpression Visit(Evaluant.Uss.SqlExpressions.IDbExpression exp)
        {
            if (type == SqlExpressions.DbExpressionType.Unknown)
                type = exp.DbExpressionType;
            return base.Visit(exp);
        }

        public abstract IDbExpression FindFK(string schema, string fk, SqlExpressions.Mapping.Table table);


        public abstract IDbExpression SchemaExists(string schema);


        public abstract NLinq.Expressions.Expression FindTable(SqlExpressions.Mapping.Table table);
    }
}
