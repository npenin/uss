using System;
using System.Data;
using System.Data.SqlClient;

namespace Evaluant.Uss.SqlMapper
{
    /// <summary>
    /// Description résumée de SqlDataFactory.
    /// </summary>
    public class MsSqlDriver : DriverBase
    {
        public override System.Type CommandType
        {
            get { return typeof(System.Data.SqlClient.SqlCommand); }
        }

        public override System.Type ConnectionType
        {
            get { return typeof(System.Data.SqlClient.SqlConnection); }
        }

        public override System.Type ParameterType
        {
            get { return typeof(System.Data.SqlClient.SqlParameter); }
        }

        public override IDbConnection CreateConnection()
        {
            return new SqlConnection();
        }

        public override bool UseNamedPrefixInParameter
        {
            get { return true; }
        }

        public override string NamedPrefix
        {
            get { return "@"; }
        }

        private static DateTime dateTimeMinValue = new DateTime(1753, 1, 1, 12, 0, 0);
        public override DateTime DateTimeMinValue
        {
            get
            {
                return dateTimeMinValue;
            }
        }
    }
}
