using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;
using System.IO;
using System.Diagnostics;
using Evaluant.Uss.SqlExpressions.Statements;

namespace Evaluant.Uss.SqlExpressions.Visitors
{
    public class DbExpressionWriter : DbExpressionVisitor
    {
#if DEBUG
        public DbExpressionWriter()
            : this("[", "]")
        {
        }
#endif

        public override IAliasedExpression Visit(Not item)
        {
            writer.Write("NOT(");
            base.Visit(item);
            writer.Write(")");
            return item;
        }

        public override IAliasedExpression Visit(Functions.Exec item)
        {
            writer.Write(item.MethodName);
            writer.Write("('");
            //Potential bug if a single is generated inside the parameters
            VisitArray(item.Parameters, Visit);
            writer.Write("')");
            return item;
        }

        public override IAliasedExpression Visit(RowNumber item)
        {
            Visit((WindowFunction.RankingWindowFunction)item);
            return item;
        }

        private void Visit(WindowFunction.RankingWindowFunction item)
        {
            Visit(item.MethodName);
            writer.Write("() OVER (");
            if (item.PartitionBy != null && item.PartitionBy.Count > 0)
            {
                writer.Write("PARTITION BY ");
                bool isFirst = true;
                foreach (var partitionKey in item.PartitionBy)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        writer.Write(", ");

                    Visit(partitionKey);
                }
                writer.Write(' ');
            }
            Visit(item.OverOrder);
            writer.Write(") ");
        }

        protected IDictionary<Mapping.Table, List<TableAlias>> tables = new Dictionary<Mapping.Table, List<TableAlias>>();
        protected IDictionary<TableAlias, string> alias = new Dictionary<TableAlias, string>();
        protected bool inWhere;

        string beginEscape, endEscape;

        public DbExpressionWriter(string beginEscape, string endEscape)
        {
            this.beginEscape = beginEscape;
            this.endEscape = endEscape;
        }

        public virtual string Render(IDbExpression expression)
        {
            tables.Clear();
            alias.Clear();
            using (writer = new System.IO.StringWriter())
            {
                Visit(expression);
                return writer.GetStringBuilder().ToString();
            }
        }

        public virtual string Render(NLinq.Expressions.Expression expression)
        {
            tables.Clear();
            alias.Clear();
            using (writer = new System.IO.StringWriter())
            {
                Visit(expression);
                return writer.GetStringBuilder().ToString();
            }
        }

        protected StringWriter writer;

        public void Render(NLinq.Expressions.Expression expression, StringWriter writer)
        {
            this.writer = writer;
            Visit(expression);
        }

        protected override Evaluant.NLinq.Expressions.Identifier VisitIdentifier(Evaluant.NLinq.Expressions.Identifier identifier)
        {
            writer.Write(identifier.Text);
            return base.VisitIdentifier(identifier);
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.Parameter parameter)
        {
            writer.Write("@" + parameter.Name);
            return parameter;
        }


        #region IVisitor<FromClause,FromClause> Members

        public override Evaluant.Uss.SqlExpressions.FromClause Visit(Evaluant.Uss.SqlExpressions.FromClause item)
        {
            writer.Write(" FROM ");
            VisitEnumerable(item, Visit);
            return item;
        }

        #endregion

        #region IVisitor<Aggregate,Expression> Members

