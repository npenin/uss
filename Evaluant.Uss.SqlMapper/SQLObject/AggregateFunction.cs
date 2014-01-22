using System;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de Function.
	/// </summary>
	/// 
	public enum AggregateFunctionEnum { Avg, Max, Min, Sum, Count}

	public class AggregateFunction : ISQLExpression
	{
		private ISQLExpression _ValueExpression;
		private AggregateFunctionEnum _Type;

		private ITagMapping _TagMapping;

        public AggregateFunction(ITagMapping tag, AggregateFunctionEnum type, ISQLExpression exp)
		{
        	_ValueExpression = exp;
			_Type = type;
			_TagMapping = tag;
		}

		public ISQLExpression ValueExpression
		{
			get { return _ValueExpression; }
		}

		public AggregateFunctionEnum Type
		{
			get { return _Type; }
		}

		public ITagMapping TagMapping
		{
			get { return _TagMapping; }
		}

        [System.Diagnostics.DebuggerStepThrough]
		public void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

        public override string ToString()
        {
            return _Type.ToString() + "(" + _ValueExpression.ToString() + ")";
        }
	}
}
