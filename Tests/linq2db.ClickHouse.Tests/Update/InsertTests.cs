using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Data;
using LinqToDB.DataProvider.ClickHouse;
using LinqToDB.Mapping;
using NUnit.Framework;
using Tests;
using Tests.Model;

namespace LinqToDB.ClickHouse.Tests.Update
{
	[TestFixture]
	[Order(10000)]
	class InsertTests: TestBase
	{
		[OneTimeTearDown]
		public void CleanUp()
		{
			var runner = new a_CreateData();

			foreach (var context in UserProviders)
				runner.CreateDatabase(context);
		}


#if AZURE
		[ActiveIssue("Error from Azure runs (db encoding issue?): FbException : Malformed string", Configuration = TestProvName.AllFirebird)]
#endif
		[Test]
		public void DistinctInsert1(
			[DataSources(
				ProviderName.DB2,
				TestProvName.AllInformix,
				TestProvName.AllPostgreSQL,
				TestProvName.AllSQLite,
				TestProvName.AllAccess)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				var delta = db
					.Types
					.Select(_ => Math.Floor(_.ID / 3.0))
					.Distinct()
					.Count();

				var cnt1 = db.Types.Count(c => c.ID > 1000);

				db
					.Types
					.Select(_ => Math.Floor(_.ID / 3.0))
					.Distinct()
					.Insert(db.Types, _ => new LinqDataTypes
					{
						ID        = (int)(_ + 1001),
						GuidValue = Sql.NewGuid(),
						BoolValue = true
					});
				
				var cnt2 = db.Types.Count(c => c.ID > 1000);

				Assert.AreEqual(delta, cnt2 - cnt1);
			}
		}

#if AZURE
		[ActiveIssue("Error from Azure runs (db encoding issue?): FbException : Malformed string", Configuration = TestProvName.AllFirebird)]
#endif
		[Test]
		public void DistinctInsert2(
			[DataSources(
				ProviderName.DB2,
				TestProvName.AllInformix,
				TestProvName.AllPostgreSQL,
				TestProvName.AllSQLite,
				TestProvName.AllAccess)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				var delta = db
					.Types
					.Select(_ => Math.Floor(_.ID / 3.0))
					.Distinct()
					.Count();

				var cnt1 = db.Types.Count(c => c.ID > 1000);

				db.Types
					.Select(_ => Math.Floor(_.ID / 3.0))
					.Distinct()
					.Into(db.Types)
						.Value(t => t.ID,        t => (int)(t + 1001))
						.Value(t => t.GuidValue, t => Sql.NewGuid())
						.Value(t => t.BoolValue, t => true)
					.Insert();

				var cnt2 = db.Types.Count(c => c.ID > 1000);

				Assert.AreEqual(delta, cnt2 - cnt1);
			}
		}

		[Test]
		public void Insert1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id   = 1001;
				var cnt1 = db.Child.Count(c => c.ChildID == id);

				db.Child
					.Insert(() => new Child
					{
						ParentID = 1,
						ChildID  = id
					});

