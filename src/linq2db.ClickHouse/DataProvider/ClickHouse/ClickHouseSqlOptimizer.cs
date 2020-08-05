using LinqToDB.SqlProvider;
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
	}
}
