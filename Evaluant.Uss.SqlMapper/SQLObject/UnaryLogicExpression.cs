using System;
using SQLObject.Renderer;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de LogicOperator.
	/// </summary>
	public enum UnaryLogicOperator { Minus, Not, Unknown }

	public class UnaryLogicExpression : LogicExpression
	{
		private ISQLExpression _Operand;
		private UnaryLogicOperator _Operator;

		public UnaryLogicExpression(UnaryLogicOperator op, ISQLExpression operand)
		{
			_Operator = op;
			_Operand = operand;
		}

		public ISQLExpression Operand
		{
			get { return _Operand; }
		}

		public UnaryLogicOperator Operator
		{
			get { return _Operator; }
		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
