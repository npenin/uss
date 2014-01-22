using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Evaluant.Uss.SqlExpressions
{
    public class FromClause : ICollection<IAliasedExpression>
    {
        List<IAliasedExpression> tables;

        public FromClause(IEnumerable<IAliasedExpression> froms)
        {
            tables = new List<IAliasedExpression>(froms);
        }

        public FromClause(params IAliasedExpression[] froms)
        {
            tables = new List<IAliasedExpression>(froms);
        }

        public IAliasedExpression this[int index]
        {
            get { return tables[index]; }
        }

        public IAliasedExpression[] ToArray()
        {
            return tables.ToArray();
        }

        #region ICollection<TableExpression> Members

        void ICollection<IAliasedExpression>.Add(IAliasedExpression item)
        {
            throw new NotSupportedException();
        }

        void ICollection<IAliasedExpression>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(IAliasedExpression table)
        {
            if (tables.Contains(table))
                return true;
            foreach (TableExpression item in tables)
            {
                if (item.Alias == table.Alias && item.DbExpressionType == table.DbExpressionType)
                    return true;
            }
            return false;
        }

        public void CopyTo(IAliasedExpression[] array, int arrayIndex)
        {
            tables.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return tables.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<IAliasedExpression>.Remove(IAliasedExpression item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<TableExpression> Members

        public IEnumerator<IAliasedExpression> GetEnumerator()
        {
            return tables.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return tables.GetEnumerator();
        }

        #endregion
    }
}
