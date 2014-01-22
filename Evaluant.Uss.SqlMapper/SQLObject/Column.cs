using System;
using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de Column.
	/// </summary>
	public class Column : ISQLExpression
	{
		private ITagMapping _TagMapping;
		
		private string _ColumnName = String.Empty;
		private string _TableName = String.Empty;
		private string _Alias = string.Empty;

		public string Alias
		{
			get { return _Alias; }
			set { _Alias = value; }
		}

		public Column(ITagMapping tagMapping, string column_name)
		{
			_TagMapping = tagMapping;
			_ColumnName = column_name;
		}

		public Column(ITagMapping tagMapping, string table_name, string column_name, string alias)
		{
			_TagMapping = tagMapping;
			_ColumnName = column_name;
			_TableName = table_name;
			_Alias = alias;
		}
		public Column(ITagMapping tagMapping, string table_name, string column_name)
		{
			_TagMapping = tagMapping;
			_ColumnName = column_name;
			_TableName = table_name;
		}

		public string ColumnName
		{
			get { return _ColumnName; }
            set { _ColumnName = value;}
		}

		public ITagMapping TagMapping
		{
			get { return _TagMapping; }
		}

		public string TableName
		{
			get { return _TableName; }
			set { _TableName = value; }
		}

        /// <summary>
        /// Gets the name of the column in the select.
        /// </summary>
        /// <returns></returns>
        public string GetSelectName()
        {
            return _Alias == null | Alias == String.Empty ? _ColumnName : _Alias;
        }

		public virtual void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

        public override string ToString()
        {
            return TableName + "." + ColumnName;
        }
	}

    public class MultipledKey : Column
    {
        public MultipledKey(ITagMapping tagMapping)
            : base(tagMapping, "")
        { }
 
        protected ColumnCollection collection = new ColumnCollection();
        public ColumnCollection Collection
        {
            get { return collection; }
            set { collection = value; }
        }

    }

	public class ColumnCollection: CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the ColumnCollection class.
		/// </summary>
		public ColumnCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the ColumnCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new ColumnCollection.
		/// </param>
		public ColumnCollection(Column[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the ColumnCollection class, containing elements
		/// copied from another instance of ColumnCollection
		/// </summary>
		/// <param name="items">
		/// The ColumnCollection whose elements are to be added to the new ColumnCollection.
		/// </param>
		public ColumnCollection(ColumnCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this ColumnCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this ColumnCollection.
		/// </param>
		public virtual void AddRange(Column[] items)
		{
			foreach (Column item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another ColumnCollection to the end of this ColumnCollection.
		/// </summary>
		/// <param name="items">
		/// The ColumnCollection whose elements are to be added to the end of this ColumnCollection.
		/// </param>
		public virtual void AddRange(ColumnCollection items)
		{
			foreach (Column item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type Column to the end of this ColumnCollection.
		/// </summary>
		/// <param name="value">
		/// The Column to be added to the end of this ColumnCollection.
		/// </param>
		public virtual void Add(Column value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic Column value is in this ColumnCollection.
		/// </summary>
		/// <param name="value">
		/// The Column value to locate in this ColumnCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this ColumnCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(Column value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this ColumnCollection
		/// </summary>
		/// <param name="value">
		/// The Column value to locate in the ColumnCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(Column value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the ColumnCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the Column is to be inserted.
		/// </param>
		/// <param name="value">
		/// The Column to insert.
		/// </param>
		public virtual void Insert(int index, Column value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the Column at the given index in this ColumnCollection.
		/// </summary>
		public virtual Column this[int index]
		{
			get
			{
				return (Column) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific Column from this ColumnCollection.
		/// </summary>
		/// <param name="value">
		/// The Column value to remove from this ColumnCollection.
		/// </param>
		public virtual void Remove(Column value)
		{
			this.List.Remove(value);
		}

		/// <summary>
		/// Type-specific enumeration class, used by ColumnCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: IEnumerator
		{
			private IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(ColumnCollection collection)
			{
				this.wrapped = ((CollectionBase)collection).GetEnumerator();
			}
			/// <summary>
			/// 
			/// </summary>
			public Column Current
			{
				get
				{
					return (Column) (this.wrapped.Current);
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return (Column) (this.wrapped.Current);
				}
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public bool MoveNext()
			{
				return this.wrapped.MoveNext();
			}

			/// <summary>
			/// 
			/// </summary>
			public void Reset()
			{
				this.wrapped.Reset();
			}
		}

		/// <summary>
		/// Returns an enumerator that can iterate through the elements of this ColumnCollection.
		/// </summary>
		/// <returns>
		/// An object that implements System.Collections.IEnumerator.
		/// </returns>        
		public new virtual Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

	}
}
