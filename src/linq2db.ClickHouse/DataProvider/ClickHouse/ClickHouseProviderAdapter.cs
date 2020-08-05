using System;
using System.Data;
using System.Linq;

namespace LinqToDB.DataProvider.ClickHouse
{
	public class ClickHouseProviderAdapter : IDynamicProviderAdapter
	{
		private static readonly object _tcpSyncRoot  = new object();
		private static readonly object _jsonSyncRoot = new object();

		private static ClickHouseProviderAdapter? _tcpAdapter;
		private static ClickHouseProviderAdapter? _jsonAdapter;

		public  const string TcpProviderName       = "ClickHouse.Ado";
		private const string TcpClientAssemblyName = "ClickHouse.Ado";
		private const string TcpClientNamespace    = "ClickHouse.Ado";

		public  const string HttpProviderName        = "ClickHouse.Client";
		private const string HttpClientAssemblyName  = "ClickHouse.Client";
		private const string HttpClientNamespace     = "ClickHouse.Client.ADO";

		private ClickHouseProviderAdapter(
			Type connectionType,
			Type dataReaderType,
			Type parameterType,
			Type commandType,
			Type transactionType)
		{
			ConnectionType  = connectionType;
			DataReaderType  = dataReaderType;
			ParameterType   = parameterType;
			CommandType = commandType;
			TransactionType = transactionType;
		}

		public Type ConnectionType  { get; }
		public Type DataReaderType  { get; }
		public Type ParameterType   { get; }
		public Type CommandType     { get; }
		public Type TransactionType { get; }

		private class FakeTransaction : System.Data.IDbTransaction
		{
			public IDbConnection Connection => null;

			public IsolationLevel IsolationLevel => IsolationLevel.Unspecified;

			public void Commit()
			{
			}

			public void Dispose()
			{
			}

			public void Rollback()
			{
			}
		}

		private static ClickHouseProviderAdapter CreateAdapter(string assemblyName, string clientNamespace, string prefix)
		{
			var assembly = Common.Tools.TryLoadAssembly(assemblyName, null);
			if (assembly == null)
				throw new InvalidOperationException($"Cannot load assembly {assemblyName}");

			var types = assembly.GetTypes();

			var connectionType  = types.Single         (_ => _.IsPublic && _.GetInterfaces().Contains(typeof(IDbConnection)))!;
			var dataReaderType  = types.Single         (_ => _.IsPublic && _.GetInterfaces().Contains(typeof(IDataReader)))!;
			var parameterType   = types.Single         (_ => _.IsPublic && _.GetInterfaces().Contains(typeof(IDbDataParameter)))!;
			var commandType     = types.Single         (_ => _.IsPublic && _.GetInterfaces().Contains(typeof(IDbCommand)))!;
			var transactionType = types.SingleOrDefault(_ => _.IsPublic && _.GetInterfaces().Contains(typeof(IDbTransaction))) ?? typeof(FakeTransaction); // assembly.GetType($"{clientNamespace}.{prefix}Transaction", true)!;

			return new ClickHouseProviderAdapter(
				connectionType,
				dataReaderType,
				parameterType,
				commandType,
				transactionType);
		}

		public static ClickHouseProviderAdapter GetInstance(string name)
		{
			if (name == TcpProviderName)
			{
				if (_tcpAdapter == null)
					lock (_tcpSyncRoot)
						if (_tcpAdapter == null)
							_tcpAdapter = CreateAdapter(TcpClientAssemblyName, TcpClientNamespace, "ClickHouse");

				return _tcpAdapter;
			}
			else
			{
				if (_jsonAdapter == null)
					lock (_jsonSyncRoot)
						if (_jsonAdapter == null)
							_jsonAdapter = CreateAdapter(HttpClientAssemblyName, HttpClientNamespace, "ClickHouse");

				return _jsonAdapter;
			}
		}
	}
}
