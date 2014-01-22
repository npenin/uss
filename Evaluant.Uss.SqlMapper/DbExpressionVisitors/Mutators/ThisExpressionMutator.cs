using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class ThisExpressionMutator : DbExpressionVisitor
    {
        public TableAlias This { get { return self; } }

        private TableAlias self;
        private bool isInMember;
        private string thisType;

        public ThisExpressionMutator(string thisType)
            : this(thisType, new TableAlias())
        {

        }

        public ThisExpressionMutator(string thisType, TableAlias alias)
        {
            self = alias;
            isInMember = false;
            this.thisType = thisType;
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.Identifier identifier)
        {
            if (isInMember)
                return base.Visit(identifier);
            return new MemberExpression(identifier, new EntityExpression(self) { Type = thisType });
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression expression)
        {
            bool wasInMember = isInMember;
            isInMember = true;
            var result = updater.Update(expression, Visit(expression.Previous), expression.Statement);
            isInMember = wasInMember;
            return result;
        }
    }
}
