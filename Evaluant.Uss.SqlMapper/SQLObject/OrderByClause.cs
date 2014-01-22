using System;
using SQLObject.Renderer;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de OrderByExpression.
	/// </summary>
	public class OrderByClause : OrderByClauseColumnCollection
	{
//		private bool _IsDesc = false;

		public OrderByClause(SelectStatement select)
		{
			this.select = select;
		}

		private SelectStatement select;
		public SelectStatement Select
		{
			get { return select; }
			set { select = value; }
		}

		public void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

	}
}
