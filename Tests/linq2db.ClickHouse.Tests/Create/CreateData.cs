using System;
using System.Data;
using System.IO;
using System.Linq;

using LinqToDB;
using LinqToDB.ClickHouse.Tests;
using LinqToDB.Data;
using LinqToDB.DataProvider.Access;
using LinqToDB.DataProvider.ClickHouse;
using NUnit.Framework;
using Tests;
using Tests.Model;

// for unknown reason order doesn't help on ubuntu 18, so namespace were removed and class name changed to be first in
// sort order
[TestFixture]
[Category(Tests.TestCategory.Create)]
[Order(-1)]
// ReSharper disable once InconsistentNaming
// ReSharper disable once TestClassNameSuffixWarning
public class a_CreateData : TestBase
{
	static void RunScript(string configString, string divider, string name, Action<IDbConnection>? action = null, string? database = null)
		=> TestDataConnection.RunScript(
			configString,
			divider,
			name,
			false,
			action,
			database);

	[Test, Order(0)]
	public void CreateDatabase([CreateDatabaseSources] string context)
	{
		switch (context)
		{
			case ClickHouseProviderAdapter.TcpProviderName     : RunScript(context,           ";",      "ClickHouse", null);         break;
			case ClickHouseProviderAdapter.HttpProviderName    : RunScript(context,           ";",      "ClickHouse", null);         break;
			case ProviderName.SQLiteMS                         : RunScript(context,           "\nGO\n", "SQLite",     SQLiteAction);
			                                                     RunScript(context + ".Data", "\nGO\n", "SQLite",     SQLiteAction); break;
			default                                            : throw new InvalidOperationException(context);
		}
	}

	static void SQLiteAction(IDbConnection connection)
	{
		using (var conn = LinqToDB.DataProvider.SQLite.SQLiteTools.CreateDataConnection(connection))
		{
			conn.Execute(@"
				UPDATE AllTypes
				SET
					binaryDataType           = @binaryDataType,
					varbinaryDataType        = @varbinaryDataType,
					imageDataType            = @imageDataType,
					uniqueidentifierDataType = @uniqueidentifierDataType
				WHERE ID = 2",
				new
				{
					binaryDataType = new byte[] { 1 },
					varbinaryDataType = new byte[] { 2 },
					imageDataType = new byte[] { 0, 0, 0, 3 },
					uniqueidentifierDataType = new Guid("{6F9619FF-8B86-D011-B42D-00C04FC964FF}"),
				});
		}
	}

}
