using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using System.Collections;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Memory
{
    public class TypeFilterVisitor : NLinqVisitors.NLinqExpressionVisitor
    {
        public TypeFilterVisitor(Model.Model model, IEnumerable entities, string defaultSource)
        {
            this.model = model;
            Entities = new List<Entity>();
            foreach (Entity entity in entities)
                Entities.Add(entity);
            this.defaultSource = defaultSource;
        }

        private string defaultSource;
        private Model.Model model;
        public List<Entity> Entities { get; set; }

        public override QueryBodyClause Visit(FromClause expression)
        {
            if (expression.Expression == null || (expression.Expression is Identifier && ((Identifier)expression.Expression).Text == defaultSource))
            {
                for (int i = Entities.Count - 1; i >= 0; i--)
                {
                    if (!model.Inherits(Entities[i].Type, expression.Type))
                        Entities.RemoveAt(i);
                }
            }
            return base.Visit(expression);
        }
    }
}
