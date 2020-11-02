using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB
{
	/// <summary>
	/// ClickHouse SQL Extensions
	/// </summary>
	class SqlCh
	{
		[Sql.Function]
		public static decimal? RoundBankers(decimal? value)
			=> Sql.RoundToEven(value);

		[Sql.Function]
		public static double? RoundBankers(double? value)
			=> Sql.RoundToEven(value);

		[Sql.Function]
		public static decimal? RoundBankers(decimal? value, int? precision)
			=> Sql.RoundToEven(value, precision);

		[Sql.Function]
		public static double? RoundBankers(double? value, int? precision)
			=> Sql.RoundToEven(value, precision);
	}
}
