using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NLINQ = Evaluant.NLinq.Expressions;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.Linq;

namespace Evaluant.Uss.Extensions
{
    class QueryTranslator
    {
        public QueryTranslator(string sourceType)
        {
            identifiers.Add("#First", new NLINQ.Identifier("e0"));
            from = new NLINQ.FromClause(sourceType, identifiers["#First"], null);
            InferredRelationShips = new List<string>();
        }

        NLINQ.FromClause from;
        NLINQ.Expression tree;
        public IList<string> InferredRelationShips { get; set; }

        public NLINQ.Expression Transform(System.Linq.Expressions.Expression expression)
        {
            Visit(Evaluator.PartialEval(expression));
            if (tree == null)
                tree = new NLINQ.QueryExpression(from, new NLINQ.QueryBody(clauses, new NLINQ.SelectClause(from.Identifier), null));
            return tree;
        }

        public NLINQ.Expression Visit(System.Linq.Expressions.Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.AddChecked:
                    break;
                case ExpressionType.ArrayIndex:
                    break;
                case ExpressionType.ArrayLength:
                    break;
                case ExpressionType.Call:
                    return Visit((MethodCallExpression)expression);
                case ExpressionType.Coalesce:
                    break;
                case ExpressionType.Conditional:
                    break;
                case ExpressionType.Constant:
                    return Visit((ConstantExpression)expression);
                case ExpressionType.Convert:
                    break;
                case ExpressionType.ConvertChecked:
                    break;
                case ExpressionType.Add:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return Visit((System.Linq.Expressions.BinaryExpression)expression);
                case ExpressionType.Invoke:
                    break;
                case ExpressionType.Lambda:
                    return Visit((System.Linq.Expressions.LambdaExpression)expression);
                case ExpressionType.LeftShift:
                    break;
                case ExpressionType.ListInit:
                    break;
                case ExpressionType.MemberAccess:
                    return Visit((System.Linq.Expressions.MemberExpression)expression);
                case ExpressionType.MemberInit:
                    break;
                case ExpressionType.Negate:
                    return Visit((System.Linq.Expressions.UnaryExpression)expression);
                case ExpressionType.NegateChecked:
                    break;
                case ExpressionType.New:
                    return Visit((System.Linq.Expressions.NewExpression)expression);
                case ExpressionType.NewArrayBounds:
                    break;
                case ExpressionType.NewArrayInit:
                    break;
                case ExpressionType.Not:
                    return Visit((System.Linq.Expressions.UnaryExpression)expression);
                case ExpressionType.Parameter:
                    return Visit((ParameterExpression)expression);
                case ExpressionType.Quote:
                    return Visit((System.Linq.Expressions.UnaryExpression)expression);
                case ExpressionType.RightShift:
                    break;
                case ExpressionType.TypeAs:
                    break;
                case ExpressionType.TypeIs:
                    break;
                case ExpressionType.UnaryPlus:
                    break;
                default:
                    break;
            }
            return null;
        }

        public NLinq.Expressions.Expression Visit(System.Linq.Expressions.BinaryExpression expression)
        {
            return new NLinq.Expressions.BinaryExpression(VisitBinary(expression.NodeType), Visit(expression.Left), Visit(expression.Right));
        }

        public NLinq.Expressions.Expression Visit(System.Linq.Expressions.NewExpression expression)
        {
            AnonymousParameterList @params = new AnonymousParameterList();
            for (int i = 0; i < expression.Arguments.Count; i++)
            {
                System.Linq.Expressions.Expression arg = expression.Arguments[i];
                string name = expression.Members[i].Name;
                @params.Add(new AnonymousParameter(new Identifier(name), Visit(arg)));
            }
            return new AnonymousNew(null, @params);
        }

