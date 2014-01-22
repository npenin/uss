using System;
using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de ISQLExpression.
	/// </summary>
	public interface ISQLExpression
	{
		ITagMapping TagMapping {get;}

        [System.Diagnostics.DebuggerStepThrough]
		void Accept(ISQLVisitor visitor);
	}

	public class ExpressionCollection: CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the ExpressionCollection class.
		/// </summary>
		public ExpressionCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the ExpressionCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new ExpressionCollection.
		/// </param>
		public ExpressionCollection(ISQLExpression[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the ExpressionCollection class, containing elements
		/// copied from another instance of ExpressionCollection
		/// </summary>
		/// <param name="items">
		/// The ExpressionCollection whose elements are to be added to the new ExpressionCollection.
		/// </param>
		public ExpressionCollection(ExpressionCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this ExpressionCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this ExpressionCollection.
		/// </param>
		public virtual void AddRange(ISQLExpression[] items)
		{
			foreach (ISQLExpression item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another ExpressionCollection to the end of this ExpressionCollection.
		/// </summary>
		/// <param name="items">
		/// The ExpressionCollection whose elements are to be added to the end of this ExpressionCollection.
		/// </param>
		public virtual void AddRange(ExpressionCollection items)
		{
			foreach (ISQLExpression item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type Expression to the end of this ExpressionCollection.
		/// </summary>
		/// <param name="value">
		/// The Expression to be added to the end of this ExpressionCollection.
		/// </param>
		public virtual void Add(ISQLExpression value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic Expression value is in this ExpressionCollection.
		/// </summary>
		/// <param name="value">
		/// The Expression value to locate in this ExpressionCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this ExpressionCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(ISQLExpression value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this ExpressionCollection
		/// </summary>
		/// <param name="value">
		/// The Expression value to locate in the ExpressionCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(ISQLExpression value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the ExpressionCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the Expression is to be inserted.
		/// </param>
		/// <param name="value">
		/// The Expression to insert.
		/// </param>
		public virtual void Insert(int index, ISQLExpression value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the Expression at the given index in this ExpressionCollection.
		/// </summary>
		public virtual ISQLExpression this[int index]
		{
			get
			{
				return (ISQLExpression) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific Expression from this ExpressionCollection.
		/// </summary>
		/// <param name="value">
		/// The Expression value to remove from this ExpressionCollection.
		/// </param>
		public virtual void Remove(ISQLExpression value)
		{
			this.List.Remove(value);
		}

		/// <summary>
		/// Type-specific enumeration class, used by ExpressionCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: IEnumerator
		{
			private IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(ExpressionCollection collection)
			{
				this.wrapped = ((CollectionBase)collection).GetEnumerator();
			}
			/// <summary>
			/// 
			/// </summary>
			public ISQLExpression Current
			{
				get
				{
					return (ISQLExpression) (this.wrapped.Current);
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return (ISQLExpression) (this.wrapped.Current);
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
		/// Returns an enumerator that can iterate through the elements of this ExpressionCollection.
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
