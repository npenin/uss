using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de SelectStatement.
	/// </summary>
	public class SelectStatement : Table, ISQLExpression
    {
        #region Members

        private ExpressionCollection _SelectList;
		private bool _IsDistinct;
		private bool _SelectedAllColumns;
        private int _Limit;
        private int _Offset;

		private FromClause _FromClause;
		private WhereClause _WhereClause;
		private OrderByClause _OrderByClause;

        #endregion

        #region Ctor

        public SelectStatement(ITagMapping tag): base(tag)
		{
            SelectList = new ExpressionCollection();
            _IsDistinct = false;
            _SelectedAllColumns = false;
            _Limit = 0;
            _Offset = 0;

            _FromClause = new FromClause();
            _WhereClause = new WhereClause(BinaryLogicOperator.And);
            _OrderByClause = new OrderByClause(this);
        }

        #endregion

        #region Properties

        public ExpressionCollection SelectList
        {
            get { return _SelectList; }
            set { _SelectList = value; }
        }

        /// <summary>
        /// Specify the number of row returned by the query, if 0 or less: no limit
        /// </summary>
        public int Limit
        {
            get { return _Limit; }
            set { _Limit = value; }
        }

        /// <summary>
        /// Specify the first row number to return b y the query, if 0 or 1: start with the first
        /// </summary>
        public int Offset
        {
            get { return _Offset; }
            set { _Offset = value; }
        }
	

        public bool IsDistinct
		{
			get { return _IsDistinct; }
			set { _IsDistinct = value; }
		}

		public bool SelectedAllColumns
		{
			get { return _SelectedAllColumns; }
			set { _SelectedAllColumns = value; }
		}

		public FromClause FromClause
		{
			get { return _FromClause; }
			set { _FromClause = value; }
		}

		public WhereClause WhereClause
		{
            [System.Diagnostics.DebuggerStepThrough]
			get { return _WhereClause; }
			set { _WhereClause = value; }
		}

		public OrderByClause OrderByClause
		{
			get { return _OrderByClause; }
			set { _OrderByClause = value; }
        }

        #endregion

        #region Visitor

        [System.Diagnostics.DebuggerStepThrough]
        public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
        }

        #endregion

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("SELECT ");
            if (SelectList.Count > 0)
            {
                foreach (ISQLExpression e in SelectList)
                    sb.Append(e).Append(", ");
                sb.Remove(sb.Length - 2, 2);
            }
            if (FromClause.Count > 0)
            {
                sb.Append(" FROM ");
                foreach (ISQLExpression e in FromClause)
                    sb.Append(e).Append(", ");
                sb.Remove(sb.Length - 2, 2);
            }
            if (WhereClause.SearchCondition.Count > 0)
            {
                sb.Append("WHERE ");
                foreach (ISQLExpression e in WhereClause.SearchCondition)
                    sb.Append(e).Append(" ").Append(WhereClause.DefaultOperator);
                sb.Remove(sb.Length - WhereClause.DefaultOperator.ToString().Length, WhereClause.DefaultOperator.ToString().Length);
            }
            return sb.ToString();
        }
    }

    #region class SelectStatementCollection

    public class SelectStatementCollection: CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the SelectStatementCollection class.
		/// </summary>
		public SelectStatementCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the SelectStatementCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new SelectStatementCollection.
		/// </param>
		public SelectStatementCollection(SelectStatement[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the SelectStatementCollection class, containing elements
		/// copied from another instance of SelectStatementCollection
		/// </summary>
		/// <param name="items">
		/// The SelectStatementCollection whose elements are to be added to the new SelectStatementCollection.
		/// </param>
		public SelectStatementCollection(SelectStatementCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this SelectStatementCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this SelectStatementCollection.
		/// </param>
		public virtual void AddRange(SelectStatement[] items)
		{
			foreach (SelectStatement item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another SelectStatementCollection to the end of this SelectStatementCollection.
		/// </summary>
		/// <param name="items">
		/// The SelectStatementCollection whose elements are to be added to the end of this SelectStatementCollection.
		/// </param>
		public virtual void AddRange(SelectStatementCollection items)
		{
			foreach (SelectStatement item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type SelectStatement to the end of this SelectStatementCollection.
		/// </summary>
		/// <param name="value">
		/// The SelectStatement to be added to the end of this SelectStatementCollection.
		/// </param>
		public virtual void Add(SelectStatement value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic SelectStatement value is in this SelectStatementCollection.
		/// </summary>
		/// <param name="value">
		/// The SelectStatement value to locate in this SelectStatementCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this SelectStatementCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(SelectStatement value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this SelectStatementCollection
		/// </summary>
		/// <param name="value">
		/// The SelectStatement value to locate in the SelectStatementCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(SelectStatement value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the SelectStatementCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the SelectStatement is to be inserted.
		/// </param>
		/// <param name="value">
		/// The SelectStatement to insert.
		/// </param>
		public virtual void Insert(int index, SelectStatement value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the SelectStatement at the given index in this SelectStatementCollection.
		/// </summary>
		public virtual SelectStatement this[int index]
		{
            [System.Diagnostics.DebuggerStepThrough]
            get
			{
				return (SelectStatement) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific SelectStatement from this SelectStatementCollection.
		/// </summary>
		/// <param name="value">
		/// The SelectStatement value to remove from this SelectStatementCollection.
		/// </param>
		public virtual void Remove(SelectStatement value)
		{
			this.List.Remove(value);
		}

		/// <summary>
		/// Type-specific enumeration class, used by SelectStatementCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: IEnumerator
		{
			private IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(SelectStatementCollection collection)
			{
				this.wrapped = ((CollectionBase)collection).GetEnumerator();
			}
			/// <summary>
			/// 
			/// </summary>
			public SelectStatement Current
			{
				get
				{
					return (SelectStatement) (this.wrapped.Current);
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return (SelectStatement) (this.wrapped.Current);
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
		/// Returns an enumerator that can iterate through the elements of this SelectStatementCollection.
		/// </summary>
		/// <returns>
		/// An object that implements System.Collections.IEnumerator.
		/// </returns>        
		public new virtual Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
    }

    #endregion
}
