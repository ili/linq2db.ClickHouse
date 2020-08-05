//using System;
//using System.Collections.Generic;
//using System.Text;
//using LinqToDB.Data;
//using LinqToDB.DataProvider.ClickHouse;

//namespace LinqToDB.ClickHouse.Tests
//{
//	public class TestBase : global::Tests.TestBase
//	{
//		static TestBase()
//		{
//			DataConnection.AddDataProvider(ClickHouseProviderAdapter.TcpProviderName,
//				new ClickHouseDataProvider(ClickHouseProviderAdapter.TcpProviderName));
//			DataConnection.AddDataProvider(ClickHouseProviderAdapter.HttpProviderName,
//				new ClickHouseDataProvider(ClickHouseProviderAdapter.HttpProviderName));
//		}
//	}
//}
