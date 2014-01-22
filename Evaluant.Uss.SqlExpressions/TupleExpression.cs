using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class TupleExpression : DbExpression, ICollection<Expression>
    {
        public TupleExpression()
            : this(new List<Expression>())
        {

        }

        public TupleExpression(IEnumerable<Expression> expressions)
            : this(new List<Expression>(expressions))
        {

        }

        public TupleExpression(ICollection<Expression> expressions)
        {
            this.expressions = expressions;
        }

        ICollection<Expression> expressions;

        #region ICollection<Expression> Members

        public void Add(Expression item)
        {
            expressions.Add(item);
        }

        public void Clear()
        {
            expressions.Clear();
        }

        public bool Contains(Expression item)
        {
            return expressions.Contains(item);
        }

        public void CopyTo(Expression[] array, int arrayIndex)
        {
            expressions.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return expressions.Count; }
        }

        public bool IsReadOnly
        {
            get { return expressions.IsReadOnly; }
        }

        public bool Remove(Expression item)
        {
            return expressions.Remove(item);
        }

        #endregion

        #region IEnumerable<Expression> Members

        public IEnumerator<Expression> GetEnumerator()
        {
            return expressions.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDbExpression Members

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Projection; }
        }

        #endregion
    }
}
