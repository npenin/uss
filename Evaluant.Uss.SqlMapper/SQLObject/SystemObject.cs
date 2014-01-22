using System;
using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Class used to define non user column but system object (ie: rownum in oracle)
	/// </summary>
    public class SystemObject : Column
    {
        public SystemObject(string objName)
            : this(objName, String.Empty)
        {}

        public SystemObject(string objName, string alias)
            : this(String.Empty, objName, alias)
        {}

        public SystemObject(string tableName, string objName, string alias) 
            : base(null, tableName, objName, alias)
        {}

        public override void Accept(ISQLVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
