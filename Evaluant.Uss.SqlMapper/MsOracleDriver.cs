using System;
using System.Data;
using System.Data.OracleClient;
using System.Text;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de MsOracleDriver.
	/// </summary>
	public class MsOracleDriver : DriverBase
	{
		public override System.Type CommandType
		{
			get { return typeof(System.Data.OracleClient.OracleCommand); }
		}

		public override System.Type ConnectionType
		{
			get { return typeof(System.Data.OracleClient.OracleConnection); }
		}

		public override System.Type ParameterType
		{
			get { return typeof(System.Data.OracleClient.OracleParameter); }
		}

		public override bool UseNamedPrefixInParameter
		{
			get { return true; }
		}

		public override string NamedPrefix
		{
			get { return "&p"; }
		}
	}
}
