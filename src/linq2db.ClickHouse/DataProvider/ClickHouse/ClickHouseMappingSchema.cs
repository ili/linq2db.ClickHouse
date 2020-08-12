using System;
using System.Globalization;
using System.Text;
using System.Linq;

namespace LinqToDB.DataProvider.ClickHouse
{
	using LinqToDB.Metadata;
	using Mapping;
	using SqlQuery;
	using System.Data.Linq;
	using System.Reflection;

	public class ClickHouseMappingSchema : MappingSchema
	{
		protected ClickHouseMappingSchema(string configuration) : base(configuration)
		{
		}

		public static readonly ClickHouseMappingSchema Instance = new ClickHouseMappingSchema(nameof(ClickHouseMappingSchema));

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

		private class WrappedMetadataReader : IMetadataReader
		{
			private readonly IMetadataReader _source;

			public WrappedMetadataReader(IMetadataReader source)
			{
				_source = source;
			}

			public T[] GetAttributes<T>(Type type, bool inherit = true) where T : Attribute
				=> Convert(_source.GetAttributes<T>(type, inherit));

			private T[] Convert<T>(T[] attributes) where T : Attribute
			{
				if (typeof(ColumnAttribute).IsAssignableFrom(typeof(T)))
					foreach (var ca in attributes.OfType<ColumnAttribute>())
						ca.IsIdentity = false;

				return attributes;
			}

			public T[] GetAttributes<T>(Type type, MemberInfo memberInfo, bool inherit = true) where T : Attribute
				=> Convert(_source.GetAttributes<T>(type, memberInfo, inherit));


			public MemberInfo[] GetDynamicColumns(Type type)
				=> _source.GetDynamicColumns(type);
			
		}

		private class ClickHouseMetadataReader : MetadataReader
		{
			 
		}
	}
}
