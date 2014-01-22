using System;
using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de Table.
	/// </summary>
	public abstract class Table : ISQLExpression
	{
		private ITagMapping _TagMapping;
		private string _TableAlias;

		public Table(ITagMapping tag) : this(tag, String.Empty)
		{
		}

		public Table(ITagMapping tag, string tableAlias)
		{
			_TableAlias = tableAlias;
			_TagMapping = tag;
		}

		public string TableAlias
		{
			get { return _TableAlias; }
			set { _TableAlias = value; }
		}

		public ITagMapping TagMapping
		{
			get { return _TagMapping; }
		}

		public abstract void Accept(ISQLVisitor visitor);

	}

	public class TableCollection: CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the TableCollection class.
		/// </summary>
		public TableCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the TableCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new TableCollection.
		/// </param>
		public TableCollection(Table[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the TableCollection class, containing elements
		/// copied from another instance of TableCollection
		/// </summary>
		/// <param name="items">
		/// The TableCollection whose elements are to be added to the new TableCollection.
		/// </param>
		public TableCollection(TableCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this TableCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this TableCollection.
		/// </param>
		public virtual void AddRange(Table[] items)
		{
			foreach (Table item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another TableCollection to the end of this TableCollection.
		/// </summary>
		/// <param name="items">
		/// The TableCollection whose elements are to be added to the end of this TableCollection.
		/// </param>
		public virtual void AddRange(TableCollection items)
		{
			foreach (Table item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type Table to the end of this TableCollection.
		/// </summary>
		/// <param name="value">
		/// The Table to be added to the end of this TableCollection.
		/// </param>
		public virtual void Add(Table value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic Table value is in this TableCollection.
		/// </summary>
		/// <param name="value">
		/// The Table value to locate in this TableCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this TableCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(Table value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this TableCollection
		/// </summary>
		/// <param name="value">
		/// The Table value to locate in the TableCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(Table value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the TableCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the Table is to be inserted.
		/// </param>
		/// <param name="value">
		/// The Table to insert.
		/// </param>
		public virtual void Insert(int index, Table value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the Table at the given index in this TableCollection.
		/// </summary>
		public virtual Table this[int index]
		{
			get
			{
				return (Table) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific Table from this TableCollection.
		/// </summary>
		/// <param name="value">
		/// The Table value to remove from this TableCollection.
		/// </param>
		public virtual void Remove(Table value)
		{
			this.List.Remove(value);
		}

		/// <summary>
		/// Type-specific enumeration class, used by TableCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: IEnumerator
		{
			private IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(TableCollection collection)
			{
				this.wrapped = ((CollectionBase)collection).GetEnumerator();
			}
			/// <summary>
			/// 
			/// </summary>
			public Table Current
			{
				get
				{
					return (Table) (this.wrapped.Current);
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return (Table) (this.wrapped.Current);
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
		/// Returns an enumerator that can iterate through the elements of this TableCollection.
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
