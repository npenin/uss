using System;
using SQLObject.Renderer;

namespace SQLObject
{
	/// <summary>
	/// Description r�sum�e de IStatement.
	/// </summary>
	public interface IStatement
	{
		void Accept(ISQLVisitor visitor);
	}
}
