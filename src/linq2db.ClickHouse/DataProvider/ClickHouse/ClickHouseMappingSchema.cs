using System;
using System.Globalization;
using System.Text;

namespace LinqToDB.DataProvider.ClickHouse
{
	using Mapping;
	using SqlQuery;
	using System.Data.Linq;

	public class ClickHouseMappingSchema : MappingSchema
	{
		public ClickHouseMappingSchema() : this(ClickHouseProviderAdapter.TcpProviderName)
		{
		}

		protected ClickHouseMappingSchema(string configuration) : base(configuration)
		{
		}

		internal static readonly ClickHouseMappingSchema Instance = new ClickHouseMappingSchema();

		public class TcpMappingSchema : MappingSchema
		{
			public TcpMappingSchema()
				: base(ClickHouseProviderAdapter.TcpProviderName, Instance)
			{
			}
		}

		public class HttpMappingSchema : MappingSchema
		{
			public HttpMappingSchema()
				: base(ClickHouseProviderAdapter.HttpProviderName, Instance)
			{
			}
		}
	}
}
