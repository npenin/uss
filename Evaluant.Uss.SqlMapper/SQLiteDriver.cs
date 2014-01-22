using System;
using System.Data;

using Evaluant.Uss.Common;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de SqlDataFactory.
	/// </summary>

	public class SQLiteDriver : DeclarativeDriver
	{
		public SQLiteDriver() : 
			base(
                "System.Data.SQLite.SQLiteConnection, System.Data.SQLite",
                "System.Data.SQLite.SQLiteCommand, System.Data.SQLite",
                "System.Data.SQLite.SQLiteParameter, System.Data.SQLite",
				"@"
			)
		{
		}
	}
}
