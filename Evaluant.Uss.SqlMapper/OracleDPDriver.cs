using System;
using System.Data;
using System.Text;
using Evaluant.Uss.SqlMapper;
using System.Configuration;
using Evaluant.Uss.Common;
using System.Reflection;
using SQLObject;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de MsOracleDriver.
	/// </summary>
    public class OracleDPDriver : DriverBase
	{
        private Type connectionType;
        private Type commandType;
        private Type parameterType;
        private Type dbtypeType;

        private IActivator commandTypeActivator;

        public OracleDPDriver()
		{
            string connectionClassName = "Oracle.DataAccess.Client.OracleConnection, Oracle.DataAccess";
            string commandClassName = "Oracle.DataAccess.Client.OracleCommand, Oracle.DataAccess";
            string parameterClassName = "Oracle.DataAccess.Client.OracleParameter, Oracle.DataAccess";
            string dbtypeClassName = "Oracle.DataAccess.Client.OracleDbType, Oracle.DataAccess";

			// try to get the Types from an already loaded assembly
			connectionType = Utils.GetType(connectionClassName);
			commandType = Utils.GetType(commandClassName);
			parameterType = Utils.GetType(parameterClassName);
			dbtypeType = Utils.GetType(dbtypeClassName);

            commandTypeActivator = ActivatorFactory.CreateActivator(commandType);

			if( connectionType == null || commandType == null )
				throw new SqlMapperException("The IDbCommand and IDbConnection implementation in Oracle.DataAccess.Client could not be found.  ");
		}

		public override System.Type CommandType
		{
			get { return commandType; }
		}

		public override System.Type ConnectionType
		{
			get { return connectionType; }
		}

		public override System.Type ParameterType
		{
			get { return parameterType; }
		}

		public override bool UseNamedPrefixInParameter
		{
			get { return true; }
		}

		public override string NamedPrefix
		{
			get { return ":p"; }
		}

		public override IDbCommand CreateCommand()
		{
			// Force the Oracle Driver to use Named parameters (instead of ordered ones)
			IDbCommand cmde = (IDbCommand) commandTypeActivator.CreateInstance();
			commandType.GetProperty("BindByName").SetValue(cmde, true, null);

            return cmde;
		}

		public override IDbDataParameter CreateParameter(string name, DbType dbtype)
		{
			IDbDataParameter param = CreateParameter();
			param.ParameterName = FormatParameter(name);

			switch (dbtype)
			{
				case DbType.Boolean :
					param.DbType = DbType.Int32;
					break;

					//case DbType.Currency :
					//    break;


                case DbType.Guid:
                    param.DbType = DbType.AnsiString;
                    break;

				case DbType.Object :
                case DbType.SByte:
                    parameterType.GetProperty("OracleDbType").SetValue(param, dbtypeType.InvokeMember("Blob", BindingFlags.Default | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, null, null), null);
                    break;

					//case DbType.UInt16 :
					//    break;

					//case DbType.UInt32 :
					//    break;

					//case DbType.UInt64 :
					//    break;

					//case DbType.VarNumeric :
					//    break;

				default:
					param.DbType = dbtype;
					break;
			}
			return param;
		}

        public override IDbDataParameter CreateParameter(string name, object value, DbType dbtype, int size)
        {
            if ((dbtype == DbType.String || dbtype==DbType.AnsiString) && size == 0)
            {
                IDbDataParameter param= CreateParameter(name, dbtype, value);
                parameterType.GetProperty("OracleDbType").SetValue(param, dbtypeType.InvokeMember("NClob", BindingFlags.Default | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, null, null), null);
                return param;
            }

            return base.CreateParameter(name, value, dbtype, size);
        }

        public override IDbDataParameter CreateParameter(string name, DbType dbtype, object value)
        {
            IDbDataParameter param = base.CreateParameter(name, dbtype, value);
            if (dbtype == DbType.Guid)
                param.Value = param.Value.ToString();
            return param;
        }
	}
}
