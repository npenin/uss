using System;
namespace Evaluant.Uss.SqlExpressions
{
    public interface IAliasedExpression : IDbExpression
    {
        TableAlias Alias { get; }
    }
}
