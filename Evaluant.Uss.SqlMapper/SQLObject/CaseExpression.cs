using System;
using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
    #region class CaseExpression
    /// <summary>
	/// Case structur for conditionnal tests
	/// </summary>
	public class CaseExpression : Column
    {
        #region Members

        private ISQLExpression _ExpressionToEval;
        private ISQLExpression _DefaultResult;
        private CaseTestCollection _TestExpressions;

        #endregion

        #region Ctor

        public CaseExpression(ITagMapping tagMapping, ISQLExpression expressiontToEval, string column_alias) : base(tagMapping, String.Empty, String.Empty, column_alias)
        {
            _ExpressionToEval = expressiontToEval;
            _TestExpressions = new CaseTestCollection();
        }

        public CaseExpression(ITagMapping tagMapping, string column_alias) : this(tagMapping, null, column_alias)
		{
        }

        #endregion

        #region Properties

        public ISQLExpression ExpressionToEval
        {
            get { return _ExpressionToEval; }
            set { _ExpressionToEval = value; }
        }

        public ISQLExpression DefaultResult
        {
            get { return _DefaultResult; }
            set { _DefaultResult = value; }
        }

        public CaseTestCollection TestExpressions
        {
            get { return _TestExpressions; }
            set { _TestExpressions = value; }
        }

        #endregion

        #region Visitor

        public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
        }

        #endregion
    }

    #endregion

    #region class CaseTest

    public class CaseTest
    {
        #region Members

        private ISQLExpression _TestExpression;
        private ISQLExpression _TestResult;

        #endregion

        #region Ctor

        public CaseTest(ISQLExpression testExpression, ISQLExpression testResult)
        {
            _TestExpression = testExpression;
            _TestResult = testResult;
        }

        #endregion

        #region Properties

        public ISQLExpression TestExpression
        {
            get { return _TestExpression; }
            set { _TestExpression = value; }
        }
        public ISQLExpression TestResult
        {
            get { return _TestResult; }
            set { _TestResult = value; }
        }

        #endregion
    }

    #endregion

    #region class CaseTestCollection

    public class CaseTestCollection : CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ExpressionCollection class.
        /// </summary>
        public CaseTestCollection()
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
        public CaseTestCollection(CaseTest[] items)
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
        public CaseTestCollection(CaseTestCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this ExpressionCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this ExpressionCollection.
        /// </param>
        public virtual void AddRange(CaseTest[] items)
        {
            foreach (CaseTest item in items)
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
        public virtual void AddRange(CaseTestCollection items)
        {
            foreach (CaseTest item in items)
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
        public virtual void Add(CaseTest value)
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
        public virtual bool Contains(CaseTest value)
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
        public virtual int IndexOf(CaseTest value)
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
        public virtual void Insert(int index, CaseTest value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the Expression at the given index in this ExpressionCollection.
        /// </summary>
        public virtual CaseTest this[int index]
        {
            get
            {
                return (CaseTest)this.List[index];
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
        public virtual void Remove(CaseTest value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by ExpressionCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : IEnumerator
        {
            private IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(CaseTestCollection collection)
            {
                this.wrapped = ((CollectionBase)collection).GetEnumerator();
            }
            /// <summary>
            /// 
            /// </summary>
            public CaseTest Current
            {
                get
                {
                    return (CaseTest)(this.wrapped.Current);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (CaseTest)(this.wrapped.Current);
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

    #endregion
}
