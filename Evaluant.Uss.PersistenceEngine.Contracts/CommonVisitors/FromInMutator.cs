using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.CommonVisitors;
using Evaluant.Uss.NLinqVisitors;

namespace Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors
{
    public class FromInMutator : NLinqExpressionVisitor<NLinqExpressionUpdater>
    {
        private NLinq.Expressions.Identifier defaultInName;

        public FromInMutator()
            : this("#context")
        {

        }

        bool firstFrom = true;
        string firstInNameUsed;

        public FromInMutator(string defaultInName)
            : base(new NLinqExpressionUpdater())
        {
            this.defaultInName = new Evaluant.NLinq.Expressions.Identifier(defaultInName);
        }

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.FromClause expression)
        {
            if (firstFrom)
            {
                Evaluant.NLinq.Expressions.Expression exp = Visit(expression.Expression);
                firstFrom = false;
                return updater.Update(expression, expression.Identifier, exp ?? defaultInName, expression.Type);
            }
            return updater.Update(expression, expression.Identifier, Visit(expression.Expression), expression.Type);
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.Identifier identifier)
        {
            if (firstFrom)
            {
                firstInNameUsed = identifier.Text;
                return defaultInName;
            }
            if (identifier.Text == firstInNameUsed)
                return defaultInName;
            return identifier;
        }
    }
}
