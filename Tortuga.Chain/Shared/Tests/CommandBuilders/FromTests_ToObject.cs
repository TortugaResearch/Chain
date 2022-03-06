﻿using Tests.Models;
using Tortuga.Chain;


namespace Tests.CommandBuilders;

[TestClass]
public class FromTests_ToObject : TestBase
{

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToInferredObject(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);

		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(EmployeeTableName, emp1).ToObject<Employee>().Execute();

			var lookup = dataSource.From(EmployeeTableName, new { Title = uniqueKey }).ToObject<EmployeeLookup>(RowOptions.InferConstructor).Execute();

			Assert.AreEqual("A", lookup.FirstName, "First Name");
			Assert.AreEqual("1", lookup.LastName, "Last Name");
		}
		finally
		{
			Release(dataSource);
		}
	}

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToObject(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);

		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(EmployeeTableName, emp1).ToObject<Employee>().Execute();

			var lookup = dataSource.From(EmployeeTableName, new { Title = uniqueKey }).ToObject<Employee>().Execute();

			Assert.AreEqual("A", lookup.FirstName, "First Name");
			Assert.AreEqual("1", lookup.LastName, "Last Name");
		}
		finally
		{
			Release(dataSource);
		}
	}

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToObject_AutoTableSelection(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);

		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(EmployeeTableName, emp1).ToObject<Employee>().Execute();

			var lookup = dataSource.From<Employee>(new { Title = uniqueKey }).ToObject().Execute();

			Assert.AreEqual("A", lookup.FirstName, "First Name");
			Assert.AreEqual("1", lookup.LastName, "Last Name");
		}
		finally
		{
			Release(dataSource);
		}
	}

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToObject_IncludedProperties_AutoTableSelection(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);

		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(emp1).Execute();

			var lookup = dataSource.From<Employee>(new { Title = uniqueKey }).ToObject().WithProperties("EmployeeKey", "FirstName").Execute();

			Assert.IsNotNull(lookup.EmployeeKey, "EmployeeKey");
			Assert.AreEqual("A", lookup.FirstName, "First Name");
			Assert.IsNull(lookup.LastName, "Last Name");
		}
		finally
		{
			Release(dataSource);
		}
	}

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToObject_IncludedProperties(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);

		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(emp1).Execute();

			var lookup = dataSource.From(EmployeeTableName, new { Title = uniqueKey }).ToObject<Employee>().WithProperties("EmployeeKey", "FirstName").Execute();

			Assert.IsNotNull(lookup.EmployeeKey, "EmployeeKey");
			Assert.AreEqual("A", lookup.FirstName, "First Name");
			Assert.IsNull(lookup.LastName, "Last Name");
		}
		finally
		{
			Release(dataSource);
		}
	}

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToObject_ExcludedProperties_AutoTableSelection(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);

		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(emp1).Execute();

			var lookup = dataSource.From<Employee>(new { Title = uniqueKey }).ToObject().ExceptProperties("LastName").Execute();

			Assert.IsNotNull(lookup.EmployeeKey, "EmployeeKey");
			Assert.AreEqual("A", lookup.FirstName, "First Name");
			Assert.IsNull(lookup.LastName, "Last Name");
		}
		finally
		{
			Release(dataSource);
		}
	}

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToObject_ExcludedProperties(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);

		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(emp1).Execute();

			var lookup = dataSource.From(EmployeeTableName, new { Title = uniqueKey }).ToObject<Employee>().ExceptProperties("LastName").Execute();

			Assert.IsNotNull(lookup.EmployeeKey, "EmployeeKey");
			Assert.AreEqual("A", lookup.FirstName, "First Name");
			Assert.IsNull(lookup.LastName, "Last Name");
		}
		finally
		{
			Release(dataSource);
		}
	}

#if SQLITE

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToImmutableObject(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);
		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(EmployeeTableName, emp1).ToObject<Employee>().Execute();

			var lookup = dataSource.From(EmployeeTableName, new { Title = uniqueKey }).ToObject<EmployeeLookup>().WithConstructor<long, string, string>().Execute();

			Assert.AreEqual("A", lookup.FirstName);
			Assert.AreEqual("1", lookup.LastName);
		}
		finally
		{
			Release(dataSource);
		}
	}

#elif MYSQL

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToImmutableObject(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);
		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(EmployeeTableName, emp1).ToObject<Employee>().Execute();

			var lookup = dataSource.From(EmployeeTableName, new { Title = uniqueKey }).ToObject<EmployeeLookup>().WithConstructor<ulong, string, string>().Execute();

			Assert.AreEqual("A", lookup.FirstName);
			Assert.AreEqual("1", lookup.LastName);
		}
		finally
		{
			Release(dataSource);
		}
	}

#else

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToImmutableObject(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);
		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(EmployeeTableName, emp1).ToObject<Employee>().Execute();

			var lookup = dataSource.From(EmployeeTableName, new { Title = uniqueKey }).ToObject<EmployeeLookup>().WithConstructor<int, string, string>().Execute();

			Assert.AreEqual("A", lookup.FirstName);
			Assert.AreEqual("1", lookup.LastName);
		}
		finally
		{
			Release(dataSource);
		}
	}

	[DataTestMethod, BasicData(DataSourceGroup.Primary)]
	public void ToImmutableObject_AutoTableSelection(string dataSourceName, DataSourceType mode)
	{
		var dataSource = DataSource(dataSourceName, mode);
		try
		{
			var uniqueKey = Guid.NewGuid().ToString();

			var emp1 = new Employee() { FirstName = "A", LastName = "1", Title = uniqueKey };
			dataSource.Insert(emp1).ToObject().Execute();

			var lookup = dataSource.From<EmployeeLookup>(new { Title = uniqueKey }).ToObject().WithConstructor<int, string, string>().Execute();

			Assert.AreEqual("A", lookup.FirstName);
			Assert.AreEqual("1", lookup.LastName);
		}
		finally
		{
			Release(dataSource);
		}
	}

#endif
}