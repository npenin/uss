using System;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de Union.
	/// </summary>
	public class UnionStatement : SelectStatement
	{
		private SelectStatementCollection _SelectExpressions = new SelectStatementCollection();
		public SelectStatementCollection SelectExpressions
		{
			get { return _SelectExpressions; }
			set { _SelectExpressions = value; }
		}

		//private ITagMapping _TagMapping;

		public UnionStatement(ITagMapping tag)
			: base(tag)
		{
			//_TagMapping = tag;
		}

		public UnionStatement()
			:	base(null)
		{
		
		}

//		public ITagMapping TagMapping
//		{
//			get { return _TagMapping; }
//		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
