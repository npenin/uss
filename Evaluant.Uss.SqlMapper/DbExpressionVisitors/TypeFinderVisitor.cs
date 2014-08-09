using Evaluant.Uss.SqlExpressions.Visitors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors
{
    class TypeFinderVisitor : DbExpressionVisitor
    {
        private Model.Model model;
        private Mapping.Mapping mapping;

        public string Type { get; set; }
        public DbType DbType { get; set; }

        public TypeFinderVisitor(Model.Model model, Mapping.Mapping mapping)
        {
            this.model = model;
            this.mapping = mapping;
        }

        public override NLinq.Expressions.Identifier Visit(NLinqImprovements.EntityIdentifier identifier)
        {
            Type = model[identifier.Entity.Type].Type;
            return base.Visit(identifier);
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression expression)
        {
            Visit(expression.Previous);
            if (expression.Statement.ExpressionType == NLinq.Expressions.ExpressionTypes.Identifier)
            {
                string propertyName = ((NLinq.Expressions.Identifier)expression.Statement).Text;
                Model.Attribute attribute = model.GetAttribute(Type, propertyName, false);
                Model.Reference reference = model.GetReference(Type, propertyName, false);
                if (attribute != null)
                {
                    DbType = mapping[Type].Attributes[propertyName].DbType;
                    Type = attribute.TypeName;
                }
                else if (reference != null)
                    Type = reference.ChildType;
            }
            return base.Visit(expression);
        }
    }
}
