using System;
using System.Data;

using Evaluant.Uss.Common;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de SqlDataFactory.
	/// </summary>
	public abstract class DeclarativeDriver : DriverBase
	{
		private System.Type connectionType;
		private System.Type commandType;
		private System.Type parameterType;
		private string prefix;
 
		public DeclarativeDriver(string connectionTypeName, string commandTypeName, string parameterTypeName, string prefix)
		{
			// try to get the Types from an already loaded assembly
			connectionType = Utils.GetType(connectionTypeName);
			commandType = Utils.GetType(commandTypeName);
			parameterType = Utils.GetType(parameterTypeName);

			this.prefix = prefix;

			if( connectionType == null || commandType == null )
				throw new SqlMapperException("The IDbCommand and IDbConnection implementation in the specified assembly could not be found.");
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
			get { return prefix; }
		}
	}
}
