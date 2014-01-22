using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public class NQuery : NLinqQuery
    {
        public NQuery(Type type)
            : base("from " + type.FullName + " e select e")
        {
        }



        public NQuery(string expression)
            : base(expression)
        {
        }

        public NQuery(Expression linqExpression)
        {
            this.linqExpression = linqExpression;
        }

        NLinqQuery parent;

        public NQuery(Expression linqExpression, Dictionary<string,object> parameters):this(linqExpression)
        {
            Parameters = parameters;
        }

        public NQuery(NLinqQuery parent, string expression)
            : this(expression)
        {
            this.parent = parent;
        }

        public NQuery(NQuery parent, Expression linqExpression)
            : this(linqExpression)
        {
            this.parent = parent;
        }

        public NLinqQuery End()
        {
            return parent;
        }

        public NQuery Count()
        {
            return new NQuery(this, new MemberExpression(new MethodCall(new Identifier("Count")), linqExpression));
        }

        public NQuery Page(int first, int max)
        {
            if (first > 0 && max > 0)
            {
                return new NQuery(this, new MemberExpression(new MethodCall(new Identifier("Take"), new ValueExpression(max, TypeCode.Int32)), new MemberExpression(new MethodCall(new Identifier("Skip"), new ValueExpression(first, TypeCode.Int32)), linqExpression)));
            }

            if (first > 0)
            {
                return new NQuery(this, new MemberExpression(new MethodCall(new Identifier("Skip"), new ValueExpression(first, TypeCode.Int32)), linqExpression));
            }

            if (max > 0)
            {
                return new NQuery(this, new MemberExpression(new MethodCall(new Identifier("Take"), new ValueExpression(max, TypeCode.Int32)), linqExpression));
            }

            return this;
        }

        public NQuery Where(object left, object right, BinaryExpressionType type)
        {
            Expression e = new BinaryExpression(type, new ValueExpression(left, Convert.GetTypeCode(left)), new ValueExpression(right, Convert.GetTypeCode(right)));
            return new NQuery(this, e);
        }


        public NQuery Where(string constraint)
        {
            BinaryExpression binaryExpression = (BinaryExpression)GetParser(constraint).conditionalExpression().value;
            return new NQuery(this, binaryExpression);
        }
    }
}
