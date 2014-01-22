using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Services.Routing
{
    public abstract class Condition
    {
        public abstract bool IsSatisfied(object o);
        public enum BinaryOperator { And, Or }
    }

    public class BooleanCondition : Condition
    {
        public BooleanCondition(Condition left, Condition right) : this(left, right, BinaryOperator.And) { }
        public BooleanCondition(Condition left, Condition right, Condition.BinaryOperator op)
        {
            this.leftCondition = left;
            this.rightCondition = right;
            this.op = op;
        }

        protected Condition leftCondition;
        public Condition LeftCondition
        {
            get { return leftCondition; }
            set { leftCondition = value; }
        }

        protected Condition rightCondition;
        public Condition RightCondition
        {
            get { return rightCondition; }
            set { rightCondition = value; }
        }

        protected Condition.BinaryOperator op;
        public Condition.BinaryOperator Operator
        {
            get { return op; }
            set { op = value; }
        }

        public override bool IsSatisfied(object o)
        {
            switch (op)
            {
                case BinaryOperator.And:
                    return leftCondition.IsSatisfied(o) && rightCondition.IsSatisfied(o);
                case BinaryOperator.Or:
                    return leftCondition.IsSatisfied(o) || rightCondition.IsSatisfied(o);
            }
            return false;
        }
    }

    public class InversedCondition : Condition
    {
        public InversedCondition(Condition condition)
        {
            this.condition = condition;
        }

        protected Condition condition;
        public Condition Condition
        {
            get { return condition; }
            set { condition = value; }
        }

        public override bool IsSatisfied(object o)
        {
            return !condition.IsSatisfied(o);
        }
    }

    public class TypeCondition : Condition
    {
        public TypeCondition(Type type)
        {
            this.expectedType = type;
        }

        protected Type expectedType;
        public Type ExpectedType
        {
            get { return expectedType; }
            set { expectedType = value; }
        }

        public override bool IsSatisfied(object o)
        {
            return expectedType.IsAssignableFrom(o.GetType());
        }
    }

    public class MultiTypeCondition : Condition
    {
        public MultiTypeCondition(params Type[] expectedTypes)
        {
            foreach (Type expectedType in expectedTypes)
                this.expectedTypes.Add(expectedType);
        }

        protected IList<Type> expectedTypes = new List<Type>();
        public IList<Type> ExpectedTypes
        {
            get { return expectedTypes; }
        }

        public override bool IsSatisfied(object o)
        {
            if (o == null)
                return false;
            foreach (Type type in expectedTypes)
            {
                if (type.IsAssignableFrom(o.GetType()))
                    return true;
            }
            return false;
        }
    }
}
