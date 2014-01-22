using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public enum JoinType
    {
        Inner,
        Cross,
        Right,
        Left
    }

    public class JoinedTableExpression : TableExpression
    {
        public IAliasedExpression LeftTable { get; set; }

        public IAliasedExpression RightTable { get; set; }

        public JoinType JoinType { get; set; }

        public Evaluant.NLinq.Expressions.BinaryExpression On { get; set; }

        public JoinedTableExpression(IAliasedExpression left, IAliasedExpression right)
            : this(left, right, JoinType.Inner)
        {
        }

        public JoinedTableExpression(IAliasedExpression left, IAliasedExpression right, Evaluant.NLinq.Expressions.BinaryExpression on)
            : this(left, right, JoinType.Inner, on)
        {
        }

        public JoinedTableExpression(IAliasedExpression left, IAliasedExpression right, JoinType joinType)
            : this(left, right, joinType, right.Alias)
        {
        }


        public JoinedTableExpression(IAliasedExpression left, IAliasedExpression right, JoinType joinType, Evaluant.NLinq.Expressions.BinaryExpression on)
            : this(left, right, joinType, right.Alias, on)
        {
        }

        public JoinedTableExpression(IAliasedExpression left, IAliasedExpression right, JoinType joinType, TableAlias alias)
            : base(alias)
        {
            LeftTable = left;
            RightTable = right;
            JoinType = joinType;
        }

        public JoinedTableExpression(IAliasedExpression left, IAliasedExpression right, JoinType joinType, TableAlias alias, Evaluant.NLinq.Expressions.BinaryExpression on)
            : base(alias)
        {
            LeftTable = left;
            RightTable = right;
            JoinType = joinType;
            On = on;
        }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Join; }
        }

        public JoinedTableExpression Replace(TableAlias aliasToFind, IAliasedExpression expressionReplacement)
        {
            JoinedTableExpression result;
            if (LeftTable.Alias == aliasToFind)
            {
                LeftTable = expressionReplacement;
                return (JoinedTableExpression)new Visitors.LazyAliasResolver(new Dictionary<TableAlias, TableAlias>() { { aliasToFind, expressionReplacement.Alias } }).Visit(this);
            }
            if (RightTable.Alias == aliasToFind)
            {
                RightTable = expressionReplacement;
                return (JoinedTableExpression)new Visitors.LazyAliasResolver(new Dictionary<TableAlias, TableAlias>() { { aliasToFind, expressionReplacement.Alias } }).Visit(this);
            }
            if (LeftTable is JoinedTableExpression && (result = ((JoinedTableExpression)LeftTable).Replace(aliasToFind, expressionReplacement)) != null)
            {
                LeftTable = result;
                //LeftTable = (JoinedTableExpression)new Visitors.LazyAliasResolver(new Dictionary<TableAlias, TableAlias>() { { aliasToFind, expressionReplacement.Alias } }).Visit(this);
                return this;
            }
            if (RightTable is JoinedTableExpression && (result = ((JoinedTableExpression)RightTable).Replace(aliasToFind, expressionReplacement)) != null)
            {
                RightTable = result;
                //RightTable = new Visitors.LazyAliasResolver(new Dictionary<TableAlias, TableAlias>() { { aliasToFind, expressionReplacement.Alias } }).Visit(this);
                return this;
            }
            return null;
        }
    }
}
