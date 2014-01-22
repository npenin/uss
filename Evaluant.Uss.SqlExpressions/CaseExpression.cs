using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class CaseExpression : AliasedExpression
    {
        public IDbExpression Test { get; set; }
        public IDbExpression DefaultResult { get; set; }
        public CaseTestExpression[] CaseTests { get; set; }

        public CaseExpression(TableAlias alias, IDbExpression test, IDbExpression defaultResult, params CaseTestExpression[] caseTests)
            : base(alias)
        {
            Test = test;
            DefaultResult = defaultResult;
            CaseTests = caseTests;
        }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Case; }
        }
    }

    public class CaseTestExpression
    {
        public CaseTestExpression(Expression testExpression, Expression testResult)
        {
            TestExpression = testExpression;
            TestResult = testResult;
        }

        public Expression TestExpression { get; set; }
        public Expression TestResult { get; set; }
    }
}
