## Version 4.4


### Features


### Bugs


### Technical Debt


[#497 Update interfaces to expose more functionality](https://github.com/TortugaResearch/Tortuga.Chain/issues/497)

`ICrudDataSource` and `IScalarDbCommandBuilder` were updated to expose more functionality from their matching classes.

## Version 4.3

Updated dependency to Anchor 4.1.

### Features

[#88 Simple Aggregators](https://github.com/TortugaResearch/Tortuga.Chain/issues/88)

The "simple aggregators" agument the `AsCount` method. Each returns a single value for the desired column.

* `AsAverage(columnName)`
* `AsMax(columnName)`
* `AsMin(columnName)`
* `AsSum(columnName, distinct)`

These all return a `ScalarDbCommandBuilder` with which the caller can specify the return type. They are built on the `AggregateColumn` model, which overrides the usual column selection process.

For more complex aggregation, use the `AsAggregate` method. This accepts a collection of `AggregateColumn` objects, which can be used for both aggregegate functions and grouping.

The original `AsCount` methods were reworked to fit into this model.

[#89 Declarative Aggregators](https://github.com/TortugaResearch/Tortuga.Chain/issues/89)

Attributes can now be used to declare aggregations directly in a model.

```csharp
[Table("Sales.EmployeeSalesView"]
public class SalesFigures
{
	[AggregateColumn(AggregateType.Min, "TotalPrice")]
	public decimal SmallestSale { get; set; }

	[AggregateColumn(AggregateType.Max, "TotalPrice")]
	public decimal LargestSale { get; set; }

	[AggregateColumn(AggregateType.Average, "TotalPrice")]
	public decimal AverageSale { get; set; }

	[CustomAggregateColumn("Max(TotalPrice) - Min(TotalPrice)")]
	public decimal Range { get; set; }

	[GroupByColumn]
	public int EmployeeKey { get; set; }

	[GroupByColumn]
	public string EmployeeName { get; set; }
}
```

To use this feature, you need use either of these patterns:

```csharp
datasource.FromTable(tableName, filter).ToAggregate<TObject>().ToCollection().Execute();
datasource.FromTable<TObject>(filter).ToAggregate().ToCollection().Execute();
```

In the second version, the table or view name is extracted from the class.


[#92 ToObjectStream](https://github.com/TortugaResearch/Tortuga.Chain/issues/92)

Previously, Chain would fully manage database connections by default. Specifically, it would open and close connections automatically unless a transaction was involved. In that case, the developer only needed to manage the transactional data source itself.

However, there are times when a result set is too large to handle at one time. In this case the developer will want an `IEnumerable` or `IAsyncEnumerable` instead of a collection. To support this, the `ToObjectStream` materializer was created.

When used in place of `ToCollection`, the caller gets a `ObjectStream` object. This object implements `IEnumerable<TObject>`, `IDisposable`, `IAsyncDisposable`, abd `IAsyncEnumerable<TObject>`. (That latter two are only available in .NET 6 or later.)

This object stream may be used directly, as shown below, or attached to an RX Observable or TPL Dataflow just like any other enumerable data structure.

```csharp
//sync pattern

using var objectStream = dataSource.From<Employee>(new { Title = uniqueKey }).ToObjectStream<Employee>().Execute();
foreach (var item in objectStream)
{
	Assert.AreEqual(uniqueKey, item.Title);
}

//async pattern

await using var objectStream = await dataSource.From<Employee>(new { Title = uniqueKey }).ToObjectStream<Employee>().ExecuteAsync();
await foreach (var item in objectStream)
{
	Assert.AreEqual(uniqueKey, item.Title);
}
```
It is vital that the object stream is disposed after use. If that doesn't occur, the database can suffer from thread exhaustion or deadlocks.


[#98 Dynamic Materializers and Desired Columns](https://github.com/TortugaResearch/Tortuga.Chain/issues/98)

Allow the use of `WithProperties` or `ExcludeProperties` to be used with...

* `.ToDynamicObject`
* `.ToDynamicObjectOrNull`
* `.ToDynamicCollection`


### Bugs

[#490 Command Timeout is not being honored in PostgreSQL and MySQL](https://github.com/TortugaResearch/Tortuga.Chain/issues/490)

See the ticket for an explaination for why this was broken.

### Technical Debt

[#488 Add IAsyncDisposable support](https://github.com/TortugaResearch/Tortuga.Chain/issues/488)

Added support for `IAsyncDisposable` to transactional data sources.








## Version 4.2

### Features

[#463 ISupportsDeleteByKeyList should have the same overloads as ISupportsGetByKeyList](https://github.com/TortugaResearch/Chain/issues/463)

[#464 ISupportsDeleteByKey should have the same overloads as ISupportsGetByKey](https://github.com/TortugaResearch/Chain/issues/464)

Allow an object to be used for determining which table to delete from instead of explicitly providing a table name.

[#471 Add Scalar and List options for Char](https://github.com/TortugaResearch/Tortuga.Chain/issues/471)

Adds

* `ToChar(...)`
* `ToCharOrNull(...)`
* `ToCharList(...)`
* `ToCharOrNullList(...)`

[#475 Add ToCharSet and ToByteSet materializers](https://github.com/TortugaResearch/Tortuga.Chain/issues/475)

Adds

* `ToCharSet(...)`
* `ToByteSet(...)`

[#24 Improve column name support for list based materializers](https://github.com/TortugaResearch/Tortuga.Chain/issues/24)

When using ToXxxList/ToXxxSet, you can specify a column name. If multiple columns are returned, which can happen with a stored procedure, it will only read the named column.

## Bug Fixes

[#469 Tortuga.Chain.MappingException: 'Cannot map value of type System.String to property Gender of type Char.' ](https://github.com/TortugaResearch/Tortuga.Chain/issues/469)

Adds mapping between `string` columns and `char` properties. Previously the property had to be a string.

### Technical Debt

[#400 Better Upsert Pattern for SQL Server](https://github.com/TortugaResearch/Tortuga.Chain/issues/400)

Hint `UPDLOCK` and `SERIALIZABLE` when using `MERGE` to perform an upsert. This reduces, though not elimintates, the need to perform an upsert in a transaction.

[#474 Remove duplicate code in list/set based materialzers](https://github.com/TortugaResearch/Tortuga.Chain/issues/474)

Removed roughly 60 lines of code in each column based materializer.


## Version 4.1


### Features with Breaking Changes

[#440 Default Sorting for WithLimits](https://github.com/TortugaResearch/Chain/issues/440)

If you use WithLimits without a sort order, then it will act non-deterministically. This is because the database could sort the results in any random order if not constrained.

The fix is to default to sorting with primary key when using WithLimits. If there are no primary keys and no explicit sorting, then an exception will be thrown if in strict mode.

This change also affects table-valued functions. Except these cannot infer the sort order as they do not have a primary key.

For reflection-based scenarios, the method `TableOrViewMetadata<TObjectName, TDbType>.GetDefaultSortOrder(int)` can be used to get a table's default sort order.

### Features

[#459 Add TimeSpan support for Access and OleDB](https://github.com/TortugaResearch/Tortuga.Chain/issues/459)


Access doesn't understand TimeSpan at all and treats it as a DateTime.

OleDB for SQL Server is worse. It returns time(7) columns as strings with the column type set to object.


[#445 Add support for DateOnly and TimeOnly](https://github.com/TortugaResearch/Tortuga.Chain/issues/445)

On the parameter builder side, `DateOnly` and `TimeOnly` need to be converted into `DateTime` or `TimeSpan`. Which specific conversion is used depends on the database/driver combination.

On the materializer side, a new class called `MaterializerTypeConverter` will be used. Moving forward, this will handle type conversions from the result set to the object.

The `MaterializerTypeConverter` is owned by a `DatabaseMetadata` object, allowing additional conversions to be registered at runtime.

[#451 Add support for CommitAsync, Save(savepointName), SaveAsync(savepointName), Rollback(savepointName), and RollbackAsync](https://github.com/TortugaResearch/Chain/issues/451)

We are only exposing these for .NET 6 and later.

[#443 GetByKey, UpdateByKey, and DeleteByKey should support System.Int16](https://github.com/TortugaResearch/Chain/issues/443)

This was done to improve support for lookup tables with small keys.

### Bug Fixes

The OleDB version of SQL Server was truncating fractional seconds when the parameter type is `time(n)` and `n>0`. To fix this, we have to force it to use `DateTime/DBTimeStamp` instead of `TimeSpan/DBTime`.

[#465 OleDbSqlServerTableFunction doesn't support sorting with table limits](https://github.com/TortugaResearch/Tortuga.Chain/issues/465)


Using either works, but there is an error if both are used.


### Performance Enhancements

[#439 Use `SqlCommand.EnableOptimizedParameterBinding` in SQL Server MDS.](https://github.com/TortugaResearch/Chain/issues/439)


### Technical Debt

Added test case for command timeout.

Created `ExecutionToken.PopulateCommand` method. This eliminates a lot of copy & past text from the various `Execute`/`ExecuteAsync` methods.

## Version 4.0

### Architecture

Rather than using #if in shared files, we are now using a trait based system that simulates multiple inheritance. [Tortuga.Shipwright](https://github.com/TortugaResearch/Shipwright) provides this capability using a C# Source Generator. Here is an example,

```CSharp
[UseTrait(typeof(SupportsDeleteAllTrait<AbstractObjectName, AbstractDbType>))]
[UseTrait(typeof(SupportsDeleteByKeyListTrait<AbstractCommand, AbstractParameter, AbstractObjectName, AbstractDbType>))]
[UseTrait(typeof(SupportsUpdateTrait<OleDbCommand, OleDbParameter, AccessObjectName, OleDbType>))]
[UseTrait(typeof(SupportsDeleteTrait<OleDbCommand, OleDbParameter, AccessObjectName, OleDbType>))]
[UseTrait(typeof(SupportsSqlQueriesTrait<OleDbCommand, OleDbParameter>))]
[UseTrait(typeof(SupportsUpdateByKeyListTrait<AbstractCommand, AbstractParameter, AbstractObjectName, AbstractDbType>))]
[UseTrait(typeof(SupportsInsertTrait<AbstractCommand, AbstractParameter, AbstractObjectName, AbstractDbType>))]
[UseTrait(typeof(SupportsUpdateSet<AbstractCommand, AbstractParameter, AbstractObjectName, AbstractDbType>))]
[UseTrait(typeof(SupportsDeleteSet<AbstractCommand, AbstractParameter, AbstractObjectName, AbstractDbType>))]
[UseTrait(typeof(SupportsFromTrait<AbstractCommand, AbstractParameter, AbstractObjectName, AbstractDbType, AbstractLimitOption>))]
[UseTrait(typeof(SupportsGetByKeyListTrait<AbstractCommand, AbstractParameter, AbstractObjectName, AbstractDbType>))]

partial class AccessDataSourceBase : ICrudDataSource
```

As you can see, the common functionality is added via the `UseTrait` attribute. In cases where additional logic is needed, the trait can make calls to partial methods in the database-specific class.


### Features with Breaking Changes

[#429 Realign database classes](https://github.com/TortugaResearch/Chain/issues/429)

The `IClassXDataSource` interfaces are gone. Now we have interfaces based on specific feature sets such as `ISupportsInsert` or `ISupportsScalarFunction`. Rollups such as `ICrudDataSource` are used to combine related feature interfaces.

### Other Features 

[#327 Enabled SQL Dependency in Tortuga.Chain.SqlServer.MDS Materializers and Appenders SQL Server](https://github.com/TortugaResearch/Chain/issues/327)

Previously there was a bug preventing this feature from working with `Microsoft.Data.SqlClient`. Now that the bug is fixed, we can enable the feature in Chain.

[#411 Improve the default behavior when only one constructor exists.](https://github.com/TortugaResearch/Chain/issues/411)

If only one constructor exists, then default to using that constructor even if InferConstructor is not set.

[#408 TableAndView and GetByKey Command Builders](https://github.com/TortugaResearch/Chain/issues/408)

If you use the TableAndView attribute, then GetByKey doesn't work because it doesn't know what the primary key is.

When that happens, ask the table for its primary key. 

[#422 Add Truncate Command Builder Command Builders](https://github.com/TortugaResearch/Chain/issues/422)

This will only be exposed for databases that have a native truncate command. For other databases, use `DeleteAll` instead.

Note that in SQLite, `Truncate` and `DeleteAll` will have have the same effect. This is due to SQLite's internal optimization strategy.

[#430 Add DeleteAll command](https://github.com/TortugaResearch/Chain/issues/430)

As noted above, calling `DeleteAll` on a SQLite database will actually perform a truncate.


### Bug Fixes

[#406 Delete, TableAndView, and Strict Mode Command Builders](https://github.com/TortugaResearch/Chain/issues/406)

When using the TableAndView attribute, some column will be mapped to the view but not the table.

Those unmapped columns should be ignored when performing a Delete operation. We only care about the columns that feed into the primary key.

[#407 Records and Strict Mode Materializers and Appenders](https://github.com/TortugaResearch/Chain/issues/407)

The main problem was a bug in Tortuga.Anchor, which was treating protected properties as if they were public properties. Once fixed, we could check the `property.CanRead` field to avoid pulling in the hidden property called `EqualityContract` that all records expose.

[#418 PostgreSQL GetTableApproximateCount returns -1 for empty tables Command Builders PostgreSQL](https://github.com/TortugaResearch/Chain/issues/418)

A weird effect in PostgreSQL is that if the table has never had any rows, GetTableApproximateCount will return -1 instead of 0.


### Performance Enhancements

[#402 SQL Server Parameter Lengths Performance and Optimization SQL Builder SQL Server](https://github.com/TortugaResearch/Chain/issues/402)

When sending in variable length parameters (e.g. nVarChar), make sure the parameter length is based on the column's max length. This is to avoid creating a bunch of different plans, each with slightly different parameters.

[#415 Default to not using CommandBehavior.SequentialAccess Materializers and Appenders](https://github.com/TortugaResearch/Chain/issues/415)

Make using CommandBehavior.SequentialAccess an opt-in. According to Microsoft, "In most cases using the Default (non-sequential) access mode is the better choice, as [...] and you will get better performance using ReadAsync."

### Technical Debt

[Cleanup NuGet package code](https://github.com/TortugaResearch/Chain/pull/447/files)

Use the `<Version>` tag instead of the individual AssemblyVersion and FileVersion tags. This elimiantes the need for the `ReadPackageVersionFromOutputAssembly` target.

[#416 Tag unmapped data types Metadata PostgreSQL](https://github.com/TortugaResearch/Chain/issues/416)


Added new items to the list of PostgreSQL data types that are used internally and not exposed via the database driver.

This is going to be an ongoing issue.

[#427 Use global usings](https://github.com/TortugaResearch/Chain/issues/427)

The purpose of this change was to reduce the copious amounts of `#if` statements in the shared files.

### Other Breaking Changes

[#423 Remove String Names](https://github.com/TortugaResearch/Chain/issues/423)

Object names (e.g. tables, views, procs) will need to be referenced via:

* Using the database specific `ObjectName` class
* Using a class that is mapped to a table
* Via the `IDataSource` interfaces (which will continue to accept strings)

The reason for this change is to simplify the data source API. Currently there are too many overloads that perform the same action.

It was originally added to make working with F# easier. But with the improvements to table name inference using generics, it is no longer a pressing need.

