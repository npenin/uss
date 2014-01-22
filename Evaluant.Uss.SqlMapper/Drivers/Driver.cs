using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Transactions;
using System.Xml;

namespace Evaluant.Uss.SqlMapper.Drivers
{
    public class Driver : IDriver
    {
        public Driver(DbProviderFactory factory)
        {
            this.factory = factory;
        }

        protected DbProviderFactory factory;
        protected string connectionString;

        public virtual bool IsOrm { get { return false; } }

        #region IDriver Members

        public virtual void Initialize(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        public IDbCommand GetCommand(IDbConnection connection, string sqlQuery)
        {
            //BeginTransaction(connection);
            IDbCommand command = connection.CreateCommand();
            command.CommandText = sqlQuery;
            return command;
        }

        public IDataParameter GetParameter(IDbCommand command, SqlExpressions.ValuedParameter parameter)
        {
            var dbParameter = command.CreateParameter();
            dbParameter.DbType = parameter.Type;
            dbParameter.Direction = parameter.Direction;
            dbParameter.ParameterName = parameter.Name;
            dbParameter.Precision = parameter.Precision;
            dbParameter.Scale = parameter.Scale;
            dbParameter.Size = parameter.Size;
            if (parameter.Type == DbType.Guid && parameter.Value != null && parameter.Value is string)
                dbParameter.Value = new Guid((string)parameter.Value);
            else
                dbParameter.Value = parameter.Value;
            return dbParameter;
        }

        public DbType GetDbType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return GetDbType(type.GetGenericArguments()[0]);

            return GetDbTypeOverride(type);
        }

        private DbType GetDbTypeOverride(Type type)
        {
            if (type == typeof(byte[]))
                return DbType.Binary;
            if (type == typeof(bool))
                return DbType.Boolean;
            if (type == typeof(byte))
                return DbType.Byte;
            if (type == typeof(DateTime))
                return DbType.DateTime;
            if (type == typeof(decimal))
                return DbType.Decimal;
            if (type == typeof(double))
                return DbType.Double;
            if (type == typeof(Guid))
                return DbType.Guid;
            if (type == typeof(short))
                return DbType.Int16;
            if (type == typeof(int))
                return DbType.Int32;
            if (type == typeof(long))
                return DbType.Int64;
            if (type == typeof(sbyte))
                return DbType.SByte;
            if (type == typeof(float))
                return DbType.Single;
            if (type == typeof(string))
                return DbType.String;
            if (type == typeof(TimeSpan))
                return DbType.Time;
            if (type == typeof(ushort))
                return DbType.UInt16;
            if (type == typeof(uint))
                return DbType.UInt32;
            if (type == typeof(ulong))
                return DbType.UInt64;
            if (type == typeof(XmlNode))
                return DbType.Xml;
            return DbType.Object;
        }

        public void GetTypeInformation(Model.Attribute attribute, Mapping.Attribute field)
        {
            field.DbType = GetDbType(attribute.Type);
            if (field.DbType == DbType.String)
                field.Size = 50;
            if (field.IsId)
            {
                switch (field.DbType)
                {
                    case DbType.Guid:
                        field.Generator = Mapping.Generator.Guid;
                        break;
                    case DbType.Date:
                    case DbType.DateTime:
                    case DbType.DateTime2:
                    case DbType.Decimal:
                    case DbType.Int16:
                    case DbType.Int32:
                    case DbType.Int64:
                    case DbType.Time:
                    case DbType.UInt16:
                    case DbType.UInt32:
                    case DbType.UInt64:
                    case DbType.VarNumeric:
                        field.Generator = Evaluant.Uss.SqlMapper.Mapping.Generator.Native;
                        break;
                }
            }
        }

        public IDbTransaction BeginTransaction(IDbConnection connection)
        {
            if (Transaction.Current != null)
            {
                switch (Transaction.Current.IsolationLevel)
                {
                    case System.Transactions.IsolationLevel.Chaos:
                        return connection.BeginTransaction(System.Data.IsolationLevel.Chaos);
                    case System.Transactions.IsolationLevel.ReadCommitted:
                        return connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    case System.Transactions.IsolationLevel.ReadUncommitted:
                        return connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                    case System.Transactions.IsolationLevel.RepeatableRead:
                        return connection.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                    case System.Transactions.IsolationLevel.Serializable:
                        return connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
                    case System.Transactions.IsolationLevel.Snapshot:
                        return connection.BeginTransaction(System.Data.IsolationLevel.Snapshot);
                    case System.Transactions.IsolationLevel.Unspecified:
                        return connection.BeginTransaction(System.Data.IsolationLevel.Unspecified);
                }
            }
            return connection.BeginTransaction();
        }


        #endregion

        #region IDriver Members


        public virtual bool SupportDataManipulationLanguage
        {
            get { return true; }
        }

        public virtual IDriver CreateAlternateDriver()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDriver Members

        public virtual DbProviderFactory AlternateFactory
        {
            get { return null; }
        }

        public virtual string AlternateProviderName
        {
            get { return null; }
        }

        public virtual string AlternateConnectionString
        {
            get { return null; }
        }

        #endregion
    }
}
