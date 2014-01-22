using System;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de BinaryOperator.
	/// </summary>
	public interface BinaryExpression
	{
		ISQLExpression LeftOperand { get; }
		ISQLExpression RightOperand { get; }

	}
}
