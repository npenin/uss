using System;
using SQLObject.Renderer;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de LogicOperator.
	/// </summary>
	public enum BinaryLogicOperator { And, Or, Equals, NotEquals, Lesser, Greater, LesserOrEquals, GreaterOrEquals, Plus, Minus, Times, Div, Modulo}

	public class BinaryLogicExpression : LogicExpression, BinaryExpression
	{
		private ISQLExpression _LeftOperand;
		private ISQLExpression _RightOperand;

		private BinaryLogicOperator _Operator;

		public BinaryLogicExpression(ISQLExpression leftOperand, BinaryLogicOperator op, ISQLExpression rightOperand)
		{
			_LeftOperand = leftOperand;
			_Operator = op;
			_RightOperand = rightOperand;
		}

		public ISQLExpression LeftOperand
		{
			get { return _LeftOperand; }
			set { _LeftOperand = value; }
		}

		public ISQLExpression RightOperand
		{
			get { return _RightOperand; }
			set { _RightOperand = value; }
		}

		public BinaryLogicOperator Operator
		{
			get { return _Operator; }
			set { _Operator = value; }
		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}


        public override string ToString()
        {
            return LeftOperand.ToString() + Operator + RightOperand.ToString();
        }
	}
}
