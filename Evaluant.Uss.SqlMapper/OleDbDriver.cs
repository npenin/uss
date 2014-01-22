using System;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Collections;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de SqlDataFactory.
	/// </summary>
	public class OleDbDriver : DriverBase
	{
		public override System.Type CommandType
		{
			get { return typeof(System.Data.OleDb.OleDbCommand); }
		}

		public override System.Type ConnectionType
		{
			get { return typeof(System.Data.OleDb.OleDbConnection);}
		}

		public override System.Type ParameterType
		{
			get { return typeof(System.Data.OleDb.OleDbParameter); }
		}

		public override bool UseNamedPrefixInParameter
		{
			get { return true; }
		}

		public override string NamedPrefix
		{
			get { return "@"; }
		}

		public override IDbDataParameter CreateParameter(string name, DbType dbtype)
		{
			IDbDataParameter param = CreateParameter();
			param.ParameterName = FormatParameter(name);

			if ( dbtype == DbType.Object )
				((OleDbParameter) param).OleDbType = OleDbType.LongVarBinary;
			else
				param.DbType =  dbtype;

			return param;
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
