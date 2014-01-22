using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    class DiscriminatorMutator : DbExpressionVisitor
    {
        private SqlExpressions.Mapping.EntitySourceExpression ese;
        public DiscriminatorMutator(SqlExpressions.Mapping.EntitySourceExpression ese)
        {
            this.ese = ese;
        }

        public override Evaluant.NLinq.Expressions.Expression Visit(NLinq.Expressions.Identifier identifier)
        {
            return new Evaluant.NLinq.Expressions.MemberExpression(identifier, new Evaluant.Uss.SqlExpressions.EntityExpression(ese.Alias) { Type = ese.EntityType });
        }
    }
}
