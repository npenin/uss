using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de InsertCommand.
	/// </summary>
	public class InsertCommand : SQLCommand
	{
		//INSERT [INTO] nom_de_la_table_cible [(liste_des_colonnes_visées)] 
		//{VALUES (liste_des_valeurs) | requête_select | DEFAULT VALUES }
		
		private ColumnValueCollection _ColumnValueCollection = new ColumnValueCollection();

		private SelectStatement _SelectStatement = null;
		private bool _DefaultValues = false;

		public InsertCommand(ITagMapping tag, string table_name) : base (tag, table_name)
		{
			
		}

		public InsertCommand(ITagMapping tag, string table_name, SelectStatement select_command)  : base (tag, table_name)
		{
			_SelectStatement = select_command;
		}

		public ColumnValueCollection ColumnValueCollection
		{
			get { return _ColumnValueCollection; }
			set { _ColumnValueCollection = value; }
		}

		public SelectStatement SelectStatement
		{
			get { return _SelectStatement; }
		}

		public bool DefaultValues
		{
			get { return _DefaultValues; }
			set { _DefaultValues = value; }
		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class ColumnValueCollection : ArrayList
	{
		public int Add(string key, ISQLExpression value)
		{
			return base.Add (new DictionaryEntry(key, value));
		}

		public bool ContainsKey(string key)
		{
			foreach(DictionaryEntry entry in this)
			{
				if ((string)entry.Key == key)
					return true;
			}
			return false;
		}
	}
}
