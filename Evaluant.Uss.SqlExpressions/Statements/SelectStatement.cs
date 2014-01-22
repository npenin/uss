using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class SelectStatement : AliasedExpression, IDbStatement
    {
        public SelectStatement(TableAlias alias)
            : base(alias)
        {
            Parameters = new Dictionary<string, DbParameter>();
        }
        public SelectStatement(TableAlias alias, IEnumerable<IAliasedExpression> columns, FromClause from, OrderByClause orderby, WhereClause where)
            : this(alias)
        {
            Columns = columns;
            From = from;
            OrderBy = orderby;
            Where = where;
        }

        public int Top { get; set; }

        public bool Distinct { get; set; }

        public IEnumerable<IAliasedExpression> Columns { get; set; }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Select; }
        }

        #region IDbStatement Members

        public StatementType StatementType
        {
            get { return System.Data.StatementType.Select; }
        }

        #endregion

        public WhereClause Where { get; set; }

        public FromClause From { get; set; }

        public OrderByClause OrderBy { get; set; }


        #region IDbStatement Members


        public IDictionary<string, DbParameter> Parameters
        {
            get;
            set;
        }

        public void Add(DbParameter parameter)
        {
            Parameters.Add(parameter.Name, parameter);
        }

        #endregion
    }
}
