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

	using static LinqToDB.Linq.Expressions;

	public class ClickHouseMappingSchema : MappingSchema
	{
		protected ClickHouseMappingSchema(string configuration) : base(configuration)
		{
			MapMembers(configuration);

			SetValueToSqlConverter(typeof(byte[]), (sb, dt, v) => ConvertBinaryToSql(sb, (byte[])v));
			SetValueToSqlConverter(typeof(Binary), (sb, dt, v) => ConvertBinaryToSql(sb, ((Binary)v).ToArray()));

			SetConverter<string, byte[]>(ToRawByteArray);
		}

		private static byte[] ToRawByteArray(string input)
		{
			if (input == null)
				return Array.Empty<byte>();
		
			var result = new byte[input.Length];
			for (int i = 0; i < input.Length; i++)
			{
				result[i] = (byte)input[i];
			}

			return result;
		}

		void MapMembers(string configuration)
		{
			var builder = GetFluentMappingBuilder();

			builder
				.HasAttribute(M(() => Sql.NewGuid()),      new Sql.FunctionAttribute(configuration, "generateUUIDv4") { ServerSideOnly = true, CanBeNull = false })
				.HasAttribute((string p) => Sql.Length(p), new Sql.FunctionAttribute(configuration, "lengthUTF8"))
				.HasAttribute((string p) => Sql.Lower(p),  new Sql.FunctionAttribute(configuration, "lowerUTF8"))
				.HasAttribute((string p) => Sql.Upper(p),  new Sql.FunctionAttribute(configuration, "upperUTF8"))
			//.HasAttribute((double? p) => Sql.Floor(p), new Sql.FunctionAttribute(configuration, "floor"))
			;
		}

		static void ConvertBinaryToSql(StringBuilder stringBuilder, byte[] value)
		{
			stringBuilder.Append("'");
			ToHexString(stringBuilder, value);
			stringBuilder.Append("'");
		}

		internal static StringBuilder ToHexString(StringBuilder stringBuilder, byte[] value)
		{
			foreach (var b in value)
				stringBuilder
					.Append("\\x")
					.Append(b.ToString("X2"));

			return stringBuilder;
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
