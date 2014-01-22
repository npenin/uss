using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Evaluant.Uss.SqlMapper.Drivers
{
    public class Script : IDriver
    {
        private string connectionString;
        #region IDriver Members

        public void Initialize(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public System.Data.IDbConnection CreateConnection()
        {
            return new File.Connection(connectionString);
        }
        
        public IDbCommand GetCommand(IDbConnection connection, string sqlQuery)
        {
            //BeginTransaction(connection);
            IDbCommand command = connection.CreateCommand();
            command.CommandText = sqlQuery;
            return command;
        }

        public System.Data.DbType GetDbType(Type type)
        {
            throw new NotImplementedException();
        }

        public void GetTypeInformation(Model.Attribute attribute, Mapping.Attribute field)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDbTransaction BeginTransaction(System.Data.IDbConnection connection)
        {
            return connection.BeginTransaction();
        }

        public bool IsOrm
        {
            get { return false; }
        }

        public bool SupportDataManipulationLanguage
        {
            get { return true; }
        }

        public System.Data.Common.DbProviderFactory AlternateFactory
        {
            get { return null; }
        }

        public string AlternateProviderName
        {
            get { return null; }
        }

        public string AlternateConnectionString
        {
            get { return null; }
        }

        public System.Data.IDataParameter GetParameter(System.Data.IDbCommand command, SqlExpressions.ValuedParameter parameter)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
