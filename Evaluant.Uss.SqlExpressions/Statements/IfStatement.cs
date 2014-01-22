using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Statements
{
    public class IfStatement : DbStatement
    {
        public IfStatement(IDbExpression condition, IDbStatement then, IDbStatement @else)
        {
            this.Condition = condition;
            this.Then = then;
            this.Else = @else;
        }

        public IDbExpression Condition { get; set; }

        public IDbStatement Then { get; set; }

        public IDbStatement Else { get; set; }

        public override System.Data.StatementType StatementType
        {
            get { return System.Data.StatementType.Batch; }
        }

        public override DbExpressionType DbExpressionType
        {
            get { return SqlExpressions.DbExpressionType.If; }
        }
    }

    //public abstract class IfExistsStatement : DbStatement
    //{
    //    public IfExistsStatement(string item, IDbStatement statement)
    //    {
    //        Item = item;
    //        this.Statement = statement;
    //    }

    //    public override System.Data.StatementType StatementType
    //    {
    //        get { return System.Data.StatementType.Batch; }
    //    }

    //    public IDbStatement Statement { get; set; }

    //    public string Item { get; set; }
    //}

    //public class IfTableExistsStatement : IfExistsStatement
    //{
    //    public IfTableExistsStatement(string item, IDbStatement statement)
    //        : base(item, statement)
    //    {
    //    }

    //    public override DbExpressionType DbExpressionType
    //    {
    //        get { return SqlExpressions.DbExpressionType.IfTableExists; }
    //    }
    //}

    //public class IfForeignKeyExistsStatement : IfExistsStatement
    //{
    //    public IfForeignKeyExistsStatement(string item, string parentItem, IDbStatement statement)
    //        : base(item, statement)
    //    {
    //        ParentItem = parentItem;
    //    }

    //    public override DbExpressionType DbExpressionType
    //    {
    //        get { return SqlExpressions.DbExpressionType.IfFKExists; }
    //    }

    //    public string ParentItem { get; set; }
    //}
}
