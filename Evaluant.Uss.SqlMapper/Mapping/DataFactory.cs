using System.Data;


namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de DataFactory.
	/// </summary>
	public interface DataFactory
	{
		IDbConnection CreateConnection();
		IDbConnection CreateConnection(string connStr);

		IDbCommand CreateCommand(string query, IDbConnection connection);

		IDbDataParameter CreateParameter(DbType dbtype);
		IDbDataParameter CreateParameter(DbType dbtype, int size);
		IDbDataParameter CreateParameter(DbType dbtype, byte precision, byte scale);
		IDbDataParameter CreateParameter(DbType dbtype, int size, byte precision, byte scale);

		string GetQueryValueSyntaxe();
	}
}
