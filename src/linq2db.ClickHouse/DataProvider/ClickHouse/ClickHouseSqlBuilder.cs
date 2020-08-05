using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToDB.DataProvider.ClickHouse
{
	using SqlQuery;
	using SqlProvider;
	using LinqToDB.Mapping;

	public class ClickHouseSqlBuilder : BasicSqlBuilder
	{
		public ClickHouseSqlBuilder(
			MappingSchema    mappingSchema,
			ISqlOptimizer    sqlOptimizer,
			SqlProviderFlags sqlProviderFlags)
			: base(mappingSchema, sqlOptimizer, sqlProviderFlags)
		{
		}


		protected override ISqlBuilder CreateSqlBuilder()
		{
			return new ClickHouseSqlBuilder(MappingSchema, SqlOptimizer, SqlProviderFlags);
		}

		protected override string LimitFormat(SelectQuery selectQuery)
		{
			return "LIMIT {0}";
		}

		protected override string OffsetFormat(SelectQuery selectQuery)
		{
			return "OFFSET {0}";
		}

		public override StringBuilder Convert(StringBuilder sb, string value, ConvertType convertType)
		{
			switch (convertType)
			{
				case ConvertType.NameToQueryParameter:
				case ConvertType.NameToCommandParameter:
				case ConvertType.NameToSprocParameter:
					return sb.Append('@').Append(value);

				case ConvertType.NameToQueryField:
				case ConvertType.NameToQueryFieldAlias:
				case ConvertType.NameToQueryTableAlias:
					if (value.Length > 0 && value[0] == '"')
						return sb.Append(value);

					return sb.Append('"').Append(value).Append('"');

				case ConvertType.NameToDatabase:
				case ConvertType.NameToSchema:
				case ConvertType.NameToQueryTable:
					if (value.Length > 0 && value[0] == '"')
						return sb.Append(value);

					if (value.IndexOf('.') > 0)
						value = string.Join("\".\"", value.Split('.'));

					return sb.Append('"').Append(value).Append('"');

				case ConvertType.SprocParameterToName:
					return value.Length > 0 && value[0] == '@'
						? sb.Append(value.Substring(1))
						: sb.Append(value);
			}

			return sb.Append(value);
		}

		protected override void BuildDataTypeFromDataType(SqlDataType type, bool forCreateTable)
		{
			switch (type.Type.DataType)
			{
				case DataType.Int32 : StringBuilder.Append("INTEGER");                      break;
				default             : base.BuildDataTypeFromDataType(type, forCreateTable); break;
			}
		}


		public override StringBuilder BuildTableName(StringBuilder sb, string? server, string? database, string? schema, string table)
		{
			if (database != null && database.Length == 0) database = null;

			if (database != null)
				sb.Append(database).Append(".");

			return sb.Append(table);
		}

		static bool IsDateTime(Type type)
		{
			return    type == typeof(DateTime)
				   || type == typeof(DateTimeOffset)
				   || type == typeof(DateTime?)
				   || type == typeof(DateTimeOffset?);
		}

		protected override void BuildDropTableStatement(SqlDropTableStatement dropTable)
		{
			BuildDropTableStatementIfExists(dropTable);
		}

		protected override void BuildMergeStatement(SqlMergeStatement merge)
		{
			throw new LinqToDBException($"{Name} provider doesn't support SQL MERGE statement");
		}
	}
}
