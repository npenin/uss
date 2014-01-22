using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.EF
{
    public class EFDialect : DbExpressionWriter, IDialect
    {
        //#region IDialect Members

        //public override string Render(NLinq.Expressions.Expression expression)
        //{
        //    throw new NotImplementedException();
        //}

        //#endregion

        public override Evaluant.Uss.SqlExpressions.IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.SelectStatement item)
        {
            bool wasInWhere = inWhere;
            inWhere = false;
            bool innerSelect = writer.GetStringBuilder().Length > 0;
            if (innerSelect)
                writer.Write("( ");
            writer.Write("SELECT ");
            if (item.Distinct)
                writer.Write(" DISTINCT ");
            if (item.Top > 0)
            {
                writer.Write(" TOP ");
                writer.Write(item.Top);
            }
            if (item.Columns != null)
            {
                VisitColumns(item.Columns);
            }
            if (item.From != null)
                Visit(item.From);
            if (item.Where != null)
                Visit(item.Where);
            if (item.OrderBy != null)
                Visit(item.OrderBy);
            inWhere = wasInWhere;
            if (innerSelect)
            {
                writer.Write(" )");
                if (!inWhere)
                {
                    //writer.Write(" as ");
                    Visit(item.Alias);
                }
            }
            return item;
        }

        public override void VisitColumns(IEnumerable<IAliasedExpression> columns)
        {

            if (columns.Count() == 2)
            {
                ComplexColumnExpression firstColumn = columns.First() as ComplexColumnExpression;
                if (firstColumn != null && firstColumn.ColumnAlias.Text == "EntityType")
                {
                    writer.Write(" VALUE ");
                    Visit(columns.Last().Alias);
                }
            }
            base.VisitColumns(columns);
        }



        public IDbExpression FindFK(string schema, string fk, SqlExpressions.Mapping.Table table)
        {
            throw new NotSupportedException();
        }

        public IDbExpression SchemaExists(string schema)
        {
            throw new NotSupportedException();
        }

        public NLinq.Expressions.Expression FindTable(SqlExpressions.Mapping.Table table)
        {
            throw new NotSupportedException();
        }
    }
}
