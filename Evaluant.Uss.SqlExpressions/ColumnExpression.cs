using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class ColumnExpression : AliasedExpression
    {
        public static readonly Identifier AllColumns = new Identifier("*");

        public ColumnExpression(TableAlias alias) : base(alias) { }
        public ColumnExpression(TableAlias alias, Identifier columnName)
            : this(alias, columnName, null)
        {
        }

        public ColumnExpression(TableAlias alias, Identifier columnName, Identifier columnAlias)
            : base(alias)
        {
            ColumnName = columnName;
            ColumnAlias = columnAlias;
        }

        public ColumnExpression(TableAlias alias, string columnName)
            : this(alias, new Identifier(columnName), null)
        {
        }

        public ColumnExpression(TableAlias alias, string columnName, string columnAlias)
            : base(alias)
        {
            if (columnName == AllColumns.Text)
                ColumnName = AllColumns;
            else
            {
                ColumnName = new Identifier(columnName);
                if (!string.IsNullOrEmpty(columnAlias))
                    ColumnAlias = new Identifier(columnAlias);
            }
        }

        public Identifier ColumnName { get; set; }
        public Identifier ColumnAlias { get; set; }


        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Column; }
        }
    }
}
