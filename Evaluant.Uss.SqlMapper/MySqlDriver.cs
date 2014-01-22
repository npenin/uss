using System;
using System.Data;

using Evaluant.Uss.Common;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de SqlDataFactory.
	/// </summary>

	public class MySqlDriver : DeclarativeDriver
	{
		public MySqlDriver() : 
			base(
				"MySql.Data.MySqlClient.MySqlConnection, MySql.Data",
				"MySql.Data.MySqlClient.MySqlCommand, MySql.Data",
				"MySql.Data.MySqlClient.MySqlParameter, MySql.Data",
				"?"
			)
		{
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
