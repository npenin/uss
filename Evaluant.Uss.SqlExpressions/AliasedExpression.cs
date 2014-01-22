using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public abstract class AliasedExpression : DbExpression, Evaluant.Uss.SqlExpressions.IAliasedExpression
    {
        TableAlias alias;
        protected AliasedExpression(TableAlias alias)
        {
            this.alias = alias;
        }



        public virtual TableAlias Alias
        {
            get { return this.alias; }
        }
    }
}
