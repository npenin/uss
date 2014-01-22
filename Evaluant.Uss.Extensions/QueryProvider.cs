using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evaluant.Uss.ObjectContext.Contracts;
using System.Reflection;
using Evaluant.Uss.MetaData;
using Evaluant.Uss.Extensions;
//using Evaluant.NLinq;

namespace Evaluant.Uss.Linq
{
    public class QueryProvider : IQueryProvider
    {
        private IObjectContext context;
        private IObjectContextAsync asyncContext;
        private static MethodInfo load = typeof(IObjectContextSyncBase).GetMethod("Load", new Type[] { typeof(Evaluant.NLinq.Expressions.Expression) });
        private static MethodInfo infer = typeof(IPersistenceEngineObjectContext).GetMethod("Infer");
        private static MethodInfo loadAsync = typeof(IObjectContextAsync).GetMethod("BeginLoad", new Type[] { typeof(AsyncCallback), typeof(Evaluant.NLinq.Expressions.Expression) });
        private static MethodInfo endLoadMany = typeof(QueryProvider).GetMethod("EndLoadMany", new Type[] { typeof(IAsyncResult) });
        private static MethodInfo endLoadSingle = typeof(QueryProvider).GetMethod("EndLoadSingle", new Type[] { typeof(IAsyncResult) });
        private static MethodInfo createQuery;

        static QueryProvider()
        {
            createQuery = typeof(QueryProvider).GetMethods().FirstOrDefault(m => m.Name == "CreateQuery" && m.IsGenericMethodDefinition);
        }

        public QueryProvider(IObjectContext context)
        {
            this.context = context;
        }

        public QueryProvider(IObjectContextAsync context)
        {
            this.asyncContext = context;
        }

        #region IQueryProvider Members

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            if (asyncContext != null)
                return new AsyncQueryableUss<TElement>(asyncContext, expression, this);
            return new QueryableUss<TElement>(context, expression, this);
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            Type iQueryable = expression.Type.GetInterfaces().FirstOrDefault(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IQueryable<>));
            if (iQueryable != null)
            {
                return (IQueryable)createQuery.MakeGenericMethod(iQueryable.GetGenericArguments()[0]).Invoke(this, new object[] { expression });
            }
            return null;
        }

        public void BeginExecute<TResult>(AsyncCallback callback, System.Linq.Expressions.Expression expression)
        {
            if (TypeResolver.IsPrimitive(expression.Type))
                asyncContext.BeginLoadSingle<TResult>(callback, new QueryTranslator(expression.Type.FullName).Transform(expression));
            if (expression.Type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(expression.Type))
                throw new NotSupportedException("Please try calling the BeginExecute overload with 2 generic parameters instead");
            asyncContext.BeginLoadSingle<TResult>(callback, new QueryTranslator(expression.Type.FullName).Transform(expression));
        }



        public void BeginExecute<TResult>(Action<TResult> callback, System.Linq.Expressions.Expression expression)
        {
            if (TypeResolver.IsPrimitive(expression.Type))
                asyncContext.BeginLoadSingle<TResult>(callback, new QueryTranslator(expression.Type.FullName).Transform(expression));
            if (expression.Type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(expression.Type))
                throw new NotSupportedException("Please try calling the BeginExecute overload with 2 generic parameters instead");
            asyncContext.BeginLoadSingle<TResult>(EndLoadSingle<TResult>, new QueryTranslator(expression.Type.FullName).Transform(expression), callback);
        }


        public void BeginExecute<TEnumerable, T>(AsyncCallback callback, System.Linq.Expressions.Expression expression)
    where TEnumerable : IEnumerable<T>
        {
            if (expression.Type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(expression.Type))
            {
                asyncContext.BeginLoad<T>(callback, new QueryTranslator(expression.Type.FullName).Transform(expression));
            }
        }


        public void BeginExecute<TEnumerable, T>(Action<TEnumerable> callback, System.Linq.Expressions.Expression expression)
            where TEnumerable : IEnumerable<T>
        {
            if (expression.Type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(expression.Type))
            {
                asyncContext.BeginLoad<T>(EndLoadMany<TEnumerable, T>, new QueryTranslator(expression.Type.FullName).Transform(expression), callback);
            }
        }

        public void EndLoadMany<TEnumerableResult, TResult>(IAsyncResult result)
            where TEnumerableResult : IEnumerable<TResult>
        {
            ((Action<TEnumerableResult>)result.AsyncState)((TEnumerableResult)asyncContext.EndLoad<TResult>(result));
        }

        public void EndLoadSingle<TResult>(IAsyncResult result)
        {
            ((Action<TResult>)result.AsyncState)(asyncContext.EndLoadSingle<TResult>(result));
        }


        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            if (context == null && asyncContext != null)
                throw new NotSupportedException("This operation is not supposed to be done synchronously. Please try using asynchronous mode.");

            QueryTranslator translator;

            if (TypeResolver.IsAnonymous(expression.Type))
                translator = new QueryTranslator("");
            else
                translator = new QueryTranslator(expression.Type.FullName);

            var query = translator.Transform(expression);

            TResult result;

            if (TypeResolver.IsPrimitive(expression.Type))
                result = context.LoadScalar<TResult>(query);
            else if (expression.Type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(expression.Type))
                result = (TResult)load.MakeGenericMethod(expression.Type.GetInterfaces().First(t=> t.IsGenericType && t.GetGenericTypeDefinition()==typeof(IEnumerable<>)).GetGenericArguments()[0])
                    .Invoke(context, new object[] { query });
            else
                result = context.LoadSingle<TResult>(query);

            IPersistenceEngineObjectContext peContext = context as IPersistenceEngineObjectContext;
            if (peContext == null && translator.InferredRelationShips.Count > 0)
                throw new NotSupportedException("Non Persistence engine context do not support loading relationships that way. You have to do it manually");


            //Infer relationships
            if (peContext != null)
            {
                MethodInfo infer;
                if (expression.Type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(expression.Type))
                    infer = QueryProvider.infer.MakeGenericMethod(expression.Type.GetInterfaces().First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0]);
                else
                    infer = QueryProvider.infer.MakeGenericMethod(typeof(TResult));

                System.Collections.IEnumerable entities = result as System.Collections.IEnumerable;
                if (entities == null)
                    entities = new TResult[] { result };
                foreach (var relationship in translator.InferredRelationShips)
                {
                    infer.Invoke(peContext, new object[] { entities, relationship });
                }
            }


            return result;
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
            //return context.Load<TResult>();
        }

        #endregion
    }
}
