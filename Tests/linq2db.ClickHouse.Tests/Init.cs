using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using LinqToDB.Data;
using LinqToDB.DataProvider.ClickHouse;
using NUnit.Framework;

[SetUpFixture]
public class Init
{
	[OneTimeSetUp]
	public void SetUp()
	{
		DataConnection.AddDataProvider(ClickHouseProviderAdapter.TcpProviderName,
			new ClickHouseDataProvider(ClickHouseProviderAdapter.TcpProviderName));
		DataConnection.AddDataProvider(ClickHouseProviderAdapter.HttpProviderName,
			new ClickHouseDataProvider(ClickHouseProviderAdapter.HttpProviderName));

		// Walkaround to use Yandex.Cloud Ignore SSL
		ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
		{
			return true;
		};
	}
}
