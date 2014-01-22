using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Evaluant.Uss.ObjectContext.Contracts;
using System.Reflection;

namespace Evaluant.Uss.Linq
{
    public class QueryableUss<T> : AsyncQueryableUss<T>, IOrderedQueryable<T>
    {
        IObjectContext context;
        public QueryableUss(QueryableUss<T> query, Expression expression)
            : this(query.context, expression, query.provider)
        {

        }

        public QueryableUss(IObjectContext context)
            : this(context, null, new QueryProvider(context))
        {
        }

        public QueryableUss(IObjectContext context, Expression expression, QueryProvider provider)
            : base(null, expression, provider)
        {
            this.context = context;
        }

        public List<T> ToList()
        {
            return Provider.Execute<List<T>>(Expression);
        }

        #region IEnumerable<T> Members

        public new IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ILinqableContext<T> Members

        public IQueryable<T> Cast()
        {
            return this;
        }

        #endregion

        #region IInferrable Members

        public override IInferrable<T> Infer(string reference)
        {
            var parameter = Expression.Parameter(typeof(T), "item");
            var property = Expression.Property(parameter, reference);
            return new QueryableUss<T>(this, Expression.Call(InferMethod.GetGenericMethodDefinition().MakeGenericMethod(typeof(T), property.Type), Expression, Expression.Quote(Expression.Lambda(property, parameter))));
        }

        #endregion
    }
}
