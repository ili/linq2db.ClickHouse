using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToDB.DataProvider.ClickHouse
{
	using SqlQuery;
	using SqlProvider;
	using LinqToDB.Mapping;
	using System.Data;

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
					return sb/*.Append('_')*/.Append(value);

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
					return value.Length > 0 && value[0] == '{'
						? sb.Append(value.Substring(1))
						: sb.Append(value);
			}

			return sb.Append(value);
		}

		protected override void BuildCreateTableFieldType(SqlField field)
		{
			if (field.CanBeNull)
				StringBuilder.Append("Nullable(");

			BuildDataType(new SqlDataType(field.Type!.Value), true);

			if (field.CanBeNull)
				StringBuilder.Append(")");
		}

		protected override void BuildEndCreateTableStatement(SqlCreateTableStatement createTable)
		{
			base.BuildEndCreateTableStatement(createTable);
			var objectType = createTable.Table!.ObjectType!;
			var primaryKeys = createTable.Table!.Fields
				.Where(_ => _.Value.IsPrimaryKey)
				.OrderBy(_ => _.Value.PrimaryKeyOrder)
				.ToList()
				;
			
			var enginaAttribute = MappingSchema.GetAttribute<TableEngineAttribute>(objectType)
				?? ((primaryKeys.Count > 0) 
					? TableEngineAttribute.MergeTree 
					: TableEngineAttribute.StripeLog);

			StringBuilder
				.Append("ENGINE = ")
				.Append(enginaAttribute.Engine)
				.AppendLine();



			var keys = new StringBuilder();
			foreach (var pk in primaryKeys)
			{
				Convert(keys, pk.Value.PhysicalName, ConvertType.NameToQueryField);
				keys.Append(", ");
			}
			if (keys.Length > 2)
				keys.Length -= 2;

			var tablePrimaryKeys = keys.ToString();
			
			var pimaryKeyStatement = enginaAttribute.PrimaryKeys ?? tablePrimaryKeys;
			var orderByStatement = enginaAttribute.OrderBy ?? tablePrimaryKeys;

			if (!string.IsNullOrEmpty(pimaryKeyStatement))
				StringBuilder.Append("PRIMARY KEY (").Append(pimaryKeyStatement).Append(")").AppendLine();
			
			if (!string.IsNullOrEmpty(orderByStatement))
				StringBuilder.Append("ORDER BY (").Append(orderByStatement).Append(")").AppendLine();

			if (!string.IsNullOrEmpty(enginaAttribute.PartitionBy))
				StringBuilder.Append("PARTITION BY (").Append(enginaAttribute.PartitionBy).Append(")").AppendLine();

			if (!string.IsNullOrEmpty(enginaAttribute.SampleBy))
				StringBuilder.Append("SAMPLE BY (").Append(enginaAttribute.SampleBy).Append(")").AppendLine();

			if (!string.IsNullOrEmpty(enginaAttribute.Ttl))
				StringBuilder.Append("TTL ").Append(enginaAttribute.Ttl).AppendLine();

			if (!string.IsNullOrEmpty(enginaAttribute.Settings))
				StringBuilder.Append("SETTINGS ").Append(enginaAttribute.Settings).AppendLine();
		}

		protected override void BuildCreateTablePrimaryKey(SqlCreateTableStatement createTable, string pkName, IEnumerable<string> fieldNames)
		{
			while (StringBuilder[StringBuilder.Length - 1] != ',')
				StringBuilder.Length--;
			StringBuilder.Length--;
		}

		protected override void BuildCreateTableNullAttribute(SqlField field, DefaultNullable defaultNullable)
		{
			return;
		}

		protected override void BuildDataTypeFromDataType(SqlDataType type, bool forCreateTable)
		{
			if (type.CanBeNull)
				StringBuilder.Append("Nullable(");

			var builded      = true;
			var ignoreLength = !forCreateTable;

			switch (type.Type.DataType)
			{
				case DataType.Undefined:
				case DataType.Char:
				case DataType.VarChar:
				case DataType.Text:
				case DataType.NChar:
				case DataType.NVarChar:
				case DataType.NText:
					ignoreLength = true;
					StringBuilder.Append("String");
					break;
				case DataType.Binary:
				case DataType.VarBinary:
				case DataType.Blob:
				case DataType.Image:
					StringBuilder.Append("String");
					break;
				case DataType.Guid:
					StringBuilder.Append("UUID");
					break;
				case DataType.SByte:
					StringBuilder.Append("Int8");
					break;
				case DataType.Byte:
				case DataType.Boolean:
					StringBuilder.Append("UInt8");
					break;
				case DataType.Int16:
				case DataType.Int64:
				case DataType.UInt16:
				case DataType.UInt32:
				case DataType.UInt64:
					StringBuilder.Append(type.Type.DataType);
					break;
				case DataType.Single:
					StringBuilder.Append("Float32");
					break;
				case DataType.Double:
					StringBuilder.Append("Float64");
					break;
				case DataType.Decimal:
				case DataType.Money:
				case DataType.SmallMoney:
				case DataType.VarNumeric:
					StringBuilder.Append("Decimal");
					if (type.Type.Precision == null)
						StringBuilder.Append("64(8)");
					break;
				case DataType.Date:
				case DataType.SmallDateTime:
				case DataType.Time:
					StringBuilder.Append("DateTime");
					break;
				case DataType.DateTime:
				case DataType.DateTime2:
				case DataType.DateTimeOffset:
				case DataType.Timestamp:
					StringBuilder.Append("DateTime64");
					break;
				case DataType.Xml:
				case DataType.Variant:
				case DataType.Udt:
				case DataType.Dictionary:
				case DataType.Cursor:
				case DataType.Json:
				case DataType.BinaryJson:
				case DataType.Structured:
				case DataType.Long:
				case DataType.LongRaw:
				case DataType.Interval:
				case DataType.BFile:
					StringBuilder.Append("String");
					ignoreLength = true;
					break;
				case DataType.BitArray:
					StringBuilder.Append("Array(UInt8)");
					break;
				default:
					builded = false; break;
			}

			if (!builded)
				base.BuildDataTypeFromDataType(type, forCreateTable);
			else
			{
				if (!ignoreLength && type.Type.Length > 0)
					StringBuilder.Append('(').Append(type.Type.Length).Append(')');

				if (type.Type.Precision > 0)
					StringBuilder.Append('(').Append(type.Type.Precision).Append(',').Append(type.Type.Scale).Append(')');
			}

			if (type.CanBeNull)
				StringBuilder.Append(")");
		}

		protected override void BuildDropTableStatement(SqlDropTableStatement dropTable)
		{
			BuildDropTableStatementIfExists(dropTable);
		}

		protected override void BuildMergeStatement(SqlMergeStatement merge)
		{
			throw new LinqToDBException($"{Name} provider doesn't support SQL MERGE statement");
		}

		protected override void BuildInsertQuery(SqlStatement statement, SqlInsertClause insertClause, bool addAlias)
		{
			base.BuildInsertQuery(statement, insertClause, addAlias);
		}

		protected override void BuildLikePredicate(SqlPredicate.Like predicate)
		{
			var precedence = GetPrecedence(predicate);
			StringBuilder.Append(predicate.IsNot ? " notLike(" : " like(");

			BuildExpression(precedence, predicate.Expr1);

			StringBuilder.Append(" , ");

			BuildExpression(precedence, predicate.Expr2);

			StringBuilder.Append(")");
		}

		protected override void BuildFunction(SqlFunction func)
		{
			if (func.Name.Equals("CAST", StringComparison.OrdinalIgnoreCase))
			{
				StringBuilder.Append("CAST(");
				BuildExpression(func.Parameters[0]);
				StringBuilder.Append(" AS ");
				BuildExpression(func.Parameters[1]);
				StringBuilder.Append(")");

				return;
			}
			base.BuildFunction(func);
		}

		protected override void BuildSetOperation(SetOperation operation, StringBuilder sb)
		{
			switch (operation)
			{
				case SetOperation.Union:
					sb.Append("UNION ALL");
					break;
				default:
					base.BuildSetOperation(operation, sb);
					break;
			}
		}

		internal static bool InlineParameter(SqlParameter parameter)
		{
			return parameter.Value == null ||
				   parameter.Type.DataType == DataType.Guid ||
				   parameter.Type.SystemType == typeof(string);
		}

		protected override void BuildParameter(SqlParameter parameter)
		{
			if (InlineParameter(parameter))
			{
				BuildValue(new SqlDataType(parameter.Type), parameter.Value);
				return;
			}

			StringBuilder.Append("{");
			base.BuildParameter(parameter);
			StringBuilder.Append(":");
			BuildDataType(new SqlDataType(parameter.Type), false);
			StringBuilder.Append("}");
		}

		protected override void BuildSql(int commandNumber, SqlStatement statement, StringBuilder sb, int indent, bool skipAlias)
		{
			base.BuildSql(commandNumber, statement, sb, indent, false);
		}

		protected override void BuildColumnExpression(SelectQuery? selectQuery, ISqlExpression expr, string? alias, ref bool addAlias)
		{
			base.BuildColumnExpression(selectQuery, expr, alias, ref addAlias);
			addAlias = true;
		}
	}
}
