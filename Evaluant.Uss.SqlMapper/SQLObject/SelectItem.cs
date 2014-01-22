using System;
using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de SelectItem.
	/// </summary>
	public class SelectItem : Column
	{
		private string _ColumnAlias = String.Empty;

		public SelectItem(ITagMapping tagMapping, string column_name, string column_alias) : this(tagMapping, String.Empty, column_name, column_alias)
		{
			//
			// TODO : ajoutez ici la logique du constructeur
			//
		}

		public SelectItem(ITagMapping tagMapping, string table_alias, string column_name, string column_alias) : base (tagMapping, table_alias, column_name)
		{
			_ColumnAlias = column_alias;
		}

		public string ColumnAlias
		{
			get { return _ColumnAlias; }
		}

		public override void Accept(ISQLVisitor visitor)
		{
			base.Accept(visitor);
			visitor.Visit(this);
		}
	}

	public class SelectItemCollection: CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the SelectItemCollection class.
		/// </summary>
		public SelectItemCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the SelectItemCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new SelectItemCollection.
		/// </param>
		public SelectItemCollection(SelectItem[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the SelectItemCollection class, containing elements
		/// copied from another instance of SelectItemCollection
		/// </summary>
		/// <param name="items">
		/// The SelectItemCollection whose elements are to be added to the new SelectItemCollection.
		/// </param>
		public SelectItemCollection(SelectItemCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this SelectItemCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this SelectItemCollection.
		/// </param>
		public virtual void AddRange(SelectItem[] items)
		{
			foreach (SelectItem item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another SelectItemCollection to the end of this SelectItemCollection.
		/// </summary>
		/// <param name="items">
		/// The SelectItemCollection whose elements are to be added to the end of this SelectItemCollection.
		/// </param>
		public virtual void AddRange(SelectItemCollection items)
		{
			foreach (SelectItem item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type SelectItem to the end of this SelectItemCollection.
		/// </summary>
		/// <param name="value">
		/// The SelectItem to be added to the end of this SelectItemCollection.
		/// </param>
		public virtual void Add(SelectItem value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic SelectItem value is in this SelectItemCollection.
		/// </summary>
		/// <param name="value">
		/// The SelectItem value to locate in this SelectItemCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this SelectItemCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(SelectItem value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this SelectItemCollection
		/// </summary>
		/// <param name="value">
		/// The SelectItem value to locate in the SelectItemCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(SelectItem value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the SelectItemCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the SelectItem is to be inserted.
		/// </param>
		/// <param name="value">
		/// The SelectItem to insert.
		/// </param>
		public virtual void Insert(int index, SelectItem value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the SelectItem at the given index in this SelectItemCollection.
		/// </summary>
		public virtual SelectItem this[int index]
		{
			get
			{
				return (SelectItem) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific SelectItem from this SelectItemCollection.
		/// </summary>
		/// <param name="value">
		/// The SelectItem value to remove from this SelectItemCollection.
		/// </param>
		public virtual void Remove(SelectItem value)
		{
			this.List.Remove(value);
		}

		/// <summary>
		/// Type-specific enumeration class, used by SelectItemCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: IEnumerator
		{
			private IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(SelectItemCollection collection)
			{
				this.wrapped = ((CollectionBase)collection).GetEnumerator();
			}
			/// <summary>
			/// 
			/// </summary>
			public SelectItem Current
			{
				get
				{
					return (SelectItem) (this.wrapped.Current);
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return (SelectItem) (this.wrapped.Current);
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
		/// Returns an enumerator that can iterate through the elements of this SelectItemCollection.
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
