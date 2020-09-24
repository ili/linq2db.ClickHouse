using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Tests;
using System.Linq;
using Tests.Model;

namespace LinqToDB.ClickHouse.Tests.Linq
{
	[TestFixture]
	public class UnionTests: TestBase
	{
		[Test]
		public void AllDistinctTest1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				AreEqual(
					   Parent.Union(
					   Parent.Concat(
					   Parent.Union(
					   Parent))),
					db.Parent.Union(
					db.Parent.Concat(
					db.Parent.Union(
					db.Parent))),
					sort: x => x.OrderBy(_ => _.ParentID).ThenBy(_ => _.Value1)
					);

			}

		}

		[Test]
		public void AllDistinctTest2([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				AreEqual(
					   Parent.Union(
					   Parent).Concat(
					   Parent).Union(
					   Parent),
					db.Parent.Union(
					db.Parent).Concat(
					db.Parent).Union(
					db.Parent),
					sort: x => x.OrderBy(_ => _.ParentID).ThenBy(_ => _.Value1)
					);

			}

		}

		[Test]
		public void AllDistinctTest3([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				AreEqual(
					   Parent.Union(
					   Parent.Concat(
					   Parent).Union(
					   Parent)),
					db.Parent.Union(
					db.Parent.Concat(
					db.Parent).Union(
					db.Parent)),
					sort: x => x.OrderBy(_ => _.ParentID).ThenBy(_ => _.Value1)
					);

			}

		}

		[Test]
		public void AllDistinctTest4([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				AreEqual(
					   Parent.Union(
					   Parent.Union(
					   Parent.Union(
					   Parent))),
					db.Parent.Union(
					db.Parent.Union(
					db.Parent.Union(
					db.Parent))),
					sort: x => x.OrderBy(_ => _.ParentID).ThenBy(_ => _.Value1)
					);

			}

		}

		[Test]
		public void AllDistinctTest5([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				AreEqual(
					   Parent.Union(
					   Parent).Union(
					   Parent).Union(
					   Parent),
					db.Parent.Union(
					db.Parent).Union(
					db.Parent).Union(
					db.Parent),
					sort: x => x.OrderBy(_ => _.ParentID).ThenBy(_ => _.Value1)
					);

			}

		}

		[Test]
		public void AllDistinctTest6([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				AreEqual(
					   Child. Select(c => new Parent { Value1 = c.ParentID, ParentID = c.ParentID }).Union(
					   Parent.Select(c => new Parent { Value1 = c.ParentID, ParentID = c.ParentID })).Concat(
					   Child. Select(c => new Parent { Value1 = c.ParentID, ParentID = c.ParentID }).Union(
					   Parent.Select(c => new Parent { Value1 = c.ParentID, ParentID = c.ParentID }))),
					db.Child. Select(c => new Parent { Value1 = c.ParentID, ParentID = c.ParentID }).Union(
					db.Parent.Select(c => new Parent { Value1 = c.ParentID, ParentID = c.ParentID })).Concat(
					db.Child. Select(c => new Parent { Value1 = c.ParentID, ParentID = c.ParentID }).Union(
					db.Parent.Select(c => new Parent { Value1 = c.ParentID, ParentID = c.ParentID }))),
					sort: x => x.OrderBy(_ => _.ParentID).ThenBy(_ => _.Value1)
					);

			}

		}
	}
}
