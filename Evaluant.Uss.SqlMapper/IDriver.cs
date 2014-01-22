using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Evaluant.Uss.SqlExpressions.Mapping;
using System.Data.Common;

namespace Evaluant.Uss.SqlMapper
{
    public interface IDriver
    {
        void Initialize(string connectionString);

        IDbConnection CreateConnection();
        IDbCommand GetCommand(IDbConnection connection, string sqlQuery);

        DbType GetDbType(Type type);
        void GetTypeInformation(Model.Attribute attribute, Mapping.Attribute field);

        IDbTransaction BeginTransaction(IDbConnection connection);

        bool IsOrm { get; }

        bool SupportDataManipulationLanguage { get; }

        DbProviderFactory AlternateFactory { get; }
        string AlternateProviderName { get; }
        string AlternateConnectionString { get; }


        IDataParameter GetParameter(IDbCommand command, SqlExpressions.ValuedParameter parameter);
    }
}
