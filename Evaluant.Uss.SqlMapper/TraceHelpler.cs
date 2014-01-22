using System;
using System.Text;
using System.Data;

namespace Evaluant.Uss.SqlMapper
{
    public class TraceHelpler
    {
        public static void Trace(IDbCommand command, DBDialect dialect)
        {
            string query = command.CommandText;

            if (dialect != null)
            {
                foreach (IDbDataParameter param in command.Parameters)
                    if(param.Value != DBNull.Value)
                        query = query.Replace(param.ParameterName, dialect.FormatValue(param.Value.ToString(), param.DbType));
                    else
                        query = query.Replace(param.ParameterName, "NULL");
            }
            else
            {
                foreach (IDataParameter param in command.Parameters)
                {
                    string value = param.Value == null ? "null" : param.Value.ToString();
                    query += String.Concat("\n",  param.ParameterName, " : ", param.DbType.ToString(), " = ", value);
                }
            }

            System.Diagnostics.Trace.WriteLine(query);
        }
    }
}
