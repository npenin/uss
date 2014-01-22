using System;
using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de Column.
	/// </summary>
	public class OrderByClauseColumn : Column
	{
        private bool _Desc = false;
        public bool Desc
        {
            get { return _Desc; }
            set { _Desc = value; }
        }

        public OrderByClauseColumn(string columnName)
            : this(columnName, false)
        { 
        
        }

        public OrderByClauseColumn(string columName, bool desc)
            : this(string.Empty, columName, desc)
        {

        }

        public OrderByClauseColumn(string tableName, string columnName, bool desc)
            : base(null, tableName, columnName)
        {
            _Desc = desc;
        }

        public override void Accept(ISQLVisitor visitor)
        {
            visitor.Visit(this);
        }
	}

    public class OrderByClauseColumnCollection : CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ColumnCollection class.
        /// </summary>
        public OrderByClauseColumnCollection()
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
        public OrderByClauseColumnCollection(OrderByClauseColumn[] items)
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
        public OrderByClauseColumnCollection(OrderByClauseColumnCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this ColumnCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this ColumnCollection.
        /// </param>
        public virtual void AddRange(OrderByClauseColumn[] items)
        {
            foreach (OrderByClauseColumn item in items)
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
        public virtual void AddRange(OrderByClauseColumnCollection items)
        {
            foreach (OrderByClauseColumn item in items)
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
        public virtual void Add(OrderByClauseColumn value)
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
        public virtual bool Contains(OrderByClauseColumn value)
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
        public virtual int IndexOf(OrderByClauseColumn value)
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
        public virtual void Insert(int index, OrderByClauseColumn value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the Column at the given index in this ColumnCollection.
        /// </summary>
        public virtual OrderByClauseColumn this[int index]
        {
            get
            {
                return (OrderByClauseColumn)this.List[index];
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
        public virtual void Remove(OrderByClauseColumn value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by ColumnCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : IEnumerator
        {
            private IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(OrderByClauseColumnCollection collection)
            {
                this.wrapped = ((CollectionBase)collection).GetEnumerator();
            }
            /// <summary>
            /// 
            /// </summary>
            public OrderByClauseColumn Current
            {
                get
                {
                    return (OrderByClauseColumn)(this.wrapped.Current);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (OrderByClauseColumn)(this.wrapped.Current);
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
