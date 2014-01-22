using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlMapper.DbExpressionVisitors;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;


namespace Evaluant.Uss.SqlMapper.Dialects
{
    public class SqlServer : Dialect
    {
        public SqlServer()
            : base("[", "]")
        {

        }

        #region IDialect Members

        public override Evaluant.Uss.SqlExpressions.IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.TableSourceExpression item)
        {
            if (string.IsNullOrEmpty(item.Table.Schema))
                item.Table.Schema = "dbo";
            return base.Visit(item);
        }

        #endregion

        public override SqlExpressions.IDbExpression FindFK(string schema, string fk, SqlExpressions.Mapping.Table table)
        {
            if (string.IsNullOrEmpty(table.Schema))
                return new SqlExpressions.HardCodedExpression(
                @"SELECT 1 
                  FROM sys.foreign_keys 
                  WHERE object_id = OBJECT_ID(N'[" + schema + "].[" + fk + @"]') 
                  AND parent_object_id=OBJECT_ID(N'" + table.TableName + "')");

            return new SqlExpressions.HardCodedExpression(
            @"SELECT 1 
                  FROM sys.foreign_keys 
                  WHERE object_id = OBJECT_ID(N'[" + schema + "].[" + fk + @"]') 
                  AND parent_object_id=OBJECT_ID(N'[" + table.Schema + "].[" + table.TableName + "]')");
        }

        public override SqlExpressions.IAliasedExpression Visit(SqlExpressions.ComplexColumnExpression item)
        {
            writer.Write(' ');
            if (item.ColumnAlias != null)
            {
                Visit(item.ColumnAlias);
                writer.Write(" = ");
            }
            if (item.Alias != null)
            {
                Visit(item.Alias);
                writer.Write(".");
            }
            Visit(item.Expression);
            return item;
        }

        public override SqlExpressions.IDbExpression SchemaExists(string schema)
        {
            return new SqlExpressions.HardCodedExpression("SCHEMA_ID('" + schema + "') is null");
        }

        public override NLinq.Expressions.Expression FindTable(SqlExpressions.Mapping.Table table)
        {
            if (string.IsNullOrEmpty(table.Schema))
                return new SqlExpressions.HardCodedExpression(
                    @"SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'" + table.TableName + "')");

            return new SqlExpressions.HardCodedExpression(
                @"SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[" + table.Schema + "].[" + table.TableName + "]')");

        }
    }
}
