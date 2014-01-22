using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Evaluant.Uss.Linq
{
    public static class LinqExtensions
    {
        public static LinqQuery<TSource, TResult> Select<TSource, TResult>(this LinqQuery<TSource, TSource> source, Expression<Func<TSource, TResult>> selector)
            where TSource : class
            where TResult : class
        {
            LinqQuery<TSource, TResult> result = new LinqQuery<TSource, TResult>(source.Translator, source.Query, source.ObjectContext);
            result.Selector = selector;
            if (selector.Body.NodeType == ExpressionType.MemberAccess)
            {
                Type memberType = ((PropertyInfo)((MemberExpression)selector.Body).Member).PropertyType;
                if (memberType.IsPrimitive || memberType == typeof(string) || ((MemberExpression)selector.Body).Member.DeclaringType.Name.StartsWith("<>"))
                    return result;
                else
                {
                    result.Translator.Translate(selector);
                    if (result.Translator.Result != null && !result.Query.Constraint.Contains("." + result.Translator.Result))
                        result.Query.Constraint += "." + result.Translator.Result;
                }
            }
            return result;
        }

        public static LinqQuery<TResult, TResult> SelectMany<TSource, TCollection, TResult>(this LinqQuery<TSource, TSource> source, Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector)
            where TSource : class
            where TResult : class
        {
            string name = collectionSelector.Parameters[0].Name;

            if (!source.Translator.Identifiers.ContainsKey(name))
            {
                Evaluant.OPath.Expressions.Identifier i = source.Translator.Identifiers[String.Empty];
                source.Translator.Identifiers.Add(name, i);
                source.Translator.Identifiers.Remove(String.Empty);
            }

            MemberExpression memberExpression = null;
            // Can be "Convert(element.Addresses)" instead of "element.Addresses"
            if (collectionSelector.Body is UnaryExpression)
            {
                memberExpression = (MemberExpression)((UnaryExpression)collectionSelector.Body).Operand;
            }
            else if (collectionSelector.Body is MemberExpression)
            {
                memberExpression = (MemberExpression)collectionSelector.Body;
            }
            else if (collectionSelector.Body is MethodCallExpression)
            {
                memberExpression = ((MemberExpression)((MethodCallExpression)collectionSelector.Body).Arguments[0]);
            }

            string property = memberExpression.Member.Name;

            string idName = resultSelector.Parameters[1].Name;

            source.Translator.Identifiers.Add(idName, new Evaluant.OPath.Expressions.Identifier(property, null));
            source.Translator.Path.Identifiers.Add(source.Translator.Identifiers[idName]);
            source.Translator.SelectMany = true;

            // Refreshes the constraint to take the new information into account
            source.Query.Constraint = source.Translator.Path.ToString();

            return new LinqQuery<TResult, TResult>(source.Translator, source.Query, source.ObjectContext);
        }

        /// <summary>
        /// Called when no Select is after from from, then use the collection of the last from in the query
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="collectionSelector"></param>
        /// <returns></returns>
        public static LinqQuery<TResult, TResult> SelectMany<TSource, TResult>(this LinqQuery<TSource, TSource> source, Expression<Func<TSource, IEnumerable<TResult>>> collectionSelector)
            where TSource : class
            where TResult : class
        {
            string name = collectionSelector.Parameters[0].Name;

            if (!source.Translator.Identifiers.ContainsKey(name))
            {
                Evaluant.OPath.Expressions.Identifier i = source.Translator.Identifiers[String.Empty];
                source.Translator.Identifiers.Add(name, i);
                source.Translator.Identifiers.Remove(String.Empty);
            }

            MemberExpression memberExpression = null;
            // Can be "Convert(element.Addresses)" instead of "element.Addresses"
            if (collectionSelector.Body is UnaryExpression)
            {
                memberExpression = (MemberExpression)((UnaryExpression)collectionSelector.Body).Operand;
            }
            else if (collectionSelector.Body is MemberExpression)
            {
                memberExpression = (MemberExpression)collectionSelector.Body;
            }
            else if (collectionSelector.Body is MethodCallExpression)
            {
                memberExpression = ((MemberExpression)((MethodCallExpression)collectionSelector.Body).Arguments[0]);
            }

            string property = memberExpression.Member.Name;
            string idName = ((ParameterExpression)memberExpression.Expression).Name;
            source.Translator.Identifiers.Add("selectmany", new Evaluant.OPath.Expressions.Identifier(property, null));
            source.Translator.Path.Identifiers.Add(source.Translator.Identifiers["selectmany"]);
            source.Translator.SelectMany = true;

            // Refreshes the constraint to take the new information into account
            source.Query.Constraint = source.Translator.Path.ToString();

            return new LinqQuery<TResult, TResult>(source.Translator, source.Query, source.ObjectContext);
        }

        public static LinqQuery<TSource, TSource> Take<TSource>(this LinqQuery<TSource, TSource> source, int count)
        where TSource : class
        {
            source.Query.MaxResults = count;
            return source;
        }

        public static LinqQuery<TSource, TSource> Skip<TSource>(this LinqQuery<TSource, TSource> source, int count)
        where TSource : class
        {
            source.Query.FirstResult = count + 1;
            return source;
        }

        public static TResult First<TSource, TResult>(this LinqQuery<TSource, TResult> source)
            where TSource : class
            where TResult : class
        {
            source.Query.FirstResult = 1;
            source.Query.MaxResults = 1;

            IEnumerator<TResult> enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            else
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
        }

        public static IEnumerable<TResult> Except<TSource1, TSource2, TResult>(
    this LinqQuery<TSource1, TResult> first,
    LinqQuery<TSource2, TResult> second)
            where TSource1 : class
            where TSource2 : class
            where TResult : class
        {
            return first.Except(second.ToList());
        }

        public static TResult FirstOrDefault<TSource, TResult>(this LinqQuery<TSource, TResult> source)
            where TSource : class
            where TResult : class
        {
            source.Query.FirstResult = 1;
            source.Query.MaxResults = 1;

            IEnumerator<TResult> enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            else
            {
                return default(TResult);
            }
        }

        internal static T Aggregate<TSource, TResult, T>(string function, Type type, LinqQuery<TSource, TResult> source, Expression<Func<TSource, T>> selector)
            where TSource : class
            where TResult : class
        {
            string property = source.Translator.Translate(selector.Body).Result.ToString();
            string constraint = String.Concat(function, "(", type.FullName.Replace(".", ":") + source.Query.Constraint + "." + property, ")");
            return (T)Convert.ChangeType(source.ObjectContext.LoadScalar(constraint), typeof(T));
        }

        public static int Count<TSource, TResult>(this LinqQuery<TSource, TResult> source)
            where TSource : class
            where TResult : class
        {
            string constraint = String.Concat("count", "(", source.Query.FromType.FullName.Replace(".", ":") + source.Query.Constraint, ")");
            return (int)source.ObjectContext.LoadScalar(constraint);
        }

        public static int Count<TSource, TResult>(this LinqQuery<TSource, TResult> source, Expression<Func<TSource, bool>> selector)
            where TSource : class
            where TResult : class
        {
            Expression e = Evaluator.PartialEval(selector);
            source.Translator.Where(e);

            source.Query.Constraint = source.Translator.Path.ToString();

            string constraint = String.Concat("count", "(", source.Query.FromType.FullName.Replace(".", ":") + source.Query.Constraint, ")");
            return (int)source.ObjectContext.LoadScalar(constraint);
        }

        public static T Max<TSource, TResult, T>(this LinqQuery<TSource, TResult> source, Expression<Func<TSource, T>> selector)
            where TSource : class
            where TResult : class
        {
            return Aggregate("max", typeof(TSource), source, selector);
        }

        public static T Average<TSource, TResult, T>(this LinqQuery<TSource, TResult> source, Expression<Func<TSource, T>> selector)
            where TSource : class
            where TResult : class
        {
            return Aggregate("avg", typeof(TSource), source, selector);
        }

        public static T Min<TSource, TResult, T>(this LinqQuery<TSource, TResult> source, Expression<Func<TSource, T>> selector)
            where TSource : class
            where TResult : class
        {
            return Aggregate("min", typeof(TSource), source, selector);
        }

        public static T Sum<TSource, TResult, T>(this LinqQuery<TSource, TResult> source, Expression<Func<TSource, T>> selector)
            where TSource : class
            where TResult : class
        {
            return Aggregate("sum", typeof(TSource), source, selector);
        }


        public static LinqQuery<TSource, TSource> OrderBy<TSource, TKey>(this LinqQuery<TSource, TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource : class
        {
            MemberExpression me = (MemberExpression)keySelector.Body;

            source.Query.OrderBy.Add(me.Member.Name);
            return source;
        }

        public static LinqQuery<TSource, TSource> OrderByDescending<TSource, TKey>(this LinqQuery<TSource, TSource> source, Expression<Func<TSource, TKey>> keySelector)
            where TSource : class
        {
            string orderby = source.Translator.Translate(keySelector.Body).Result.ToString();
            source.Query.OrderBy.Add(orderby + " desc");
            return source;
        }

        public static LinqQuery<TSource, TSource> Where<TSource>(this LinqQuery<TSource, TSource> source, Expression<Func<TSource, bool>> expression)
            where TSource : class
        {
            // Evaluates all partial sub trees in the expression (e.g. variables)
            Expression e = Evaluator.PartialEval(expression);
            source.Translator.Where(e);

            if (String.IsNullOrEmpty(source.Query.Constraint))
            {
                source.Query.Constraint = source.Translator.Path.ToString();
            }
            else
            {
                string nextConstraint = source.Translator.Path.ToString();
                string previousConstraint = source.Query.Constraint;
                if (!nextConstraint.StartsWith(previousConstraint))
                {
                    source.Query.Constraint = previousConstraint.Insert(source.Query.Constraint.Length - 1, " and (" + nextConstraint.Substring(1, nextConstraint.Length - 2) + ")");
                }
                else
                {
                    source.Query.Constraint = source.Translator.Path.ToString();
                }
            }
            return source;
        }
    }
}
