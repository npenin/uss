
using System;
using System.Data;


namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de SqlDataFactory.
	/// </summary>
	public class OdbcDriver : DriverBase
	{
		public override System.Type CommandType
		{
			get { return typeof(System.Data.Odbc.OdbcCommand); }
		}

		public override System.Type ConnectionType
		{
			get { return typeof(System.Data.Odbc.OdbcConnection);}
		}

		public override System.Type ParameterType
		{
			get { return typeof(System.Data.Odbc.OdbcParameter); }
		}

		public override bool UseNamedPrefixInParameter
		{
			get { return false; }
		}

		public override string NamedPrefix
		{
			get { return String.Empty; }
		}

		public override IDbDataParameter CreateParameter(string name, DbType dbtype)
		{
			IDbDataParameter param = CreateParameter();
			param.ParameterName = FormatParameter(name);

			param.DbType =  dbtype;

			return param;
		}		
	}
}
