using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlMapper;
using System.Collections;

namespace Evaluant.Uss.SqlMapper
{
    public class CsvDialect : DBDialect
    {
        DBDialect dialect = new OracleDialect();

        public CsvDialect()
        {
            Separator = '$';
        }

        public override int MaxAnsiStringSize
        {
            get { return dialect.MaxAnsiStringSize; }
        }

        public override int MaxBinarySize
        {
            get { return dialect.MaxBinarySize; }
        }

        public override int MaxBinaryBlobSize
        {
            get { return dialect.MaxBinaryBlobSize; }
        }

        public override int MaxStringClobSize
        {
            get { return dialect.MaxStringClobSize; }
        }

        public override int MaxStringSize
        {
            get { return dialect.MaxStringSize; }
        }

        public override SQLObject.SqlType TypeName(System.Data.DbType dbType, int length)
        {
            return new OracleDialect().TypeName(dbType, length);
        }

        public override System.Data.DbType GetDbTypeToNativeGenerator(System.Collections.Specialized.StringDictionary parameters)
        {
            throw new NotImplementedException();
        }

        public override string NativeColumnString(global::Evaluant.Uss.SqlMapper.SqlObjectModel.LDD.ColumnDefinition column)
        {
            throw new NotImplementedException();
        }

        public override string NullColumnString
        {
            get { throw new NotImplementedException(); }
        }

        public override bool SupportsIdentityColumns
        {
            get { return false; }
        }

        public override string GetIdentitySelect(string tableAlias)
        {
            throw new NotImplementedException();
        }

        public override string GetDummyTableForScalarResults
        {
            get { throw new NotImplementedException(); }
        }

        public override string IdentityColumnString
        {
            get { throw new NotImplementedException(); }
        }

        public override string DefaultValuestString
        {
            get { throw new NotImplementedException(); }
        }

        public override string FormatAttribute(string name)
        {
            return new OracleDialect().FormatAttribute(name);
        }

        public override void Visit(global::Evaluant.Uss.SqlMapper.SqlObjectModel.LDD.DisableForeignKey dfkCommand)
        {
        }

        public override void Visit(global::Evaluant.Uss.SqlMapper.SqlObjectModel.LDD.EnableForeignKey dfkCommand)
        {
        }

        public override DBDialect.ForeignKeyScope GetDisableForeignKeyScope()
        {
            return ForeignKeyScope.None;
        }

        public override SQLObject.ISQLExpression Page(SQLObject.ISQLExpression sqlExp, SQLObject.OrderByClause orderby, int topPage, int pageSize)
        {
            return sqlExp;
        }

        public char Separator { get; set; }

        private IDictionary<string, IList<string>> tableColumnsOrder = new Dictionary<string, IList<string>>();
        IDictionary<string, SQLObject.ISQLExpression> columnValueCollection;

        public override void Visit(SQLObject.InsertCommand insertCommand)
        {
            _Query.AppendFormat("-- TABLE {0}", insertCommand.TableName).AppendLine();
            columnValueCollection = new Dictionary<string, SQLObject.ISQLExpression>();
            foreach (DictionaryEntry entry in insertCommand.ColumnValueCollection)
                columnValueCollection.Add(entry.Key.ToString(), (SQLObject.ISQLExpression)entry.Value);

            foreach (string columnName in tableColumnsOrder[insertCommand.TableName])
            {
                if (columnValueCollection.ContainsKey(columnName))
                    columnValueCollection[columnName].Accept(this);
                else
                    _Query.Append(Separator);
            }

            columnValueCollection.Clear();
        }

        public override string ConvertDbTypeToString(System.Data.DbType dbtype, int size, byte precision, byte scale)
        {
            switch (dbtype)
            {
                case System.Data.DbType.Guid:
                case System.Data.DbType.AnsiString:
                case System.Data.DbType.AnsiStringFixedLength:
                case System.Data.DbType.String:
                case System.Data.DbType.StringFixedLength:
                    return "CHAR";
                case System.Data.DbType.Date:
                case System.Data.DbType.DateTime:
                case System.Data.DbType.DateTime2:
                case System.Data.DbType.DateTimeOffset:
                    return "DATETIME";
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
                    return "NUMBER";
                case System.Data.DbType.Binary:
                    break;
                case System.Data.DbType.Boolean:
                    break;
                case System.Data.DbType.Byte:
                    break;
                case System.Data.DbType.Currency:
                    break;
                case System.Data.DbType.Object:
                    break;
                case System.Data.DbType.SByte:
                    break;
                case System.Data.DbType.Time:
                    break;
                case System.Data.DbType.Xml:
                    break;
                default:
                    break;
            }
            return base.ConvertDbTypeToString(dbtype, size, precision, scale);
        }

