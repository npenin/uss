using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public interface IDbExpression
    {
        DbExpressionType DbExpressionType { get; }
    }
}
