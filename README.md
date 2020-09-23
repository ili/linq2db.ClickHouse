# linq2db.ClickHouse
Experiments on ClickHouse support

## Limitations

  * JOIN predicates - ClickHouse does not support predicates, not comparing colums, so you can't do smth like `a.Column = b.Colums AND a.Value == 2`. Please use subqueries, automatic optimization supposed to bu buggy and risky.
  * Subqueries as values are not supported: `select (select count(*) from second) from first`
  * `EXISTS` condition is not supported
  * `CREATE TABLE` - basic only support, prefer using direct SQL for production tables

## Query settings

Query settings can be configured with `ClickHouseDataProvider.QuerySettings` property, defaults are:

  * `join_use_nulls = 1` 
