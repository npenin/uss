using System;
using System.Collections;
using System.Data;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;
using System.Text;

namespace SQLObject
{
    /// <summary>
    /// Description résumée de LogicOperator.
    /// </summary>
    public interface ILogicExpression : ISQLExpression
    {
        new ITagMapping TagMapping
        {
            get;
            set;
        }
    }

    public abstract class LogicExpression : ILogicExpression
    {
        private ITagMapping _TagMapping;

        public ITagMapping TagMapping
        {
            get { return _TagMapping; }
            set { _TagMapping = value; }
        }

        public abstract void Accept(ISQLVisitor visitor);

    }



    public class LogicExpressionCollection : CollectionBase, ILogicExpression
    {
        /// <summary>
        /// Initializes a new empty instance of the LogicExpressionCollection class.
        /// </summary>
        public LogicExpressionCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the LogicExpressionCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new LogicExpressionCollection.
        /// </param>
        public LogicExpressionCollection(ILogicExpression[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the LogicExpressionCollection class, containing elements
        /// copied from another instance of LogicExpressionCollection
        /// </summary>
        /// <param name="items">
        /// The LogicExpressionCollection whose elements are to be added to the new LogicExpressionCollection.
        /// </param>
        public LogicExpressionCollection(LogicExpressionCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this LogicExpressionCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this LogicExpressionCollection.
        /// </param>
        public virtual void AddRange(ILogicExpression[] items)
        {
            foreach (ILogicExpression item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another LogicExpressionCollection to the end of this LogicExpressionCollection.
        /// </summary>
        /// <param name="items">
        /// The LogicExpressionCollection whose elements are to be added to the end of this LogicExpressionCollection.
        /// </param>
        public virtual void AddRange(LogicExpressionCollection items)
        {
            foreach (ILogicExpression item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type LogicExpression to the end of this LogicExpressionCollection.
        /// </summary>
        /// <param name="value">
        /// The LogicExpression to be added to the end of this LogicExpressionCollection.
        /// </param>
        public virtual void Add(ILogicExpression value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic LogicExpression value is in this LogicExpressionCollection.
        /// </summary>
        /// <param name="value">
        /// The LogicExpression value to locate in this LogicExpressionCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this LogicExpressionCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(ILogicExpression value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this LogicExpressionCollection
        /// </summary>
        /// <param name="value">
        /// The LogicExpression value to locate in the LogicExpressionCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(ILogicExpression value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the LogicExpressionCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the LogicExpression is to be inserted.
        /// </param>
        /// <param name="value">
        /// The LogicExpression to insert.
        /// </param>
        public virtual void Insert(int index, ILogicExpression value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the LogicExpression at the given index in this LogicExpressionCollection.
        /// </summary>
        public virtual ILogicExpression this[int index]
        {
            get
            {
                return (ILogicExpression)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific LogicExpression from this LogicExpressionCollection.
        /// </summary>
        /// <param name="value">
        /// The LogicExpression value to remove from this LogicExpressionCollection.
        /// </param>
        public virtual void Remove(ILogicExpression value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by LogicExpressionCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : IEnumerator
        {
            private IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(LogicExpressionCollection collection)
            {
                this.wrapped = ((CollectionBase)collection).GetEnumerator();
            }
            /// <summary>
            /// 
            /// </summary>
            public ILogicExpression Current
            {
                get
                {
                    return (ILogicExpression)(this.wrapped.Current);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (ILogicExpression)(this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this LogicExpressionCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool notIsFirst = false;
            foreach (ILogicExpression exp in List)
            {
                if (notIsFirst)
                    sb.Append(", ");
                else
                    notIsFirst = true;
                sb.Append(exp);
            }
            return sb.ToString();
        }

        #region ILogicExpression Membres

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
