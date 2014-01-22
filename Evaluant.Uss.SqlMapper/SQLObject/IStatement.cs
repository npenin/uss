using System;
using SQLObject.Renderer;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de IStatement.
	/// </summary>
	public interface IStatement
	{
		void Accept(ISQLVisitor visitor);
	}
}
