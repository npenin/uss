using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Visitors
{
    public class LazyAliasResolver : DbExpressionVisitor
    {
        Dictionary<TableAlias, TableAlias> aliasesMapping;

        public LazyAliasResolver(Dictionary<TableAlias, TableAlias> aliases)
        {
            aliasesMapping = aliases;
        }

        public LazyAliasResolver(TableAlias oldAlias, TableAlias newAlias)
            : this(new Dictionary<TableAlias, TableAlias>() { { oldAlias, newAlias } })
        {

        }

        public IEnumerable<IAliasedExpression> VisitColumns(IEnumerable<IAliasedExpression> columns)
        {
            return VisitEnumerable(columns, Visit);
        }

        public override IAliasedExpression Visit(JoinedTableExpression item)
        {
            return base.Visit(item);
        }

        public override TableAlias Visit(TableAlias item)
        {
            if (item == null)
                return null;
            TableAlias result;
            if (!aliasesMapping.TryGetValue(item, out result))
                if (!aliasesMapping.TryGetValue(TableAlias.All, out result))
                    return item;
            return result;
        }
    }
}
