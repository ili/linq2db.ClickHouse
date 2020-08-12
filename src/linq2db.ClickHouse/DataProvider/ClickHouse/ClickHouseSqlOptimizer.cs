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

		public override ISqlExpression ConvertExpression(ISqlExpression expression)
		{
			if (expression is SqlParameter par)
				expression = new SqlValue(par.Type, par.Value);

			if (expression is SqlBinaryExpression bex && bex.Operation[0] == '+' && bex.SystemType == typeof(string))
				expression = new SqlFunction(typeof(string), "concat", bex.Expr1, bex.Expr2);

			if (expression is SqlFunction fex && fex.Name.Equals("convert", StringComparison.OrdinalIgnoreCase))
				expression = new SqlFunction(fex.SystemType, "CAST", fex.IsAggregate, fex.IsPure, fex.Precedence, fex.Parameters[1], fex.Parameters[0]);

			return base.ConvertExpression(expression);
		}

		public override SqlStatement Finalize(SqlStatement statement, bool inlineParameters)
		{
			return base.Finalize(statement, true);
		}
	}
}
