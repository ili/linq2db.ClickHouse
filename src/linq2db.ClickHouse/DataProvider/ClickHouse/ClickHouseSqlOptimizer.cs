using LinqToDB.Extensions;
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
				// FIX ClickHouse SQL issue
				// https://github.com/ClickHouse/ClickHouse/issues/14978
				if (x is SqlColumn cex)
					cex.Alias = (cex.Alias ?? "") + "_";
			});

			statement = QueryHelper.WrapQuery(statement,
				select => select.SetOperators.Any(_ => _.Operation == SetOperation.Union) == true,
				(a, b) => 
				{
					a.Select.IsDistinct = true;
				}
				);

			return base.OptimizeStatement(statement, inlineParameters, withParameters, remoteContext);
		}

		public override ISqlExpression ConvertExpression(ISqlExpression expression, bool withParameters)
		{
			if (expression is SqlBinaryExpression bex && bex.Operation[0] == '+' && bex.SystemType == typeof(string))
				expression = new SqlFunction(typeof(string), "concat",
					Cast(bex.Expr1, typeof(string)),
					Cast(bex.Expr2, typeof(string)));

			if (expression is SqlFunction fex && fex.Name.Equals("convert", StringComparison.OrdinalIgnoreCase))
			{
				if (fex.Parameters[0].SystemType == typeof(bool) && 
					(fex.Parameters[1].SystemType!.IsFloatType() || fex.Parameters[1].SystemType!.IsIntegerType()))
				{
					expression = new SqlBinaryExpression(typeof(bool), fex.Parameters[1], "!=", new SqlValue(0));
				}
				else
					expression = new SqlFunction(fex.SystemType, "CAST", fex.IsAggregate, fex.IsPure, fex.Precedence, fex.Parameters[1], fex.Parameters[0]);
			}

			if (expression is SqlBinaryExpression dbex && dbex.Expr1.SystemType != dbex.Expr2.SystemType
				&& (dbex.Expr1.SystemType == typeof(decimal) || dbex.Expr2.SystemType == typeof(decimal)))
			{
				expression = new SqlBinaryExpression(typeof(decimal),
					Cast(dbex.Expr1, typeof(decimal)),
					dbex.Operation,
					Cast(dbex.Expr2, typeof(decimal)));
			}

			return base.ConvertExpression(expression, withParameters);
		}

		public override ISqlPredicate ConvertPredicate(SelectQuery selectQuery, ISqlPredicate predicate, bool withParameters)
		{
			if (predicate is SqlPredicate.ExprExpr ee && ee.Expr1.SystemType != ee.Expr2.SystemType
				&& (ee.Expr1.SystemType == typeof(decimal) || ee.Expr2.SystemType == typeof(decimal)))
			{
				predicate = new SqlPredicate.ExprExpr(Cast(ee.Expr1, typeof(decimal)),
					ee.Operator,
					Cast(ee.Expr2, typeof(decimal)));
			}

			return base.ConvertPredicate(selectQuery, predicate, withParameters);
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
