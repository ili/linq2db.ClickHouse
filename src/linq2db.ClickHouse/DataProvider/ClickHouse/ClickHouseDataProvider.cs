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

			_sqlOptimizer = new ClickHouseSqlOptimizer(SqlProviderFlags);
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
			base.SetParameter(dataConnection, parameter, "@" + name, dataType, value);
		}

		protected override void SetParameterType(DataConnection dataConnection, IDbDataParameter parameter, DbDataType dataType)
		{
			switch (dataType.DataType)
			{
				case DataType.UInt32    : dataType = dataType.WithDataType(DataType.Int64);    break;
				case DataType.UInt64    : dataType = dataType.WithDataType(DataType.Decimal);  break;
				case DataType.DateTime2 : dataType = dataType.WithDataType(DataType.DateTime); break;
			}

			base.SetParameterType(dataConnection, parameter, dataType);
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
