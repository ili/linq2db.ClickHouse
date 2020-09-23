using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Tests;

namespace LinqToDB.ClickHouse.Tests.Update
{
	[TestFixture]
	public class NullParametersTests: TestBase
	{
		public class NullParametersTest
		{
			public int?   IntValue;
			public string StringValue;

			public static readonly IEqualityComparer<NullParametersTest> Comparer = new ComparerImpl();
			class ComparerImpl : IEqualityComparer<NullParametersTest>
			{
				public bool Equals([AllowNull] NullParametersTest x, [AllowNull] NullParametersTest y)
					=> x.IntValue == y.IntValue && x.StringValue == y.StringValue;

				public int GetHashCode([DisallowNull] NullParametersTest obj)
				{
					return $"{obj.IntValue}${obj.StringValue}".GetHashCode();
				}
			}
		}

		private NullParametersTest[] _source = new []
		{
			new NullParametersTest { IntValue = null, StringValue = null },
			new NullParametersTest { IntValue =    1, StringValue = "not null string" }
		};

		[Test]
		public void Test([DataSources]string context)
		{
			using (var db = GetDataContext(context))
			using (var t = db.CreateLocalTable<NullParametersTest>())
			{
				foreach (var s in _source)
					db.GetTable<NullParametersTest>()
						.Value(_ => _.IntValue,    () => s.IntValue)
						.Value(_ => _.StringValue, () => s.StringValue)
						.Insert();

				var res = db.GetTable<NullParametersTest>().ToList();

				AreEqual(_source, res, comparer: NullParametersTest.Comparer);

				for (var i = 0; i < _source.Length; i++)
				{
					var selecedByInt = db.GetTable<NullParametersTest>()
						.Where(_ => _.IntValue == _source[i].IntValue)
						.ToList();

					AreEqual(_source.Skip(i).Take(1), selecedByInt, comparer: NullParametersTest.Comparer);

					var selecedByString = db.GetTable<NullParametersTest>()
						.Where(_ => _.StringValue == _source[i].StringValue)
						.ToList();

					AreEqual(_source.Skip(i).Take(1), selecedByString, comparer: NullParametersTest.Comparer);
				}
			}
		}
	}
}
