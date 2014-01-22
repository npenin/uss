using System;

namespace SQLObject
{
	/// <summary>
	/// Description r�sum�e de BinaryOperator.
	/// </summary>
	public interface BinaryExpression
	{
		ISQLExpression LeftOperand { get; }
		ISQLExpression RightOperand { get; }

	}
}
