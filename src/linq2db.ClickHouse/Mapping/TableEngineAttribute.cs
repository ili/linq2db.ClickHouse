using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.Mapping
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class TableEngineAttribute: Attribute
	{
		public static readonly TableEngineAttribute StripeLog = new TableEngineAttribute("StripeLog");
		public static readonly TableEngineAttribute MergeTree = new TableEngineAttribute("MergeTree");
		public TableEngineAttribute(string engine)
		{
			Engine = engine;
		}

		public string Engine { get; }
		public string? PrimaryKeys { get; set; }
		public string? OrderBy { get; set; }
		public string? PartitionBy { get; set; }
		public string? SampleBy { get; set; }
		public string? Settings { get; set; }
		public string? Ttl { get; set; }
	}
}