        public override IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.Aggregate item)
        {
            writer.Write(item.MethodName.Text);
            writer.Write('(');
            VisitArray(item.Parameters, Visit);
            writer.Write(')');
            return item;
        }

        #endregion

        #region IVisitor<Cast,Expression> Members

        public override IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.Cast item)
        {
            return VisitFunctionPostFixed(item);
        }

        public override IAliasedExpression Visit(Functions.Lower item)
        {
            return VisitFunctionPostFixed(item);
        }

        public override IAliasedExpression Visit(Functions.Upper item)
        {
            return VisitFunctionPostFixed(item);

        }

        public override IAliasedExpression Visit(Functions.DatePart item)
        {
            return VisitFunctionPostFixed(item);
        }

        public override IAliasedExpression Visit(Functions.DateAdd item)
        {
            return VisitFunctionPostFixed(item);
        }

        protected virtual IAliasedExpression VisitFunctionPostFixed(Function item)
        {
            writer.Write(item.MethodName.Text);
            writer.Write('(');
            var isFirst = true;
            foreach (var param in item.Parameters)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.Write(", ");
                Visit(param);
            }
            writer.Write(')');
            return item;
        }

        protected virtual IAliasedExpression VisitFunctionPreFixed(Function item)
        {
            writer.Write(" ");
            writer.Write(item.MethodName.Text);
            writer.Write(" ");
            writer.Write('(');
            VisitArray(item.Parameters, Visit);
            writer.Write(')');
            return item;
        }


        #endregion

        #region IVisitor<Exists,Expression> Members

        public override IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.Exists item)
        {
            return VisitFunctionPostFixed(item);
        }

        #endregion

        #region IVisitor<InPredicate,Expression> Members

        public override IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.InPredicate item)
        {
            return VisitFunctionPreFixed(item);
        }

        #endregion

        #region IVisitor<IsNull,Expression> Members

        public override IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.IsNull item)
        {
            return VisitFunctionPostFixed(item);
        }

        #endregion

        #region IVisitor<Like,Expression> Members

        public override IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.Like item)
        {
            Visit(item.Parameters[0]);
            writer.Write(" LIKE ");
            Visit(item.Parameters[1]);
            return item;
        }

        #endregion

        #region IVisitor<SelectStatement,AliasedExpression> Members

        public override Evaluant.Uss.SqlExpressions.IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.SelectStatement item)
        {
            bool wasInWhere = inWhere;
            inWhere = false;
            bool innerSelect = writer.GetStringBuilder().Length > 0;
            if (innerSelect)
                writer.Write("( ");
            writer.Write("SELECT ");
            if (item.Distinct)
                writer.Write(" DISTINCT ");
            if (item.Top > 0)
            {
                writer.Write(" TOP ");
                writer.Write(item.Top);
            }
            if (item.Columns != null)
                VisitColumns(item.Columns);
            if (item.From != null)
                Visit(item.From);
            if (item.Where != null)
                Visit(item.Where);
            if (item.OrderBy != null)
                Visit(item.OrderBy);
            inWhere = wasInWhere;
            if (innerSelect)
            {
                writer.Write(" )");
                if (!inWhere)
                {
                    //writer.Write(" as ");
                    Visit(item.Alias);
                }
            }
            return item;
        }

        public virtual void VisitColumns(IEnumerable<IAliasedExpression> columns)
        {
            bool isFirst = true;
            foreach (var column in columns)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.Write(", ");
                Visit(column);
            }
        }

        #endregion

        public override IAliasedExpression Visit(Union item)
        {
            bool isFirst = true;
            if (item.Alias != null)
                writer.Write('(');
            foreach (var select in item.SelectStatements)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.WriteLine("UNION ALL ");
                Visit(select);
            }
            if (item.Alias != null)
            {
                writer.Write(')');
                Visit(item.Alias);
            }
            return item;
        }

        #region IVisitor<InsertStatement,Expression> Members

        public override IDbStatement Visit(Evaluant.Uss.SqlExpressions.InsertStatement item)
        {
            writer.Write("INSERT INTO ");
            Visit(item.Table);
            if (item.Values != null)
            {
                bool isFirst = true;
                writer.Write('(');
                foreach (KeyValuePair<ColumnExpression, NLinq.Expressions.Expression> kvp in item.Values)
                {
                    if (!isFirst)
                        writer.Write(", ");
                    writer.Write(beginEscape);
                    Visit(kvp.Key.ColumnName);
                    writer.Write(endEscape);
                    isFirst = false;
                }
                writer.Write(')');
            }
            writer.Write(" VALUES (");
            if (item.Select != null)
                Visit(item.Select);
            if (item.Values != null)
            {
                bool isFirst = true;
                foreach (KeyValuePair<ColumnExpression, NLinq.Expressions.Expression> kvp in item.Values)
                {
                    if (!isFirst)
                        writer.Write(", ");
                    Visit(kvp.Value);
                    isFirst = false;
                }
            }
            writer.Write(')');

            if (item.Identity != null)
            {
                writer.Write(";SELECT ");
                Visit(item.Identity);
                writer.Write(" =SCOPE_IDENTITY()");
            }

            return item;
        }

        #endregion

        #region IVisitor<DeleteStatement,Expression> Members

        public override IDbStatement Visit(Evaluant.Uss.SqlExpressions.DeleteStatement item)
        {
            writer.Write("DELETE");
            Visit(item.From);
            Visit(item.Where);
            return item;
        }

        #endregion

        #region IVisitor<CaseExpression,AliasedExpression> Members

        public override Evaluant.Uss.SqlExpressions.IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.CaseExpression item)
        {
            if (item.Test == null)
            {
                if (item.CaseTests != null && item.CaseTests.Length != 0)
                {
                    writer.Write("CASE ");
                    VisitArray(item.CaseTests, Visit);
                    writer.Write(" ELSE ");
                    Visit(item.DefaultResult);
                    writer.Write(" END");
                }
                else
                    Visit(item.DefaultResult);
            }
            else
            {
            }
            return item;
        }

        public override CaseTestExpression Visit(CaseTestExpression test)
        {
            writer.Write("WHEN ");
            Visit(test.TestExpression);
            writer.Write(" THEN ");
            Visit(test.TestResult);
            return test;
        }

        #endregion

        #region IVisitor<ColumnExpression,AliasedExpression> Members

        public override Evaluant.Uss.SqlExpressions.IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.ComplexColumnExpression item)
        {
            writer.Write(' ');
            if (item.Alias != null)
            {
                Visit(item.Alias);
                writer.Write(".");
            }
            Visit(item.Expression);
            if (item.ColumnAlias != null)
            {
                writer.Write(" AS ");
                Visit(item.ColumnAlias);
            }

            return item;
        }

        public override Evaluant.Uss.SqlExpressions.IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.ColumnExpression item)
        {
            if (item is ComplexColumnExpression)
                return Visit((ComplexColumnExpression)item);
            if (item.Alias != null)
            {
                Visit(item.Alias);
                writer.Write(".");
            }
            if (item.ColumnName != ColumnExpression.AllColumns)
                writer.Write(beginEscape);
            writer.Write(item.ColumnName.Text);
            if (item.ColumnName != ColumnExpression.AllColumns)
                writer.Write(endEscape);
            if (item.ColumnAlias != null)
            {
                writer.Write(" as ");
                writer.Write(beginEscape);
                writer.Write(item.ColumnAlias.Text);
                writer.Write(endEscape);
            }

            return item;
        }

        #endregion

        #region IVisitor<Constant,Expression> Members

        public override IDbExpression Visit(Evaluant.Uss.SqlExpressions.Constant item)
        {
            switch (item.Type)
            {
                case System.Data.DbType.AnsiString:
                case System.Data.DbType.AnsiStringFixedLength:
                case System.Data.DbType.String:
                case System.Data.DbType.StringFixedLength:
                case System.Data.DbType.Xml:
                case System.Data.DbType.Time:
                    writer.Write("'");
                    writer.Write(item.Value);
                    writer.Write("'");
                    break;
                case System.Data.DbType.Date:
                case System.Data.DbType.DateTime:
                case System.Data.DbType.DateTime2:
                case System.Data.DbType.DateTimeOffset:
                    writer.Write("'");
                    writer.Write(((DateTime)item.Value).ToString("s"));
                    writer.Write("'");
                    break;
                case System.Data.DbType.Binary:
                    break;
                case System.Data.DbType.Boolean:
                    break;
                case System.Data.DbType.Byte:
                    break;
                case System.Data.DbType.Currency:
                    writer.Write(item.Value);
                    break;
                case System.Data.DbType.Decimal:
                case System.Data.DbType.Double:
                case System.Data.DbType.Int16:
                case System.Data.DbType.Int32:
                case System.Data.DbType.Int64:
                case System.Data.DbType.Single:
                case System.Data.DbType.UInt16:
                case System.Data.DbType.UInt32:
                case System.Data.DbType.UInt64:
                case System.Data.DbType.VarNumeric:
                    writer.Write(item.Value);
                    break;
                case System.Data.DbType.Guid:
                    break;
                case System.Data.DbType.Object:
                    break;
                case System.Data.DbType.SByte:
                    break;
                default:
                    break;
            }
            return item;
        }

        #endregion

        #region IVisitor<JoinedTableExpression,AliasedExpression> Members

        public override Evaluant.Uss.SqlExpressions.IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.JoinedTableExpression item)
        {
            Visit(item.LeftTable);
            switch (item.JoinType)
            {
                case JoinType.Inner:
                    writer.Write(" INNER JOIN ");
                    break;
                case JoinType.Cross:
                    writer.Write(" CROSS JOIN ");
                    break;
                case JoinType.Left:
                    writer.Write(" LEFT JOIN ");
                    break;
                case JoinType.Right:
                    writer.Write(" RIGHT JOIN ");
                    break;
            }
            Visit(item.RightTable);
            writer.Write(" ON (");
            Visit(item.On);
            writer.Write(")");
            return item;
        }

        #endregion

        #region IVisitor<Parameter,Expression> Members

        public override IDbExpression Visit(Evaluant.Uss.SqlExpressions.DbParameter item)
        {
            writer.Write("@");
            writer.Write(item.Name);
            return item;
        }

        #endregion

        #region IVisitor<TableSourceExpression,AliasedExpression> Members

        public override Evaluant.Uss.SqlExpressions.IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.TableSourceExpression item)
        {
            Visit(item.Table);
            if (!(item.Alias is Mapping.LazyTableAlias) && !tables[item.Table].Contains(item.Alias))
                tables[item.Table].Add(item.Alias);
            if (item.Alias != null)
            {
                writer.Write(" AS ");
                Visit(item.Alias);
            }
            return item;
        }

        #endregion

        public override TableAlias Visit(Mapping.LazyTableAlias item)
        {
            if (!alias.ContainsKey(item))
                alias.Add(item, alias[tables[item.TargetTable][0]]);

            writer.Write("[");
            writer.Write(alias[item]);
            writer.Write("]");
            return item;
        }

        #region IVisitor<TableAlias,TableAlias> Members

        public override Evaluant.Uss.SqlExpressions.TableAlias Visit(Evaluant.Uss.SqlExpressions.TableAlias item)
        {
            if (item != null)
            {
                //if (item is Mapping.LazyTableAlias)
                //    return Visit((Mapping.LazyTableAlias)item);
                if (!alias.ContainsKey(item))
                    alias.Add(item, "t" + alias.Count);

                writer.Write(beginEscape);
                writer.Write(alias[item]);
                writer.Write(endEscape);
            }
            return item;
        }

        #endregion

        #region IVisitor<OrderByClause,QueryBodyClause> Members

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.OrderByClause item)
        {
            if (item == null)
                return null;
            writer.Write(" ORDER BY ");
            VisitOrderByCriteria(item.Criterias);
            return item;
        }

        #endregion

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.BinaryExpression item)
        {
            Visit(item.LeftExpression);
            switch (item.Type)
            {
                case Evaluant.NLinq.Expressions.BinaryExpressionType.And:
                    writer.Write(" AND ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Or:
                    writer.Write(" OR ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.NotEqual:
                    writer.Write(" != ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.LesserOrEqual:
                    writer.Write(" <= ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.GreaterOrEqual:
                    writer.Write(" >= ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Lesser:
                    writer.Write(" < ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Greater:
                    writer.Write(" > ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Equal:
                    writer.Write(" = ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Minus:
                    writer.Write(" - ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Plus:
                    writer.Write(" + ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Modulo:
                    writer.Write(" % ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Div:
                    writer.Write(" / ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Times:
                    writer.Write(" * ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Pow:
                    writer.Write(" ^ ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Unknown:
                    break;
                default:
                    break;
            }
            Visit(item.RightExpression);
            return item;
        }

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.WhereClause expression)
        {
            if (expression == null)
                return null;
            bool previousInWhere = inWhere;
            inWhere = true;
            writer.Write(" WHERE (");
            Visit(expression.Expression);
            writer.Write(')');
            inWhere = previousInWhere;
            return expression;
        }

        public override IDbStatement Visit(DropTableStatement item)
        {
            writer.Write("DROP TABLE ");
            Visit(item.Table);
            return item;
        }



        public override IDbStatement Visit(IfStatement item)
        {
            writer.Write("IF "); //EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'");
            Visit(item.Condition);
            writer.Write(' ');
            Visit(item.Then);
            return item;
        }

        public override IDbStatement Visit(SchemaStatement item)
        {
            switch (item.StatementType)
            {
                case System.Data.StatementType.Delete:
                    writer.Write("DROP ");
                    break;
                case System.Data.StatementType.Insert:
                    writer.Write("CREATE ");
                    break;
            }
            writer.Write("SCHEMA ");
            writer.Write(beginEscape);
            writer.Write(item.Name);
            writer.Write(endEscape);
            return item;
        }

        //public override Evaluant.NLinq.Expressions.Expression Visit(IfForeignKeyExistsStatement item)
        //{
        //    writer.Write("IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'");
        //    writer.Write(item.Item);
        //    writer.WriteLine("') AND parent_object_id=OBJECT_ID(N'");
        //    writer.Write(item.ParentItem);
        //    writer.WriteLine("'))");
        //    return Visit(item.Statement);
        //}

        public override IDbExpression Visit(HardCodedExpression exp)
        {
            writer.Write(exp.Sql);
            return exp;
        }

        public override IDbStatement Visit(CreateTableStatement item)
        {
            writer.Write("CREATE TABLE ");
            Visit(item.Table);
            writer.WriteLine("(");
            foreach (Mapping.Field field in item.Columns)
            {
                Visit(field);
                if (item.Columns[item.Columns.Count - 1] != field)
                    writer.WriteLine(",");
            }
            writer.WriteLine(")");

            writer.WriteLine();
            VisitEnumerable(item.Constraints, Visit);
            return item;
        }

        public override IDbStatement Visit(UpdateStatement item)
        {
            writer.Write("UPDATE ");
            Visit(item.From[0]);
            writer.Write(" SET ");
            bool isFirst = true;
            foreach (var setValue in item.Set)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.Write(", ");
                Visit(setValue);
            }
            Visit(item.Where);
            return item;
        }

        public override IDbExpression Visit(TupleExpression item)
        {
            writer.Write("(");
            bool isFirst = true;
            foreach (var tupleItem in item)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.Write(", ");
                Visit(tupleItem);
            }
            writer.Write(")");
            return item;
        }

        public override KeyValuePair<ColumnExpression, DbParameter> Visit(KeyValuePair<ColumnExpression, DbParameter> sets)
        {
            Visit(sets.Key);
            writer.Write("=");
            Visit(sets.Value);
            return sets;
        }

        public void Visit(Mapping.Table table)
        {
            if (!string.IsNullOrEmpty(table.Schema))
            {
                writer.Write(beginEscape);
                writer.Write(table.Schema);
                writer.Write(endEscape);
                writer.Write(".");
            }
            if (!tables.ContainsKey(table))
                tables.Add(table, new List<TableAlias>());
            writer.Write(beginEscape);
            writer.Write(table.TableName);
            writer.Write(endEscape);
        }

        public void Visit(Mapping.Field field)
        {
            writer.Write(beginEscape);
            writer.Write(field.ColumnName);
            writer.Write(endEscape);
            writer.Write(" ");
            writer.Write(beginEscape);
            Visit(field.DbType);
            writer.Write(endEscape);
            if (field.Precision != 0 && field.Scale != 0)
                writer.Write("({0}, {1}) ", field.Precision, field.Scale);
            if (field.Size != 0)
                writer.Write("({0})", field.Size);
            if (field.IsIdentity)
                writer.Write(" IDENTITY(1,1) ");
            writer.Write(field.IsNullable ? "NULL" : "NOT NULL");
        }

        public override Statements.AlterTableAddStatement Visit(Statements.AlterTableAddStatement item)
        {
            writer.Write("ALTER TABLE ");
            Visit(item.Table);
            writer.Write(" ADD ");
            if (!string.IsNullOrEmpty(item.ConstraintName))
            {
                writer.Write("CONSTRAINT ");
                writer.Write(beginEscape);
                writer.Write(item.ConstraintName);
                writer.Write(endEscape);
                Visit(item.Constraint);
            }
            return item;

        }

        public override Statements.AlterTableDropStatement Visit(Statements.AlterTableDropStatement item)
        {
            writer.Write("ALTER TABLE ");
            Visit(item.Table);
            writer.Write(" DROP ");
            if (!string.IsNullOrEmpty(item.ConstraintName))
            {
                writer.Write("CONSTRAINT ");
                writer.Write(beginEscape);
                writer.Write(item.ConstraintName);
                writer.Write(endEscape);
            }
            return item;

        }

        private void Visit(Statements.IConstraint constraint)
        {
            Visit(constraint as DefaultConstraint);
            Visit(constraint as PrimaryKeyConstraint);
            Visit(constraint as ForeignKeyConstraint);
        }

        private void Visit(DefaultConstraint constraint)
        {
            if (constraint == null)
                return;
            Mapping.Field f = constraint.Field;
            writer.Write(" DEFAULT (");
            if (f.DbType == System.Data.DbType.AnsiString)
                writer.Write('\'');
            writer.Write(f.DefaultValue);
            if (f.DbType == System.Data.DbType.AnsiString)
                writer.Write('\'');
            writer.Write(") FOR ");
            writer.Write(beginEscape);
            writer.Write(f.ColumnName.Text);
            writer.WriteLine(endEscape);
        }

        private void Visit(PrimaryKeyConstraint constraint)
        {
            if (constraint == null)
                return;
            writer.Write(" PRIMARY KEY ");
            Visit(constraint.Fields);
        }

        private void Visit(ForeignKeyConstraint constraint)
        {
            if (constraint == null)
                return;
            writer.Write(" FOREIGN KEY ");
            Visit(constraint.Fields);
            writer.Write(" REFERENCES ");
            Visit(constraint.ReferencesTable);
            Visit(constraint.References);

        }

        private void Visit(IList<Mapping.Field> fields)
        {
            bool isFirst = true;
            writer.Write("(");
            foreach (Mapping.Field f in fields)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.WriteLine(',');
                writer.Write(beginEscape);
                writer.Write(f.ColumnName.Text);
                writer.Write(endEscape);
            }
            writer.Write(")");
        }

        private void Visit(System.Data.DbType dbType)
        {
            switch (dbType)
            {
                case System.Data.DbType.AnsiString:
                    writer.Write("varchar");
                    break;
                case System.Data.DbType.AnsiStringFixedLength:
                    writer.Write("char");
                    break;
                case System.Data.DbType.Binary:
                    writer.Write("varbinary");
                    break;
                case System.Data.DbType.Boolean:
                    writer.Write("bit");
                    break;
                case System.Data.DbType.Byte:
                    break;
                case System.Data.DbType.Currency:
                    break;
                case System.Data.DbType.Date:
                    break;
                case System.Data.DbType.DateTime:
                    writer.Write("datetime");
                    break;
                case System.Data.DbType.DateTime2:
                    writer.Write("datetime");
                    break;
                case System.Data.DbType.DateTimeOffset:
                    break;
                case System.Data.DbType.Decimal:
                    writer.Write("decimal");
                    break;
                case System.Data.DbType.Double:
                    break;
                case System.Data.DbType.Guid:
                    writer.Write("uniqueidentifier");
                    break;
                case System.Data.DbType.Int16:
                    break;
                case System.Data.DbType.Int32:
                    writer.Write("int");
                    break;
                case System.Data.DbType.Int64:
                    break;
                case System.Data.DbType.Object:
                    break;
                case System.Data.DbType.SByte:
                    break;
                case System.Data.DbType.Single:
                    break;
                case System.Data.DbType.String:
                    writer.Write("nvarchar");
                    break;
                case System.Data.DbType.StringFixedLength:
                    writer.Write("nchar");
                    break;
                case System.Data.DbType.Time:
                    break;
                case System.Data.DbType.UInt16:
                    break;
                case System.Data.DbType.UInt32:
                    break;
                case System.Data.DbType.UInt64:
                    break;
                case System.Data.DbType.VarNumeric:
                    break;
                case System.Data.DbType.Xml:
                    break;
                default:
                    break;
            }
        }
    }
}
