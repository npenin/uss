using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Mapping
{
    public class EntitySourceExpression : TableExpression
    {
        public EntitySourceExpression(TableAlias alias, string type)
            : base(alias)
        {
            EntityType = type;
        }

        public override DbExpressionType DbExpressionType
        {
            get
            {
                return DbExpressionType.Entity;
            }
        }

        public string EntityType { get; set; }
    }
}
