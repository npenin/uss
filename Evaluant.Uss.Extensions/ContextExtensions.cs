using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evaluant.Uss.ObjectContext.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Evaluant.Uss.Linq
{
    public static class ContextExtensions
    {
        internal static MethodInfo inferMethod;
        static ContextExtensions()
        {
            inferMethod = typeof(ContextExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .SelectMany(mi => mi.GetParameters(), (mi, p) => new { Method = mi, Parameter = p })
                .Where(mip => mip.Parameter.ParameterType.IsGenericType && mip.Parameter.ParameterType.GetGenericTypeDefinition() == typeof(IInferrable<>))
                .Select(mip => mip.Method)
                .FirstOrDefault();
        }

        public static IInferrable<T> Cast<T>(this IObjectContextAsync context)
        {
            return new AsyncQueryableUss<T>(context);
        }

        public static IInferrable<T> Cast<T>(this IObjectContext context)
        {
            return new QueryableUss<T>(context);
        }

        public static IInferrable<T> Infer<T, U>(this IObjectContext context, Expression<Func<T, U>> accessor)
        {
            return Infer<T, U>(new QueryableUss<T>(context), accessor);
        }

        public static IInferrable<T> Infer<T, U>(this IInferrable<T> query, Expression<Func<T, U>> accessor)
        {
            return (IInferrable<T>)query.Provider.CreateQuery<T>(Expression.Call(inferMethod.MakeGenericMethod(typeof(T), typeof(U)), query.Expression, Expression.Quote(accessor)));
        }
    }
}