        public override void Visit(Evaluant.Uss.SqlMapper.SqlObjectModel.LDD.CreateTableSQLCommand command)
        {
            _Query.AppendLine("-- LOAD " + command.TableName);
            _Query.AppendLine("-- stop on error");
            _Query.AppendLine("OPTIONS (ERROR=0)");
            _Query.AppendLine();
            _Query.AppendLine("LOAD DATA");
            _Query.AppendFormat("INFILE '{0}.asc'", command.TableName).AppendLine();
            _Query.AppendFormat("INTO TABLE {0}", FormatAttribute(command.TableName)).AppendLine();
            _Query.AppendFormat("FIELDS TERMINATED BY '{0}'", Separator).AppendLine();
            _Query.AppendLine("(");
            List<string> columnsOrder = new List<string>();

            foreach (DictionaryEntry entry in command.ColumnDefinitions)
            {
                columnsOrder.Add(((SqlObjectModel.LDD.ColumnDefinition)entry.Value).ColumnName);
                _Query.AppendFormat("{0} {1} {2}", FormatAttribute(((SqlObjectModel.LDD.ColumnDefinition)entry.Value).ColumnName), this.ConvertDbTypeToString(((SqlObjectModel.LDD.ColumnDefinition)entry.Value).Type, ((SqlObjectModel.LDD.ColumnDefinition)entry.Value).Size, ((SqlObjectModel.LDD.ColumnDefinition)entry.Value).Precision, ((SqlObjectModel.LDD.ColumnDefinition)entry.Value).Scale), ((SqlObjectModel.LDD.ColumnDefinition)entry.Value).IsAutoIncrement ? "EXTERNAL" : string.Empty);
                if (command.ColumnDefinitions.IndexOfKey(entry.Key) != command.ColumnDefinitions.Count - 1)
                    _Query.AppendLine(",");
                else
                    _Query.AppendLine();

            }
            tableColumnsOrder.Add(command.TableName, columnsOrder);
            _Query.AppendLine(")");
            _Query.AppendLine(ENDQUERY);
        }

        public override void Visit(SQLObject.UpdateCommand updateCommand)
        {
            _Query.Append("-- UPDATE " + updateCommand.TableName).AppendLine();
            columnValueCollection = new Dictionary<string, SQLObject.ISQLExpression>();
            foreach (DictionaryEntry entry in updateCommand.ColumnValueCollection)
                columnValueCollection.Add(entry.Key.ToString(), (SQLObject.ISQLExpression)entry.Value);

            updateCommand.WhereClause.Accept(this);

            foreach (string columnName in tableColumnsOrder[updateCommand.TableName])
            {
                if (columnValueCollection.ContainsKey(columnName))
                    columnValueCollection[columnName].Accept(this);
                else
                    _Query.Append(Separator);
            }
        }

        public override void Visit(SQLObject.BinaryLogicExpression expression)
        {
            if (expression.Operator != SQLObject.BinaryLogicOperator.Equals)
                throw new NotImplementedException();

            string columnName;
            if (expression.LeftOperand is SQLObject.Column)
                columnName = ((SQLObject.Column)expression.LeftOperand).ColumnName;
            else
                throw new NotImplementedException();

            columnValueCollection.Add(columnName, expression.RightOperand);
        }

        public override void Visit(SQLObject.WhereClause whereClause)
        {
            whereClause.SearchCondition.Accept(this);
        }

        public override void Visit(SQLObject.DropTableSQLCommand command)
        {
        }

        public override void Visit(SQLObject.Column column)
        {

        }

        public override void Visit(SQLObject.Parameter param)
        {
            _Query.Append(_Driver.FormatParameter(param.Name));
        }

        public override void Visit(SQLObject.Constant constant)
        {
            _Query.Append(constant.Value).Append(Separator);
        }
    }
}
