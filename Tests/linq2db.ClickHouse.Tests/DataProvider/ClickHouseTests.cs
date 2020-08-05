using LinqToDB.DataProvider.ClickHouse;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Tests;

namespace LinqToDB.ClickHouse.Tests.DataProvider
{
	[TestFixture]
	public class ClickHouseTests: TestBase
	{
		[Test]
		public void CreateDataProviderTests([DataSources] string providerName)
		{
			var dp = new ClickHouseDataProvider(providerName);
			Assert.NotNull(dp);

			Assert.NotNull(dp.CreateConnection(string.Empty));
		}
	}
}
