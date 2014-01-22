using System;
using System.Linq;
using Evaluant.Uss.ObjectContext.Contracts;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace Evaluant.Uss.Linq
{
    public class AsyncQueryableUss<T> : IInferrable<T>, IOrderedQueryable<T>
    {
        IObjectContextAsync context;
        protected QueryProvider provider;
        protected MethodInfo InferMethod;

        public AsyncQueryableUss(IObjectContextAsync context)
            : this(context, null, new QueryProvider(context))
        {
        }

        public AsyncQueryableUss(AsyncQueryableUss<T> query, Expression expression)
            : this(query.context, expression, query.provider)
        {

        }

        public AsyncQueryableUss(IObjectContextAsync context, Expression expression, QueryProvider provider)
        {
            this.context = context;
            if (expression != null)
                Expression = expression;
            else
                Expression = Expression.Constant(this);
            this.provider = provider;
            InferMethod = new Func<string, IInferrable<T>>(Infer).Method;
        }

        public IInferrable<T> Infer<U>(Expression<Func<T, U>> accessor)
        {
            return new AsyncQueryableUss<T>(context, Expression.Call(Expression, "Infer", new Type[] { typeof(Func<T, U>) }, Expression.Constant(accessor)), provider);
        }

        public void ToList(Action<List<T>> callback)
        {
            provider.BeginExecute<List<T>, T>(callback, Expression);
        }

        public void Count(Action<int> callback)
        {
            provider.BeginExecute<int>(callback, new AsyncQueryableUss<T>(context, Expression.Call(null, typeof(Queryable).GetMethods().Where(m => m.Name == "Count" && m.GetParameters().Length == 1).First().MakeGenericMethod(typeof(T)), Expression), provider).Expression);
        }

        public void First(Action<T> callback)
        {
            provider.BeginExecute<T>(callback, new AsyncQueryableUss<T>(context, Expression.Call(null, typeof(Queryable).GetMethods().Where(m => m.Name == "First" && m.GetParameters().Length == 1).First().MakeGenericMethod(typeof(T)), Expression), provider).Expression);
        }

        public void FirstOrDefault(Action<T> callback)
        {
            provider.BeginExecute<T>(callback, new AsyncQueryableUss<T>(context, Expression.Call(null, typeof(Queryable).GetMethods().Where(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 1).First().MakeGenericMethod(typeof(T)), Expression), provider).Expression);
        }



        public void ToList(AsyncCallback callback)
        {
            provider.BeginExecute<List<T>, T>(callback, Expression);
        }

        public void Count(AsyncCallback callback)
        {
            provider.BeginExecute<int>(callback, new AsyncQueryableUss<T>(context, Expression.Call(null, typeof(Queryable).GetMethods().Where(m => m.Name == "Count" && m.GetParameters().Length == 1).First().MakeGenericMethod(typeof(T)), Expression), provider).Expression);
        }

        public void First(AsyncCallback callback)
        {
            provider.BeginExecute<T>(callback, new AsyncQueryableUss<T>(context, Expression.Call(null, typeof(Queryable).GetMethods().Where(m => m.Name == "First" && m.GetParameters().Length == 1).First().MakeGenericMethod(typeof(T)), Expression), provider).Expression);
        }

        public void FirstOrDefault(AsyncCallback callback)
        {
            provider.BeginExecute<T>(callback, new AsyncQueryableUss<T>(context, Expression.Call(null, typeof(Queryable).GetMethods().Where(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 1).First().MakeGenericMethod(typeof(T)), Expression), provider).Expression);
        }



        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotSupportedException("The current operation cannot be done because it has to be done asynchronously");
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IQueryable Members

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public System.Linq.Expressions.Expression Expression { get; set; }

        public IQueryProvider Provider
        {
            get { return provider; }
        }

        #endregion

        #region IInferrable Members

        public virtual IInferrable<T> Infer(string reference)
        {
            var parameter = Expression.Parameter(typeof(T), "item");
            var property = Expression.Property(parameter, reference);
            return new AsyncQueryableUss<T>(this, Expression.Call(InferMethod.GetGenericMethodDefinition().MakeGenericMethod(typeof(T), property.Type), Expression, Expression.Quote(Expression.Lambda(property, parameter))));
        }


        #endregion
    }
}
