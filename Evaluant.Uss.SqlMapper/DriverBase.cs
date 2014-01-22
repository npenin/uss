using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using Evaluant.Uss.Common;

namespace Evaluant.Uss.SqlMapper
{
    /// <summary>
    /// Description résumée de DriverBase.
    /// </summary>
    public abstract class DriverBase : IDriver
    {
        private DBDialect _Dialect;

        public DBDialect Dialect
        {
            get { return _Dialect; }
            set { _Dialect = value; }
        }

        #region IDriver Members

        public abstract System.Type CommandType { get; }
        public abstract System.Type ConnectionType { get; }
        public abstract System.Type ParameterType { get; }
        public virtual DateTime DateTimeMinValue
        {
            get { return DateTime.MinValue; }
        }

        private IActivator connectionActivator = null;

        public virtual IDbConnection CreateConnection()
        {
            if (connectionActivator == null)
            {
                connectionActivator = ActivatorFactory.CreateActivator(ConnectionType);
            }

            return (IDbConnection)connectionActivator.CreateInstance();
        }

        public IDbTransaction BeginTransaction(IDbConnection connection)
        {
            if (System.Transactions.Transaction.Current != null)
            {
                switch (System.Transactions.Transaction.Current.IsolationLevel)
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
        
        public IDbConnection CreateConnection(string connStr)
        {
            IDbConnection conn = CreateConnection();
            conn.ConnectionString = connStr;

            return conn;
        }

        private IActivator commandActivator = null;

        public virtual IDbCommand CreateCommand()
        {
            if (commandActivator == null)
            {
                commandActivator = ActivatorFactory.CreateActivator(CommandType);
            }

            return (IDbCommand)commandActivator.CreateInstance();
        }

        public virtual IDbCommand CreateCommand(string query, IDbConnection connection)
        {
            IDbCommand cmd = CreateCommand();
            cmd.CommandText = query;
            cmd.Connection = connection;

            return cmd;
        }

        public virtual IDbCommand CreateCommand(string query, IDbConnection connection, CommandType type)
        {
            IDbCommand cmd = CreateCommand(query, connection);
            cmd.CommandType = type;

            return cmd;
        }

        public IDbCommand CreateCommand(string query, IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand cmd = CreateCommand(query, connection);
            cmd.Transaction = transaction;

            return cmd;
        }

        public abstract bool UseNamedPrefixInParameter { get; }
        public virtual bool UseIndexedParameters { get { return false; } }
        public abstract string NamedPrefix { get; }

        public virtual string FormatParameter(string parameterName)
        {
            if (UseIndexedParameters)
                return NamedPrefix;
            return UseNamedPrefixInParameter ? (NamedPrefix + parameterName) : parameterName;
        }

        public string FormatParameter(string tableAlias, string parameterName)
        {
            if (!UseNamedPrefixInParameter)
                return parameterName;

            if (tableAlias != null && tableAlias.Length > 0)
                return String.Concat(NamedPrefix, tableAlias, parameterName);
            else
                return String.Concat(NamedPrefix, parameterName);
        }

        IActivator parameterActivator = null;
        public IDbDataParameter CreateParameter()
        {
            if (parameterActivator == null)
            {
                parameterActivator = ActivatorFactory.CreateActivator(ParameterType);
            }

            return (IDbDataParameter)parameterActivator.CreateInstance();
        }

        public virtual IDbDataParameter CreateParameter(string name, DbType dbtype)
        {
            IDbDataParameter param = CreateParameter();
            param.ParameterName = FormatParameter(name);
            param.DbType = dbtype;

            return param;
        }

        public virtual IDbDataParameter CreateParameter(string name, DbType dbtype, object value)
        {
            IDbDataParameter param = CreateParameter(name, dbtype);
            if (dbtype == DbType.Guid && value is string)
                param.Value = new Guid((string)value);
            else
            {
                if (dbtype == DbType.DateTime && value != null)
                {
                    DateTime dtValue = (DateTime)value;
                    if (dtValue < DateTimeMinValue)
                        param.Value = DateTimeMinValue;
                    else
                        param.Value = dtValue;
                }
                else
                    param.Value = value == null ? DBNull.Value : value;
            }


            return param;
        }

        public IDbDataParameter CreateParameter(string name, object value, DbType dbtype, ParameterDirection direction)
        {
            IDbDataParameter param = CreateParameter(name, dbtype, value);
            param.Direction = direction;
            return param;
        }

        public virtual IDbDataParameter CreateParameter(string name, object value, DbType dbtype, int size)
        {
            IDbDataParameter param = CreateParameter(name, dbtype, value);
            if (size != 0)
                param.Size = size;

            return param;
        }

        public IDbDataParameter CreateParameter(string name, object value, DbType dbtype, byte precision, byte scale)
        {
            IDbDataParameter param = CreateParameter(name, dbtype, value);
            param.Precision = precision;
            param.Scale = scale;

            return param;
        }

        public virtual Algo GetAlgorithm()
        {
            return Algo.Embeded;
        }

        public virtual void PreProcessQuery(IDbCommand command)
        {
        }

        #endregion
    }
}
