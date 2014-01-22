using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Analyzers
{
    public class ReferenceAnalyzer : DbExpressionVisitor
    {
        public ReferenceAnalyzer(Model.Model model)
        {
            this.model = model;
        }

        private Model.Model model;
        private Model.Entity currentEntity;

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression expression)
        {
            if (expression.Previous != null)
                Visit(expression.Previous);
            if (currentEntity != null)
            {
                reference = ((Model.Model)currentEntity.Model).GetReference(currentEntity.Type, ((Identifier)expression.Statement).Text);
                if (reference == null)
                    currentEntity = null;
                else
                    currentEntity = Reference.Target;
            }
            return expression;
        }

        public override SqlExpressions.IDbExpression Visit(SqlExpressions.EntityExpression item)
        {
            currentEntity = model.Entities[item.Type];
            return item;
        }

        private Model.Reference reference;

        public Model.Reference Reference
        {
            get { return reference; }
            set { reference = value; }
        }

    }
}
