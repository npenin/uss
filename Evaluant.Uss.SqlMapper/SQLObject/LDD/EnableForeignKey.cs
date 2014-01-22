using System;
using System.Collections;

using SQLObject.Renderer;
using SQLObject;

namespace Evaluant.Uss.SqlMapper.SqlObjectModel.LDD
{
	public class EnableForeignKey : ISQLExpression
	{

        public EnableForeignKey(DBDialect.ForeignKeyScope scope)
		{
            _Scope = scope;
		}

        private DBDialect.ForeignKeyScope _Scope;

        public DBDialect.ForeignKeyScope Scope
        {
            get { return _Scope; }
            set { _Scope = value; }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _Table;

        public string Table
        {
            get { return _Table; }
            set { _Table = value; }
        }


        #region ISQLExpression Members

        private ITagMapping _TagMapping;

        public ITagMapping TagMapping
        {
            get { return _TagMapping; }
			set { _TagMapping = value; }
        }

        public void Accept(ISQLVisitor visitor)
        {
            visitor.Visit(this);
        }

        #endregion
    }
}