				Assert.AreEqual(cnt1 + 1, db.Child.Count(c => c.ChildID == id));
			}
		}

		[Test]
		public void Insert2([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id   = 1001;
				var cnt1 = db.Child.Count(c => c.ChildID == id);

				db
					.Into(db.Child)
						.Value(c => c.ParentID, () => 1)
						.Value(c => c.ChildID,  () => id)
					.Insert();

				Assert.AreEqual(cnt1 + 1, db.Child.Count(c => c.ChildID == id));
			}
		}

		[Test]
		public async Task Insert2Async([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id   = 1001;
				var cnt1 = db.Child.Count(c => c.ChildID == id);

				await db
					.Into(db.Child)
						.Value(c => c.ParentID, () => 1)
						.Value(c => c.ChildID,  () => id)
					.InsertAsync();

				Assert.AreEqual(cnt1 + 1, db.Child.Count(c => c.ChildID == id));
			}
		}

		[Test]
		public void Insert3([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id    = 1001;
				var cnt1  = db.Child.Count(c => c.ChildID == id);
				var delta = db.Child.Count(c => c.ChildID == 11);

				db.Child
					.Where(c => c.ChildID == 11)
					.Insert(db.Child, c => new Child
					{
						ParentID = c.ParentID,
						ChildID  = id
					});

				Assert.AreEqual(cnt1 + 1, db.Child.Count(c => c.ChildID == id));
			}
		}

		[Test]
		public async Task Insert3Async([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id    = 1001;
				var cnt1  = db.Child.Count(c => c.ChildID == id);
				var delta = db.Child.Count(c => c.ChildID == 11);

				await db.Child
					.Where(c => c.ChildID == 11)
					.InsertAsync(db.Child, c => new Child
					{
						ParentID = c.ParentID,
						ChildID = id
					});

				Assert.AreEqual(cnt1 + 1, db.Child.Count(c => c.ChildID == id));
			}
		}
		
		[Table("LinqDataTypes")]
		public class LinqDataTypesArrayTest
		{
			[Column] public int       ID;
			[Column] public decimal   MoneyValue;
			[Column] public DateTime? DateTimeValue;
			[Column] public bool      BoolValue;
			[Column] public Guid      GuidValue;
			[Column] public byte[]?   BinaryValue;
			[Column] public short     SmallIntValue;
			[Column] public string?   StringValue;
		}

		[Test]
		public void InsertArray1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var types = db.GetTable<LinqDataTypesArrayTest>();

				var id = types.Max(t => t.ID) + 1;
				types.Insert(() => new LinqDataTypesArrayTest { ID = id, BoolValue = true, BinaryValue = null });

				Assert.IsNull(types.Single(t => t.ID == id).BinaryValue);
			}
		}

		[Test]
		public void InsertArray2([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var types = db.GetTable<LinqDataTypesArrayTest>();
				var id = types.Max(t => t.ID) + 1;

				byte[]? arr = null;

				types.Insert(() => new LinqDataTypesArrayTest { ID = id, BoolValue = true, BinaryValue = arr });

				var res = types.Single(t => t.ID == id).BinaryValue;

				Assert.IsNull(res);
			}
		}

		[Test]
		public void InsertArray3([DataSources(ProviderName.SQLiteMS)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var types = db.GetTable<LinqDataTypesArrayTest>();

				var id = types.Max(t => t.ID) + 1;

				var arr = new byte[] { 1, 2, 3, 0xF1 };
				var stringValue = "hello world \u0000 \uff7f";

				types.Insert(() => new LinqDataTypesArrayTest { ID = id, BoolValue = true, BinaryValue = arr, StringValue = stringValue });

				var res = types.Single(t => t.ID == id);

				Assert.That(res.StringValue, Is.EqualTo(stringValue));
				Assert.That(res.BinaryValue, Is.EqualTo(arr));
			}
		}

		[Test]
		public void InsertArray4([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var types = db.GetTable<LinqDataTypesArrayTest>();

				var id = types.Max(t => t.ID) + 1;

				var arr = new byte[] { 1, 2, 3, 4, 0xF1 };

				db.Insert(new LinqDataTypesArrayTest { ID = id, BoolValue = true, BinaryValue = arr });

				var res = types.Single(t => t.ID == id).BinaryValue;

				Assert.That(res, Is.EqualTo(arr));
			}
		}

		[Test]
		public void InsertUnion1([DataSources(ProviderName.SQLiteMS)] string context)
		{
			Child.Count();

			using (var db = GetDataContext(context))
			{
				var cnt1 = db.Parent.Count(p => p.ParentID > 1000);

				var q =
					db.Child.     Select(c => new Parent { ParentID = c.ParentID,      Value1 = (int) Math.Floor(c.ChildID / 10.0) }).Union(
					db.GrandChild.Select(c => new Parent { ParentID = c.ParentID ?? 0, Value1 = (int?)Math.Floor((c.GrandChildID ?? 0) / 100.0) }));

				var cnt2 = q.Count();

				q.Insert(db.Parent, p => new Parent
				{
					ParentID = p.ParentID + 1000,
					Value1 = p.Value1
				});

				Assert.AreEqual(db.Parent.Count(c => c.ParentID > 1000),
					cnt1 + cnt2);
			}
		}

		[Test]
		public void InsertEnum1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id = db.Parent4.Max(_ => _.ParentID) + 1001;

				var p = new Parent4
				{
					ParentID = id,
					Value1   = TypeValue.Value2
				};

				db.Parent4
					.Insert(() => new Parent4
					{
						ParentID = id,
						Value1 = p.Value1
					});

				Assert.AreEqual(1, db.Parent4.Count(_ => _.ParentID == id && _.Value1 == p.Value1));
			}
		}

		[Test]
		public void InsertNull1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id = db.Parent.Max(p => p.ParentID) + 1001;

				db
					.Into(db.Parent)
						.Value(p => p.ParentID, id)
						.Value(p => p.Value1,   (int?)null)
					.Insert();

				Assert.AreEqual(1, db.Parent.Count(p => p.ParentID == id));
			}
		}

		[Test]
		public void InsertNull2([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id = db.Parent.Max(p => p.ParentID) + 1001;

				db
					.Into(db.Parent)
						.Value(p => p.ParentID, id)
						.Value(p => p.Value1,   () => (int?)null)
					.Insert();

				Assert.AreEqual(1, db.Parent.Count(p => p.ParentID == id));
			}
		}

		[Test]
		public void InsertBatch1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id = db.Types2.Max(_ => _.ID);

				((DataConnection)db).BulkCopy(1, new[]
					{
						new LinqDataTypes2 { ID = id + 1003, MoneyValue = 0m, DateTimeValue = null,         BoolValue = true,  GuidValue = new Guid("ef129165-6ffe-4df9-bb6b-bb16e413c883"), SmallIntValue =  null, IntValue = null    },
						new LinqDataTypes2 { ID = id + 1004, MoneyValue = 0m, DateTimeValue = null,         BoolValue = true,  GuidValue = new Guid("ef129165-6ffe-4df9-bb6b-bb16e413c883"), SmallIntValue =  null, IntValue = null    }
					});

				Assert.AreEqual(2, db.Types.Count(_ => _.ID == id + 1003 || _.ID == id + 1004));
			}
		}

		[Test]
		public void InsertBatch2([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var id = db.Types2.Max(_ => _.ID);

				((DataConnection)db).BulkCopy(100, new[]
				{
					new LinqDataTypes2 { ID = id + 1003, MoneyValue = 0m, DateTimeValue = null,         BoolValue = true,  GuidValue = new Guid("ef129165-6ffe-4df9-bb6b-bb16e413c883"), SmallIntValue =  null, IntValue = null    },
					new LinqDataTypes2 { ID = id + 1004, MoneyValue = 0m, DateTimeValue = DateTime.Now, BoolValue = false, GuidValue = null,                                             SmallIntValue =  2,    IntValue = 1532334 }
				});

				Assert.AreEqual(2, db.Types.Count(_ => _.ID == id + 1003 || _.ID == id + 1004));
			}
		}

		[Test]
		public void Insert11([DataSources] string context)
		{
			var p = new ComplexPerson { Name = new FullName { FirstName = "fn", LastName = "ln" }, Gender = Gender.Male };

			using (var db = GetDataContext(context))
			{
				var id = db.Person.Max(t => t.ID) + 1;
				p.ID = id;

				db.Insert(p);

				var inserted = db.GetTable<ComplexPerson>().Single(p2 => p2.ID == id);

				Assert.AreEqual(p.Name.FirstName, inserted.Name.FirstName);
				Assert.AreEqual(p.Name.LastName,  inserted.Name.LastName);
				Assert.AreEqual(p.Gender,         inserted.Gender);
			}
		}
		
		[Test]
		public void Insert16([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				const string name = "Insert16я";
				      var    idx  = 4;

				var cnt1 = db.Person.Where(_ => _.FirstName.StartsWith(name)).Count();


				db.Person.Insert(() => new Person()
				{
					FirstName = name,
					LastName  = (Sql.AsSql(name).Length + idx).ToString(),
					Gender    = Gender.Male,
				});

				var lastName = (name.Length + idx).ToString();
				var cnt2     = db.Person.Where(_ => _.FirstName.StartsWith(name) && _.LastName == lastName).Count();

				Assert.AreEqual(cnt1 + 1, cnt2);
			}
		}

		[Table("LinqDataTypes")]
		class TestConvertTable1
		{
			[PrimaryKey]                        public int      ID;
			[Column(DataType = DataType.Int64)] public TimeSpan BigIntValue;
		}

		[Test]
		public void InsertConverted([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{

				var tbl = db.GetTable<TestConvertTable1>();

				var id = tbl.Max(r => r.ID) + 1;

				var tt = TimeSpan.FromMinutes(1);

				((DataConnection)db).InlineParameters = true;
				db.Insert(/*() =>*/ new TestConvertTable1 { ID = id, BigIntValue = tt });

				Assert.AreEqual(tt, tbl.First(t => t.ID == id).BigIntValue);
			}
		}
	}
}
