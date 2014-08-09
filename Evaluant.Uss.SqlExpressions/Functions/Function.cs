using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public abstract class Function : AliasedExpression, IDbExpression
    {
        private string p;
        private IDbStatement statement;

        public Function(Identifier identifier, params Expression[] parameters)
            : this(null, identifier, parameters)
        {
        }

        public Function(TableAlias alias, Identifier identifier, params Expression[] parameters)
            : base(alias)
        {
            this.MethodName = identifier;
            this.Parameters = parameters;
        }

        public Function(string identifier, params Expression[] parameters)
            : this(new Identifier(identifier), parameters)
        {
        }

        public Identifier MethodName { get; set; }

        public Expression[] Parameters { get; set; }

        public abstract FunctionType FunctionType { get; }

        #region IDbExpression Members

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Function; }
        }

        #endregion
    }

    public enum FunctionType
    {
        Cast,
        Aggregate,
        Exists,
        In,
        IsNull,
        Like,
        RowNumber,
        Not,
        Exec,
        Lower,
        Upper,
        DatePart,
        DateAdd,
    }
}
