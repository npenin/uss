using System;
using System.Data;
using System.Data.SqlClient;
using Evaluant.Uss.Data.FileClient;

namespace Evaluant.Uss.SqlMapper
{
    public class FileDriver : DriverBase
    {
        public override System.Type CommandType
        {
            get { return typeof(FileCommand); }
        }

        public override System.Type ConnectionType
        {
            get { return typeof(FileConnection); }
        }

        public override System.Type ParameterType
        {
            get { return typeof(FileParameter); }
        }

        public override IDbConnection CreateConnection()
        {
            return new FileConnection();
        }

        public override IDbCommand CreateCommand()
        {
            return new FileCommand();
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
