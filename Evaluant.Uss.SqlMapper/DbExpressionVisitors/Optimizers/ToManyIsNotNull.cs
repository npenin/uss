using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.Model;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Era;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Optimizers
{
    class ToManyIsNotNull : DbExpressionVisitor
    {
        IPersistenceEngine engine;

        public ToManyIsNotNull(IPersistenceEngine engine)
        {
            this.engine = engine;
        }

        Entity currentEntity;
        Reference currentReference;

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.BinaryExpression item)
        {
            NLinq.Expressions.Expression left = Visit(item.LeftExpression);
            NLinq.Expressions.Expression right = Visit(item.RightExpression);
            if (left == null)
                return right;
            if (right == null)
                return left;
            Constant constant = left as Constant;
            if (constant != null && right is NLinq.Expressions.MemberExpression)
            {
                if (currentReference != null && currentReference.Cardinality is Cardinality.ToMany && constant.Value == null)
                    return null;
            }
            constant = right as Constant;
            if (constant != null && left is NLinq.Expressions.MemberExpression)
            {
                if (currentReference != null && currentReference.Cardinality is Cardinality.ToMany && constant.Value == null)
                    return null;
            }
            return item;
        }

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.MemberExpression expression)
        {
            if (expression.Previous == null)
                return base.Visit(expression);
            NLinq.Expressions.Expression previous = Visit(expression.Previous);
            Evaluant.NLinq.Expressions.Identifier propertyName = expression.Statement as Evaluant.NLinq.Expressions.Identifier;
            if (propertyName != null)
            {
                if (currentEntity.References.ContainsKey(propertyName.Text))
                {
                    currentReference = currentEntity.References[propertyName.Text];
                    currentEntity = engine.Factory.Model.Entities[currentReference.ChildType];
                }
                else
                {
                    currentReference = null;
                }
            }
            else
            {
                currentEntity = null;
                currentReference = null;
            }
            return updater.Update(expression, previous, Visit(expression.Statement));
        }

        public override IDbExpression Visit(EntityExpression item)
        {
            if (item.Type != null)
            {
                if (engine.Factory.Model.Entities.TryGetValue(item.Type, out currentEntity))
                    currentReference = null;
            }
            return base.Visit(item);
        }
    }
}
