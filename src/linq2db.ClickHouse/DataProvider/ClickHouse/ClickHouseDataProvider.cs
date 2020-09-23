using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

namespace LinqToDB.DataProvider.ClickHouse
{
	using Data;
	using Common;
	using Extensions;
	using Mapping;
	using SchemaProvider;
	using SqlProvider;
	using System.Threading.Tasks;
	using System.Threading;
	using System.Net.Http;
	using System.Net;

	public class ClickHouseDataProvider : DynamicDataProviderBase<ClickHouseProviderAdapter>
	{
		/// <summary>
		/// Creates the specified ClickHouse provider based on the provider name.
		/// </summary>
		/// <param name="name">Provider name</param>
		public ClickHouseDataProvider(string name)
			: this(name, MappingSchemaInstance.Get(name))
		{
		}

		protected ClickHouseDataProvider(string name, MappingSchema mappingSchema)
			: base(name, mappingSchema, ClickHouseProviderAdapter.GetInstance(name))
		{
			SqlProviderFlags.IsSkipSupported                   = false;
			SqlProviderFlags.IsSkipSupportedIfTake             = true;
			SqlProviderFlags.IsInsertOrUpdateSupported         = false;
			SqlProviderFlags.IsUpdateSetTableAliasSupported    = false;
			SqlProviderFlags.IsCommonTableExpressionsSupported = true;
			SqlProviderFlags.IsDistinctOrderBySupported        = true;
			SqlProviderFlags.IsSubQueryOrderBySupported        = true;
			SqlProviderFlags.IsDistinctSetOperationsSupported  = true;
			SqlProviderFlags.IsUpdateFromSupported             = false;
			SqlProviderFlags.DefaultMultiQueryIsolationLevel   = IsolationLevel.Unspecified;
			SqlProviderFlags.IsDeleteSupported                 = false;
			SqlProviderFlags.IsUpdateSupported                 = false;
			SqlProviderFlags.IsInsertWithIdentitySupported     = false;
			SqlProviderFlags.IsAffectedRowsSupported           = false;

			_sqlOptimizer = new ClickHouseSqlOptimizer(SqlProviderFlags);

			SetProviderField<IDataReader, byte[], string>((r, i) => (byte[])r.GetValue(i));
		}

		protected override string? NormalizeTypeName(string? typeName)
		{
			return typeName;
		}


		public override ISqlBuilder CreateSqlBuilder(MappingSchema mappingSchema)
		{
			return new ClickHouseSqlBuilder(mappingSchema, GetSqlOptimizer(), SqlProviderFlags);
		}

		static class MappingSchemaInstance
		{
			public static readonly MappingSchema TcpMappingSchema  = new ClickHouseMappingSchema.TcpMappingSchema();
			public static readonly MappingSchema HttpMappingSchema = new ClickHouseMappingSchema.HttpMappingSchema();

			public static MappingSchema Get(string name) => name == ClickHouseProviderAdapter.TcpProviderName ? TcpMappingSchema : HttpMappingSchema;
		}

		readonly ISqlOptimizer _sqlOptimizer;

		public override ISqlOptimizer GetSqlOptimizer() => _sqlOptimizer;

		public override ISchemaProvider GetSchemaProvider()
		{
			return new ClickHouseSchemaProvider();
		}


		public override void SetParameter(DataConnection dataConnection, IDbDataParameter parameter, string name, DbDataType dataType, object? value)
		{
			var convertToString = false;
			var encoding = null as System.Text.Encoding;

			switch (dataType.DataType)
			{
				case DataType.Undefined:
				case DataType.Char:
				case DataType.VarChar:
				case DataType.Text:
				case DataType.NChar:
				case DataType.NVarChar:
				case DataType.NText:
				case DataType.Xml:
				case DataType.Variant:
				case DataType.Json:
					convertToString = true;
					encoding = System.Text.Encoding.UTF8;
					break;
				case DataType.Binary:
				case DataType.VarBinary:
				case DataType.Blob:
				case DataType.Image:
				case DataType.BinaryJson:
					convertToString = true;
					break;
			}

			if (value is bool boolValue)
			{
				value = boolValue ? 1 : 0;
				dataType = dataType.WithDataType(DataType.Byte);
			}

			var arr = (value as byte[]) ?? (value as System.Data.Linq.Binary)?.ToArray();

			if (convertToString && arr != null)
			{
				value = encoding != null
					? encoding.GetString(arr)
					: ClickHouseMappingSchema.ToHexString(new System.Text.StringBuilder(), arr).ToString();
			}

			base.SetParameter(dataConnection, parameter, name, dataType, value);
		}

		private static readonly HttpClient _httpClient = new HttpClient(
			new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			});

		public static IDictionary<string, string> QuerySettings { get; set; } =
		new Dictionary<string, string>
		{
			{ "join_use_nulls", "1" },
		};

		protected override IDbConnection CreateConnectionInternal(string connectionString)
		{
			//return base.CreateConnectionInternal(connectionString);
			var connection = new global::ClickHouse.Client.ADO.ClickHouseConnection(
				connectionString,
				_httpClient);

			foreach (var s in QuerySettings)
				connection.CustomSettings[s.Key] = s.Value;

			return connection;
		}

		#region BulkCopy

		public override BulkCopyRowsCopied BulkCopy<T>(
			ITable<T> table, BulkCopyOptions options, IEnumerable<T> source)
		{
			return new ClickHouseBulkCopy().BulkCopy(
				options.BulkCopyType,
				table,
				options,
				source);
		}

		public override Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			ITable<T> table, BulkCopyOptions options, IEnumerable<T> source, CancellationToken cancellationToken)
		{
			return new ClickHouseBulkCopy().BulkCopyAsync(
				options.BulkCopyType,
				table,
				options,
				source,
				cancellationToken);
		}

#if !NETFRAMEWORK
		public override Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			ITable<T> table, BulkCopyOptions options, IAsyncEnumerable<T> source, CancellationToken cancellationToken)
		{
			return new ClickHouseBulkCopy().BulkCopyAsync(
				options.BulkCopyType,
				table,
				options,
				source,
				cancellationToken);
		}
#endif

		#endregion
	}
}
