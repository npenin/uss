using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Evaluant.Uss.Common;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;

namespace Evaluant.Uss.SqlMapper
{
    public class Db2Driver : DriverBase
    {
        private Type connectionType;
		private Type commandType;
		private Type parameterType;
		private Type dbtypeType;

        public Db2Driver()
		{

            string connectionClassName = "IBM.Data.DB2.iSeries.iDB2Connection, IBM.Data.DB2.iSeries";
            string commandClassName = "IBM.Data.DB2.iSeries.iDB2Command, IBM.Data.DB2.iSeries";
            string parameterClassName = "IBM.Data.DB2.iSeries.iDB2Parameter, IBM.Data.DB2.iSeries";
            string dbtypeClassName = "IBM.Data.DB2.iSeries.iDB2DbType, IBM.Data.DB2.iSeries";

			// try to get the Types from an already loaded assembly
			connectionType = Utils.GetType(connectionClassName);
			commandType = Utils.GetType(commandClassName);
			parameterType = Utils.GetType(parameterClassName);
			dbtypeType = Utils.GetType(dbtypeClassName);

			if( connectionType == null || commandType == null )
				throw new SqlMapperException("The IDbCommand and IDbConnection implementation in Oracle.DataAccess.Client could not be found.  ");
		}

        public override IDbDataParameter CreateParameter(string name, DbType dbtype)
        {
            bool changeToVarChar = false;
            switch (dbtype)
            {
                case DbType.AnsiString:
                    dbtype = DbType.String;
                    changeToVarChar = true;
                    break;
                case DbType.Guid:
                    dbtype = DbType.String;
                    changeToVarChar = true;
                    break;
            }
            IDbDataParameter param = base.CreateParameter(name, dbtype);
            if (changeToVarChar)
                parameterType.GetProperty("iDB2DbType").SetValue(param, dbtypeType.InvokeMember("iDB2VarChar", BindingFlags.Default | BindingFlags.Public | BindingFlags.GetField | BindingFlags.Static, null, null, null), null);
            return param;
        }

        public override Type CommandType
        {
            get { return commandType; }
        }

        public override Type ConnectionType
        {
            get { return connectionType; }
        }

        public override Type ParameterType
        {
            get { return parameterType; }
        }

        public override bool UseNamedPrefixInParameter
        {
            get { return true; }
        }

        public override bool UseIndexedParameters
        {
            get { return false; }
        }

        public override string NamedPrefix
        {
            get { return "@"; }
        }

        public override void PreProcessQuery(IDbCommand command)
        {
            string sql = command.CommandText;
            MatchCollection matches = Regex.Matches(sql, @"@\w*");
            Hashtable parameters = new Hashtable();
            foreach (Match m in matches)
            {
                foreach (IDbDataParameter dbparam in command.Parameters)
                {
                    if (dbparam.ParameterName == m.Value && !(parameters.ContainsKey(m.Value)))
                        parameters.Add(m.Value, dbparam);
                }
            }

            command.Parameters.Clear();

            foreach (Match m in matches)
            {
                IDbDataParameter param = (IDbDataParameter)parameters[m.Value];
                command.Parameters.Add(CreateParameter(param.ParameterName, param.DbType, param.Value));
            }
        }
    }
}
