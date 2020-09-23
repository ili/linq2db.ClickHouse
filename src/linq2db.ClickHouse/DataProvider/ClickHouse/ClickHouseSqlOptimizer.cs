using LinqToDB.SqlProvider;
using LinqToDB.SqlQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToDB.DataProvider.ClickHouse
{
	class ClickHouseSqlOptimizer : BasicSqlOptimizer
	{
		public ClickHouseSqlOptimizer(SqlProviderFlags sqlProviderFlags)
			: base(sqlProviderFlags) { }

		public override SqlStatement OptimizeStatement(SqlStatement statement, bool inlineParameters, bool withParameters, bool remoteContext)
		{
			var visitor = new QueryVisitor();

			visitor.VisitAll(statement, x => 
			{
				if (x is SqlParameter parameter)
					statement.IsParameterDependent = statement.IsParameterDependent ||
					                                 parameter.CanBeNull ||
					                                 ClickHouseSqlBuilder.InlineParameter(parameter);
			});

			return base.OptimizeStatement(statement, inlineParameters, withParameters, remoteContext);
		}

		public override ISqlExpression ConvertExpression(ISqlExpression expression, bool withParameters)
		{
			if (expression is SqlBinaryExpression bex && bex.Operation[0] == '+' && bex.SystemType == typeof(string))
				expression = new SqlFunction(typeof(string), "concat",
					Cast(bex.Expr1, typeof(string)),
					Cast(bex.Expr2, typeof(string)));

			if (expression is SqlFunction fex && fex.Name.Equals("convert", StringComparison.OrdinalIgnoreCase))
				expression = new SqlFunction(fex.SystemType, "CAST", fex.IsAggregate, fex.IsPure, fex.Precedence, fex.Parameters[1], fex.Parameters[0]);

			return base.ConvertExpression(expression, withParameters);
		}

		private ISqlExpression Cast(ISqlExpression expr, Type type)
		{
			if (expr.SystemType == type)
				return expr;

			return new SqlFunction(type, "CAST", expr, new SqlDataType(type));
		}

		public override SqlStatement Finalize(SqlStatement statement, bool inlineParameters)
		{
			return base.Finalize(statement, true);
		}
	}
}
