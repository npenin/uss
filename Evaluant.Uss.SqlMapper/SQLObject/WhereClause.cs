using System;
using SQLObject.Renderer;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de WhereClause.
	/// </summary>
	public class WhereClause
	{
		private BinaryLogicOperator _DefaultOperator;
		private LogicExpressionCollection _SearchCondition = new LogicExpressionCollection();

		public WhereClause(BinaryLogicOperator defaultOperator)
		{
			_DefaultOperator = defaultOperator;
		}

		public LogicExpressionCollection SearchCondition
		{
			get { return _SearchCondition; }
			set { _SearchCondition = value; }
		}

		public BinaryLogicOperator DefaultOperator
		{
			get { return _DefaultOperator; }
		}

        [System.Diagnostics.DebuggerStepThrough]
		public void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
