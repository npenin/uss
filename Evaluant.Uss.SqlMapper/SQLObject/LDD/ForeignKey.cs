using System;
using System.Collections;

using SQLObject.Renderer;

namespace Evaluant.Uss.SqlMapper.SqlObjectModel.LDD
{
	/// <summary>
	/// Description résumée de ForeignKeyConstraint.
	/// </summary>
	public class ForeignKey
	{
        private string _name;
		private string[] _foreignKeys;
		private string _parentTable;
		private string[] _referenceKeys;

		public ForeignKey(string name, string[] foreignKeys, string parentTable, string[] referenceKeys)
		{
			_name = name;
			_foreignKeys = foreignKeys;
			_parentTable = parentTable;
			_referenceKeys = referenceKeys;
		}

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string[] ForeignKeys
        {
            get { return _foreignKeys; }
            set { _foreignKeys = value; }
        }

        public string ParentTable
        {
            get { return _parentTable; }
            set { _parentTable = value; }
        }

        public string[] ReferenceKeys
        {
            get { return _referenceKeys; }
            set { _referenceKeys = value; }
        }

	}
}
