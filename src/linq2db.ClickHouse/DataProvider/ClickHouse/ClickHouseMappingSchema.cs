using System;
using System.Globalization;
using System.Text;
using System.Linq;

namespace LinqToDB.DataProvider.ClickHouse
{
	using LinqToDB.Expressions;
	using LinqToDB.Metadata;
	using LinqToDB.SqlQuery;
	using Mapping;
	using SqlQuery;
	using System.Data.Linq;
	using System.Data.SqlTypes;
	using System.Linq.Expressions;
	using System.Reflection;

	using static LinqToDB.Linq.Expressions;

	public class ClickHouseMappingSchema : MappingSchema
	{
		protected ClickHouseMappingSchema(string configuration) : base(configuration)
		{
			MapMembers(configuration);

			SetValueToSqlConverter(typeof(byte[]),      (sb, dt, v) => ConvertBinaryToSql(sb, (byte[])v));
			SetValueToSqlConverter(typeof(Binary),      (sb, dt, v) => ConvertBinaryToSql(sb, ((Binary)v).ToArray()));
			SetValueToSqlConverter(typeof(DateTime),    (sb, bt, v) => BuildDateTime(sb, (DateTime)v));
			SetValueToSqlConverter(typeof(SqlDateTime), (sb, dt, v) => BuildDateTime(sb, (DateTime)(SqlDateTime)v));

			SetConverter<string, byte[]>(ToRawByteArray);
		}

		private static void BuildDateTime(StringBuilder sb, DateTime dateTime)
			=> sb.AppendFormat("'{0:yyyy-MM-dd HH:mm:ss.fff}'", dateTime);

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
				.HasAttribute(M(() => Sql.NewGuid()),      new Sql.FunctionAttribute (configuration, "generateUUIDv4") { ServerSideOnly = true, CanBeNull = false })
				.HasAttribute((string p) => Sql.Length(p), new Sql.FunctionAttribute (configuration, "lengthUTF8"))
				.HasAttribute((string p) => Sql.Lower(p),  new Sql.FunctionAttribute (configuration, "lowerUTF8"))
				.HasAttribute((string p) => Sql.Upper(p),  new Sql.FunctionAttribute (configuration, "upperUTF8"))
				.HasAttribute((Sql.DateParts p) => Sql.DatePart(p, null as DateTime?), 
					new Sql.ExtensionAttribute(configuration, "") { ServerSideOnly = false, PreferServerSide = false, BuilderType = typeof(DatePartBuilderClickHouse) })

				.HasAttribute((double?  p) => Sql.RoundToEven(p),            new Sql.FunctionAttribute(configuration, "roundBankers"))
				.HasAttribute((decimal? p) => Sql.RoundToEven(p),            new Sql.FunctionAttribute(configuration, "roundBankers"))
				.HasAttribute((double?  p) => Sql.RoundToEven(p, 0 as int?), new Sql.FunctionAttribute(configuration, "roundBankers"))
				.HasAttribute((decimal? p) => Sql.RoundToEven(p, 0 as int?), new Sql.FunctionAttribute(configuration, "roundBankers"))

				.HasAttribute((int      p) => Sql.Decimal(p),                new Sql.ExpressionAttribute(configuration, "Decimal({0}, 4)") { ServerSideOnly = true })
				.HasAttribute(M(()         => Sql.DefaultDecimal),           new Sql.PropertyAttribute  (configuration, "Decimal(32,  10)") { ServerSideOnly = true })
				.HasAttribute(M(()         => Sql.Money),                    new Sql.PropertyAttribute  (configuration, "Decimal(32,  10)") { ServerSideOnly = true })
				.HasAttribute(M(()         => Sql.SmallMoney),               new Sql.PropertyAttribute  (configuration, "Decimal(32,  10)") { ServerSideOnly = true })
				
				.HasAttribute(M(()         => Sql.DateTime2),                new Sql.PropertyAttribute  (configuration, "DateTime64") { ServerSideOnly = true })
				.HasAttribute(M(()         => Sql.SmallDateTime),            new Sql.PropertyAttribute  (configuration, "DateTime")   { ServerSideOnly = true })
			;

			MapMember(configuration, (double?  p)         => Sql.RoundToEven(p),    (double?  p)         => SqlCh.RoundBankers(p));
			MapMember(configuration, (decimal? p)         => Sql.RoundToEven(p),    (decimal? p)         => SqlCh.RoundBankers(p));
			MapMember(configuration, (double?  p, int? i) => Sql.RoundToEven(p, i), (double?  p, int? i) => SqlCh.RoundBankers(p, i));
			MapMember(configuration, (decimal? p, int? i) => Sql.RoundToEven(p, i), (decimal? p, int? i) => SqlCh.RoundBankers(p, i));

			// https://github.com/ClickHouse/ClickHouse/issues/5690
			ValueToSqlConverter.SetConverter(typeof(decimal),  (sb, dt, o) => sb.AppendFormat(CultureInfo.InvariantCulture, "CAST({0} as decimal(32, 10))", o));
			ValueToSqlConverter.SetConverter(typeof(decimal?), (sb, dt, o) => sb.AppendFormat(CultureInfo.InvariantCulture, "CAST({0} as decimal(32, 10))", o));
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

		class DatePartBuilderClickHouse : Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				string partStr;
				var part = builder.GetValue<Sql.DateParts>("part");
				switch (part)
				{
					case Sql.DateParts.Year        : partStr = "toYear({date})";                                                     break;
					case Sql.DateParts.Quarter     : partStr = "(toMonth({date}) % 3)";                                              break;
					case Sql.DateParts.Month       : partStr = "toMonth({date})";                                                    break;
					case Sql.DateParts.DayOfYear   : partStr = "dateDiff('day', toStartOfYear({date}), {date})";                     break;
					case Sql.DateParts.Day         : partStr = "toDayOfMonth({date})";                                               break;
					case Sql.DateParts.Week        : partStr = "dateDiff('week', toStartOfYear({date}), {date})";                    break;
					case Sql.DateParts.WeekDay     : partStr = "toDayOfWeek({date})";                                                break;
					case Sql.DateParts.Hour        : partStr = "toHour({date})";                                                     break;
					case Sql.DateParts.Minute      : partStr = "toMinute({date})";                                                   break;
					case Sql.DateParts.Second      : partStr = "toSecond({date})";                                                   break;
					case Sql.DateParts.Millisecond : partStr = "(toUnixTimestamp({date}) - toUnixTimestamp(toStartOfSecond({date})"; break;
					default:
						throw new ArgumentOutOfRangeException(part.ToString());
				}

				builder.Expression = partStr;
			}
		}

	}
}