        private NLinq.Expressions.BinaryExpressionType VisitBinary(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return BinaryExpressionType.Plus;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return BinaryExpressionType.And;
                case ExpressionType.Divide:
                    return BinaryExpressionType.Div;
                case ExpressionType.Equal:
                    return BinaryExpressionType.Equal;
                case ExpressionType.ExclusiveOr:
                    return BinaryExpressionType.Or;
                case ExpressionType.GreaterThan:
                    return BinaryExpressionType.Greater;
                case ExpressionType.GreaterThanOrEqual:
                    return BinaryExpressionType.GreaterOrEqual;
                case ExpressionType.LessThan:
                    return BinaryExpressionType.Lesser;
                case ExpressionType.LessThanOrEqual:
                    return BinaryExpressionType.LesserOrEqual;
                case ExpressionType.Modulo:
                    return BinaryExpressionType.Modulo;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return BinaryExpressionType.Times;
                case ExpressionType.NotEqual:
                    return BinaryExpressionType.NotEqual;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return BinaryExpressionType.Or;
                case ExpressionType.Power:
                    return BinaryExpressionType.Pow;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return BinaryExpressionType.Minus;
            }
            return BinaryExpressionType.Unknown;
        }

        ClauseList clauses = new ClauseList();

        public NLinq.Expressions.Expression Visit(MethodCallExpression call)
        {
            if (call.Method.IsStatic && (call.Method.DeclaringType == typeof(Enumerable) || call.Method.DeclaringType == typeof(Queryable)) || call.Method.DeclaringType == typeof(ContextExtensions))
            {
                NLinq.Expressions.Expression subExpression = Visit(call.Arguments[0]);
                switch (call.Method.Name)
                {
                    case "Skip":
                    case "Take":
                        if (tree == null)
                        {
                            if (subExpression == null)
                                subExpression = new QueryExpression(from, new QueryBody(clauses, new SelectClause(identifiers["#First"]), null));
                            tree = new NLINQ.MemberExpression(new MethodCall(call.Method.Name, Visit(call.Arguments[1])), subExpression);
                        }
                        else
                            tree = new NLINQ.MemberExpression(new MethodCall(call.Method.Name, Visit(call.Arguments[1])), tree);
                        return tree;
                    case "Cast":
                        return subExpression;
                    case "FirstOrDefault":
                    case "LastOrDefault":
                    case "Last":
                    case "First":
                        string methodName = call.Method.Name;
                        if (call.Method.Name.EndsWith("OrDefault"))
                            methodName = methodName.Substring(0, call.Method.Name.Length - 9);
                        if (tree == null)
                        {
                            if (subExpression == null)
                                subExpression = new QueryExpression(from, new QueryBody(clauses, new SelectClause(identifiers["#First"]), null));
                            tree = new NLINQ.MemberExpression(new MethodCall(new Identifier(methodName)), subExpression);
                        }
                        else
                            tree = new NLINQ.MemberExpression(new MethodCall(new Identifier(methodName)), tree);
                        break;
                    case "Select":
                        SelectOrGroupClause select = new SelectClause(Visit(call.Arguments[1]));
                        tree = new QueryExpression(from, new QueryBody(clauses, select, null));
                        identifiers["#First"].Text = "e" + identifiers["#First"].GetHashCode();
                        identifiers.Clear();
                        identifiers["#First"] = new Identifier("e0");
                        clauses = new ClauseList();
                        from = new FromClause(null, identifiers["#First"], tree);
                        return tree;
                    case "SelectMany":
                        //Select the result of the join (a load reference)
                        FromClause from2 = null;
                        Identifier newElement = null;
                        select = null;

                        if (call.Arguments.Count == 2)
                            newElement = new Identifier("e1");
                        if (call.Arguments.Count == 3)
                            newElement = new Identifier(((System.Linq.Expressions.LambdaExpression)((System.Linq.Expressions.UnaryExpression)call.Arguments[2]).Operand).Parameters[1].Name);

                        from2 = new FromClause(call.Method.GetGenericArguments()[1].FullName, newElement, Visit(call.Arguments[1]));

                        if (call.Arguments.Count == 2)
                        {
                            //System.Linq.Expressions.LambdaExpression lambda = ((System.Linq.Expressions.LambdaExpression)((System.Linq.Expressions.UnaryExpression)call.Arguments[1]).Operand);
                            identifiers["#First"].Text = "e" + identifiers["#First"].GetHashCode();

                            //newElement = new Identifier("e0");
                            identifiers.Clear();
                            newElement.Text = "e0";
                            identifiers.Add("#First", newElement);
                            identifiers.Add(newElement.Text, newElement);
                            select = new SelectClause(newElement);
                        }

                        //Select a property of the select many
                        if (call.Arguments.Count == 3)
                        {
                            //newElement = new Identifier(((System.Linq.Expressions.LambdaExpression)((System.Linq.Expressions.UnaryExpression)call.Arguments[2]).Operand).Parameters[1].Name);
                            identifiers.Add(newElement.Text, newElement);
                            Identifier src = new Identifier(((System.Linq.Expressions.LambdaExpression)((System.Linq.Expressions.UnaryExpression)call.Arguments[2]).Operand).Parameters[0].Name);
                            if (!identifiers.ContainsKey(src.Text))
                                identifiers.Add(src.Text, identifiers["#First"]);
                            select = new SelectClause(Visit(((System.Linq.Expressions.LambdaExpression)((System.Linq.Expressions.UnaryExpression)call.Arguments[2]).Operand).Body));
                            if (((SelectClause)select).Expression == null)
                                select = null;
                        }
                        if (call.Arguments.Count > 3)
                            throw new NotSupportedException();
                        //from2 = new FromClause(call.Method.GetGenericArguments()[1].FullName, newElement, Visit(((System.Linq.Expressions.LambdaExpression)((System.Linq.Expressions.UnaryExpression)call.Arguments[1]).Operand).Body));
                        clauses.Add(from2);
                        tree = new QueryExpression(from, new QueryBody(clauses, select, null));
                        break;
                    case "Count":
                    case "Avg":
                    case "Sum":
                    case "Any":
                        if (call.Arguments.Count > 1)
                            clauses.Add(new WhereClause(Visit(call.Arguments[1])));
                        if (subExpression == null)
                            subExpression = new QueryExpression(from, new QueryBody(clauses, new SelectClause(identifiers["#First"]), null));
                        tree = new NLINQ.MemberExpression(new MethodCall(new Identifier(call.Method.Name)), subExpression);
                        return tree;
                    case "Intersect":
                        break;
                    case "Where":
                        clauses.Add(new WhereClause(Visit(call.Arguments[1])));
                        tree = null;
                        if (from == null)
                            from = new FromClause(null, identifiers["#First"], subExpression);
                        break;
                    case "OrderBy":
                        NLINQ.Expression m = Visit(call.Arguments[1]);
                        clauses.Add(new OrderByClause(new OrderByCriteria(m, true)));
                        break;
                    case "ThenBy":
                        break;
                    case "Infer":
                        var inferQuery = new QueryTranslator(from.Type);
                        var result = inferQuery.Visit(call.Arguments[1]);
                        var first = inferQuery.identifiers["#First"];
                        NLINQ.MemberExpression me = (NLINQ.MemberExpression)result;
                        while (me.Previous != first)
                            me = (NLINQ.MemberExpression)me.Previous;
                        me.Previous = null;
                        if (inferQuery.tree != null)
                            result = inferQuery.tree;
                        InferredRelationShips.Add(result.ToString());
                        tree = subExpression;
                        return subExpression;
                }
            }
            if (call.Method.DeclaringType == typeof(string))
            {
                NLinq.Expressions.Expression source = Visit(call.Object);
                List<NLinq.Expressions.Expression> parameters = new List<NLinq.Expressions.Expression>();
                foreach (var arg in call.Arguments)
                {
                    var param = Visit(arg);
                    if (param != null)
                        parameters.Add(param);
                }
                return new NLinq.Expressions.MemberExpression(
                    new NLinq.Expressions.MethodCall(new NLinq.Expressions.Identifier(call.Method.Name), parameters.ToArray()),
                    source);
            }
            return null;
        }

        Dictionary<string, NLinq.Expressions.Identifier> identifiers = new Dictionary<string, NLinq.Expressions.Identifier>();

        public NLinq.Expressions.Expression Visit(System.Linq.Expressions.LambdaExpression expression)
        {
            List<string> identifiers = new List<string>();
            foreach (ParameterExpression param in expression.Parameters)
            {
                if (!string.IsNullOrEmpty(param.Name))
                    identifiers.Add(param.Name);

                Identifier identifier;

                if (this.identifiers["#First"].Text == "e0")
                {
                    this.identifiers["#First"].Text = param.Name;
                    this.identifiers.Add(param.Name, this.identifiers["#First"]);
                    identifiers.Remove(param.Name);
                }
                else if (!this.identifiers.TryGetValue(param.Name, out identifier))
                {
                    identifier = new Identifier(param.Name);
                    if (string.IsNullOrEmpty(param.Name))
                        identifier.Text = "e" + param.GetHashCode();
                    this.identifiers.Add(param.Name, identifier);
                }
            }
            if (from == null)
                Visit(expression.Parameters[0]);
            NLinq.Expressions.Expression result = Visit(expression.Body);
            return result;
        }

        public NLinq.Expressions.Expression Visit(System.Linq.Expressions.UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Not)
                return new NLinq.Expressions.UnaryExpression(UnaryExpressionType.Not, Visit(expression.Operand));
            if (expression.NodeType == ExpressionType.Negate)
                return new NLinq.Expressions.UnaryExpression(UnaryExpressionType.Negate, Visit(expression.Operand));
            return Visit(expression.Operand);
        }

        public NLinq.Expressions.Expression Visit(System.Linq.Expressions.MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression && ((((ParameterExpression)expression.Expression).Name.StartsWith("<>")) || (((ParameterExpression)expression.Expression).Name.StartsWith("VB$"))))
                return identifiers[expression.Member.Name];
            NLinq.Expressions.Expression result = Visit(expression.Expression);
            return new NLINQ.MemberExpression(new Identifier(expression.Member.Name), result);
        }

        public NLinq.Expressions.Expression Visit(ParameterExpression expression)
        {
            if (from == null)
                from = new FromClause(expression.Type.FullName, this.identifiers["#First"], null);

            return identifiers[expression.Name];
        }

        public NLinq.Expressions.Expression Visit(ConstantExpression expression)
        {
            if (expression.Value != null && expression.Value.GetType().IsGenericType && (expression.Value.GetType().GetGenericTypeDefinition() == typeof(QueryableUss<>) || expression.Value.GetType().GetGenericTypeDefinition() == typeof(AsyncQueryableUss<>)))
            {
                from = new FromClause(expression.Value.GetType().GetGenericArguments()[0].FullName, this.identifiers["#First"], null);
                return null;
            }
            else
                return new ValueExpression(expression.Value, Utility.Helper.GetTypeCode(expression.Value));
        }


    }
}
