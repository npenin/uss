// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.CommonVisitors
{
    public delegate object Mutator<T, RT>(T item);
    public delegate T Mutator<T>(T item);

    public abstract class ExpressionVisitor<T> : IVisitor<Expression>
        where T : ExpressionUpdater
    {
        /// <summary>
        /// Exists to prevent stack overflows
        /// </summary>
        protected Expression visiting;

        protected T updater;

        protected ExpressionVisitor(T updater)
        {
            this.updater = updater;
        }

        public virtual Expression Visit(Expression exp)
        {
            if (visiting == exp)
                return VisitUnknown(exp);
            if (exp == null)
                return exp;
            switch (exp.ExpressionType)
            {
                case ExpressionTypes.Unary:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionTypes.Ternary:
                    return this.VisitTernary((TernaryExpression)exp);
                case ExpressionTypes.Binary:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionTypes.Constant:
                    return this.VisitConstant((ValueExpression)exp);
                case ExpressionTypes.Call:
                    return this.VisitMethodCall((MethodCall)exp);
                case ExpressionTypes.Parameter:
                    return this.VisitParameter((Parameter)exp);
            }
            return this.VisitUnknown(exp);
        }

        protected virtual Expression VisitUnknown(Expression expression)
        {
            throw new Exception(string.Format("Unhandled expression type: '{0}'", expression.GetType().FullName));
        }

        protected virtual IEnumerable<ArrayItemType> VisitEnumerable<ArrayType, ArrayItemType>(ArrayType original, Mutator<ArrayItemType> del, EqualityComparer<ArrayItemType> comparer)
            where ArrayType : IEnumerable<ArrayItemType>
        {
            if (original != null)
            {
                List<ArrayItemType> list = null;
                foreach (ArrayItemType exp in original)
                {
                    if (exp == null)
                        continue;
                    ArrayItemType p = del(exp);
                    if (list != null && !comparer.Equals(p, default(ArrayItemType)))
                    {
                        list.Add(p);
                    }
                    else if (!comparer.Equals(p, exp))
                    {
                        list = new List<ArrayItemType>();
                        foreach (ArrayItemType exp2 in original)
                        {
                            if (comparer.Equals(exp2, exp))
                                break;
                            if (!comparer.Equals(p, default(ArrayItemType)))
                                list.Add(exp2);
                        }
                        if (!comparer.Equals(p, default(ArrayItemType)))
                            list.Add(p);
                    }
                }
                if (list != null)
                {
                    return list.ToArray();
                }
            }
            return original;

        }

        protected virtual ArrayItemType[] VisitArray<ArrayItemType>(ArrayItemType[] original, Mutator<ArrayItemType> visit)
        {
            return (ArrayItemType[])VisitEnumerable(original, visit);
        }

        protected virtual ArrayItemType[] VisitArray<ArrayItemType>(ArrayItemType[] original, Mutator<ArrayItemType> visit, EqualityComparer<ArrayItemType> equaliter)
        {
            return (ArrayItemType[])VisitEnumerable(original, visit, equaliter);
        }


        protected virtual IEnumerable<ArrayItemType> VisitEnumerable<ArrayItemType>(IEnumerable<ArrayItemType> original, Mutator<ArrayItemType> visit)
        {
            return VisitEnumerable(original, visit, EqualityComparer<ArrayItemType>.Default);
        }

        protected virtual IEnumerable<AnonymousParameter> VisitAnonymousParameters(AnonymousParameterList parameters)
        {
            return VisitEnumerable(parameters, VisitAnonymousParameter);
        }

        protected virtual AnonymousParameter VisitAnonymousParameter(AnonymousParameter parameter)
        {
            Identifier identifier = VisitIdentifier(parameter.Identifier);
            Expression expression = Visit(parameter.Expression);
            return updater.Update(parameter, identifier, expression);
        }

        protected virtual Identifier VisitIdentifier(Identifier identifier)
        {
            return identifier;
        }

        //protected virtual MemberBinding VisitBinding(MemberBinding binding)
        //{
        //    switch (binding.BindingType)
        //    {
        //        case MemberBindingType.Assignment:
        //            return this.VisitMemberAssignment((MemberAssignment)binding);
        //        case MemberBindingType.MemberBinding:
        //            return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
        //        case MemberBindingType.ListBinding:
        //            return this.VisitMemberListBinding((MemberListBinding)binding);
        //        default:
        //            throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
        //    }
        //}

        //protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        //{
        //    ReadOnlyCollection<Expression> arguments = this.VisitExpressionList(initializer.Arguments);
        //    if (arguments != initializer.Arguments)
        //    {
        //        return Expression.ElementInit(initializer.AddMethod, arguments);
        //    }
        //    return initializer;
        //}

        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = this.Visit(u.Expression);
            return updater.Update(u, operand, u.Type);
        }



        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = this.Visit(b.LeftExpression);
            Expression right = this.Visit(b.RightExpression);
            if (left == null)
                return right;
            if (right == null)
                return left;
            //Expression conversion = this.Visit(b.Conversion);
            if (left != b.LeftExpression || right != b.RightExpression)
                return new BinaryExpression(b.Type, left, right);
            else
                return b;
        }

        //protected BinaryExpression UpdateBinary(BinaryExpression b, Expression left, Expression right, Expression conversion, bool isLiftedToNull, MethodInfo method)
        //{
        //    if (left != b.Left || right != b.Right || conversion != b.Conversion || method != b.Method || isLiftedToNull != b.IsLiftedToNull)
        //    {
        //        if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
        //        {
        //            return Expression.Coalesce(left, right, conversion as LambdaExpression);
        //        }
        //        else
        //        {
        //            return Expression.MakeBinary(b.NodeType, left, right, isLiftedToNull, method);
        //        }
        //    }
        //    return b;
        //}

        //protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        //{
        //    Expression expr = this.Visit(b.Expression);
        //    return this.UpdateTypeIs(b, expr, b.TypeOperand);
        //}

        //protected TypeBinaryExpression UpdateTypeIs(TypeBinaryExpression b, Expression expression, Type typeOperand)
        //{
        //    if (expression != b.Expression || typeOperand != b.TypeOperand)
        //    {
        //        return Expression.TypeIs(expression, typeOperand);
        //    }
        //    return b;
        //}

        protected virtual Expression VisitConstant(ValueExpression c)
        {
            return c;
        }

        protected virtual Expression VisitTernary(TernaryExpression c)
        {
            Expression test = this.Visit(c.LeftExpression);
            Expression ifTrue = this.Visit(c.MiddleExpression);
            Expression ifFalse = this.Visit(c.RightExpression);
            return updater.Update(c, test, ifTrue, ifFalse);
        }



        protected virtual Expression VisitParameter(Parameter p)
        {
            return p;
        }

        //protected virtual Expression VisitMemberAccess(MemberExpression m)
        //{
        //    Expression exp = this.Visit(m.Expression);
        //    return this.UpdateMemberAccess(m, exp, m.Member);
        //}

        //protected MemberExpression UpdateMemberAccess(MemberExpression m, Expression expression, MemberInfo member)
        //{
        //    if (expression != m.Expression || member != m.Member)
        //    {
        //        return Expression.MakeMemberAccess(expression, member);
        //    }
        //    return m;
        //}

        protected virtual Expression VisitMethodCall(MethodCall m)
        {
            Identifier obj = m.AnonIdentifier;
            if (m.AnonIdentifier != null)
                obj = this.VisitIdentifier(m.AnonIdentifier);
            Expression[] args = this.VisitExpressionList(m.Parameters);
            return updater.Update(m, obj, m.Identifier, args);
        }



        protected virtual Expression[] VisitExpressionList(Expression[] original)
        {
            return (Expression[])VisitArray(original, Visit);
        }

        //protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        //{
        //    Expression e = this.Visit(assignment.Expression);
        //    return this.UpdateMemberAssignment(assignment, assignment.Member, e);
        //}

        //protected MemberAssignment UpdateMemberAssignment(MemberAssignment assignment, MemberInfo member, Expression expression)
        //{
        //    if (expression != assignment.Expression || member != assignment.Member)
        //    {
        //        return Expression.Bind(member, expression);
        //    }
        //    return assignment;
        //}

        //protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        //{
        //    IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);
        //    return this.UpdateMemberMemberBinding(binding, binding.Member, bindings);
        //}

        //protected MemberMemberBinding UpdateMemberMemberBinding(MemberMemberBinding binding, MemberInfo member, IEnumerable<MemberBinding> bindings)
        //{
        //    if (bindings != binding.Bindings || member != binding.Member)
        //    {
        //        return Expression.MemberBind(member, bindings);
        //    }
        //    return binding;
        //}

        //protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        //{
        //    IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);
        //    return this.UpdateMemberListBinding(binding, binding.Member, initializers);
        //}

        //protected MemberListBinding UpdateMemberListBinding(MemberListBinding binding, MemberInfo member, IEnumerable<ElementInit> initializers)
        //{
        //    if (initializers != binding.Initializers || member != binding.Member)
        //    {
        //        return Expression.ListBind(member, initializers);
        //    }
        //    return binding;
        //}

        //protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        //{
        //    List<MemberBinding> list = null;
        //    for (int i = 0, n = original.Count; i < n; i++)
        //    {
        //        MemberBinding b = this.VisitBinding(original[i]);
        //        if (list != null)
        //        {
        //            list.Add(b);
        //        }
        //        else if (b != original[i])
        //        {
        //            list = new List<MemberBinding>(n);
        //            for (int j = 0; j < i; j++)
        //            {
        //                list.Add(original[j]);
        //            }
        //            list.Add(b);
        //        }
        //    }
        //    if (list != null)
        //        return list;
        //    return original;
        //}

        //protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        //{
        //    List<ElementInit> list = null;
        //    for (int i = 0, n = original.Count; i < n; i++)
        //    {
        //        ElementInit init = this.VisitElementInitializer(original[i]);
        //        if (list != null)
        //        {
        //            list.Add(init);
        //        }
        //        else if (init != original[i])
        //        {
        //            list = new List<ElementInit>(n);
        //            for (int j = 0; j < i; j++)
        //            {
        //                list.Add(original[j]);
        //            }
        //            list.Add(init);
        //        }
        //    }
        //    if (list != null)
        //        return list;
        //    return original;
        //}

        //protected virtual Expression VisitLambda(LambdaExpression lambda)
        //{
        //    Expression body = this.Visit(lambda.Body);
        //    return this.UpdateLambda(lambda, lambda.Type, body, lambda.Parameters);
        //}

        //protected LambdaExpression UpdateLambda(LambdaExpression lambda, Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
        //{
        //    if (body != lambda.Body || parameters != lambda.Parameters || delegateType != lambda.Type)
        //    {
        //        return Expression.Lambda(delegateType, body, parameters);
        //    }
        //    return lambda;
        //}

        protected virtual Expression VisitNew(TypedNew nex)
        {
            Expression[] args = this.VisitExpressionList(nex.Parameters);
            return updater.Update(nex, nex.Type, args);
        }

        public virtual OrderByClause VisitOrderBy(OrderByClause expression)
        {
            if (expression == null)
                return null;
            IEnumerable<OrderByCriteria> criteria = VisitOrderByCriteria(expression.Criterias);
            return updater.Update(expression, criteria);
        }

        protected virtual IEnumerable<OrderByCriteria> VisitOrderByCriteria(IEnumerable<OrderByCriteria> orderByCriteria)
        {
            return VisitEnumerable(orderByCriteria, VisitCriterium);
        }

        protected virtual OrderByCriteria VisitCriterium(OrderByCriteria criterium)
        {
            Expression expression = Visit(criterium.Expression);
            return updater.Update(criterium, expression, criterium.Ascending);
        }


        //protected virtual Expression VisitMemberInit(MemberInitExpression init)
        //{
        //    NewExpression n = this.VisitNew(init.NewExpression);
        //    IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings);
        //    return this.UpdateMemberInit(init, n, bindings);
        //}

        //protected MemberInitExpression UpdateMemberInit(MemberInitExpression init, NewExpression nex, IEnumerable<MemberBinding> bindings)
        //{
        //    if (nex != init.NewExpression || bindings != init.Bindings)
        //    {
        //        return Expression.MemberInit(nex, bindings);
        //    }
        //    return init;
        //}

        //protected virtual Expression VisitListInit(ListInitExpression init)
        //{
        //    NewExpression n = this.VisitNew(init.NewExpression);
        //    IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers);
        //    return this.UpdateListInit(init, n, initializers);
        //}

        //protected ListInitExpression UpdateListInit(ListInitExpression init, NewExpression nex, IEnumerable<ElementInit> initializers)
        //{
        //    if (nex != init.NewExpression || initializers != init.Initializers)
        //    {
        //        return Expression.ListInit(nex, initializers);
        //    }
        //    return init;
        //}

        //protected virtual Expression VisitNewArray(NewArrayExpression na)
        //{
        //    IEnumerable<Expression> exprs = this.VisitExpressionList(na.Expressions);
        //    return this.UpdateNewArray(na, na.Type, exprs);
        //}

        //protected NewArrayExpression UpdateNewArray(NewArrayExpression na, Type arrayType, IEnumerable<Expression> expressions)
        //{
        //    if (expressions != na.Expressions || na.Type != arrayType)
        //    {
        //        if (na.NodeType == ExpressionType.NewArrayInit)
        //        {
        //            return Expression.NewArrayInit(arrayType.GetElementType(), expressions);
        //        }
        //        else
        //        {
        //            return Expression.NewArrayBounds(arrayType.GetElementType(), expressions);
        //        }
        //    }
        //    return na;
        //}

        //protected virtual Expression VisitInvocation(InvocationExpression iv)
        //{
        //    IEnumerable<Expression> args = this.VisitExpressionList(iv.Arguments);
        //    Expression expr = this.Visit(iv.Expression);
        //    return this.UpdateInvocation(iv, expr, args);
        //}

        //protected InvocationExpression UpdateInvocation(InvocationExpression iv, Expression expression, IEnumerable<Expression> args)
        //{
        //    if (args != iv.Arguments || expression != iv.Expression)
        //    {
        //        return Expression.Invoke(expression, args);
        //    }
        //    return iv;
        //}
    }
}