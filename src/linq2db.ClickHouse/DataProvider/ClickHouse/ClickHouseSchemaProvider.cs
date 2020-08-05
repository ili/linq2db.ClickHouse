using System;

using LinqToDB.Data;
using LinqToDB.SchemaProvider;

namespace LinqToDB.DataProvider.ClickHouse
{
	class ClickHouseSchemaProvider : ISchemaProvider
	{
		public DatabaseSchema GetSchema(DataConnection dataConnection, GetSchemaOptions? options = null)
		{
			throw new NotImplementedException();
		}
	}
}
