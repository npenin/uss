using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.NLinqImprovements
{
    public class EntityIdentifier : Evaluant.NLinq.Expressions.Identifier
    {

        public EntityIdentifier(string identifier, EntityExpression entity)
            : this(new Evaluant.NLinq.Expressions.Identifier(identifier), entity)
        {
        }

        public EntityIdentifier(Evaluant.NLinq.Expressions.Identifier identifier, EntityExpression entity)
            : base(null)
        {
            Identifier = identifier;
            Entity = entity;
        }

        public EntityExpression Entity { get; set; }

        public Evaluant.NLinq.Expressions.Identifier Identifier { get; set; }

        public override string Text
        {
            get
            {
                return Identifier.Text;
            }
            set
            {
                Identifier.Text = value;
            }
        }
    }
}
