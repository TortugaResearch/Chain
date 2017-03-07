﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.Metadata
{
    [TestClass]
    public class MetadataTests : TestBase
    {
        [TestMethod]
        public void PreloadTables()
        {
            DataSource.DatabaseMetadata.PreloadTables();
            foreach (var table in DataSource.DatabaseMetadata.GetTablesAndViews().Where(x => x.IsTable))
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(table.Name.ToString()), "Name");
                Assert.IsTrue(table.Columns.Count > 0, "Columns.Count");
                foreach (var column in table.Columns)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(column.SqlName), "Name");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(column.ClrName), "Name");
                }
            }
        }

        [TestMethod]
        public void PreloadUserDefinedTypes()
        {
            DataSource.DatabaseMetadata.PreloadUserDefinedTypes();
            foreach (var type in DataSource.DatabaseMetadata.GetUserDefinedTypes())
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(type.Name.ToString()), "Name");
                if (type.IsTableType)
                {
                    Assert.IsTrue(type.Columns.Count > 0, "Columns.Count");
                    foreach (var column in type.Columns)
                    {
                        Assert.IsFalse(string.IsNullOrWhiteSpace(column.SqlName), "Name");
                        Assert.IsFalse(string.IsNullOrWhiteSpace(column.ClrName), "Name");
                    }
                }
                else
                {
                    Assert.IsTrue(type.Columns.Count == 1, $"Columns.Count was {type.Columns.Count} but it should be 1 for non-table type {type.Name} in sql server");
                }
            }
        }

        [TestMethod]
        public void PreloadViews()
        {
            DataSource.DatabaseMetadata.PreloadViews();
            foreach (var view in DataSource.DatabaseMetadata.GetTablesAndViews().Where(x => !x.IsTable))
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(view.Name.ToString()), "Name");
                Assert.IsTrue(view.Columns.Count > 0, "Columns.Count");
                foreach (var column in view.Columns)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(column.SqlName), "Name");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(column.ClrName), "Name");
                }
            }
        }

        [TestMethod]
        public void PreloadStoredProcedures()
        {
            DataSource.DatabaseMetadata.PreloadStoredProcedures();
            foreach (var item in DataSource.DatabaseMetadata.GetStoredProcedures())
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.Name.ToString()), "Name");
            }
        }

        [TestMethod]
        public void PreloadTableValueFunctions()
        {
            DataSource.DatabaseMetadata.PreloadTableFunctions();
            foreach (var item in DataSource.DatabaseMetadata.GetStoredProcedures())
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.Name.ToString()), "Name");
            }
        }

        [TestMethod]
        public void PreloadScalarFunctions()
        {
            DataSource.DatabaseMetadata.PreloadScalarFunctions();
            foreach (var item in DataSource.DatabaseMetadata.GetScalarFunctions())
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.Name.ToString()), "Name");
            }
        }


        [TestMethod]
        public void PreloadAll()
        {
            DataSource.DatabaseMetadata.Preload();
        }

        [TestMethod]
        public void SqlBuilder()
        {
            DataSource.DatabaseMetadata.Preload();
            foreach (var item in DataSource.DatabaseMetadata.GetTablesAndViews().Where(x => x.IsTable))
            {
                var builder = item.CreateSqlBuilder(false);
                builder.ApplyDesiredColumns(Tortuga.Chain.Materializers.Materializer.AllColumns);


                var selectColumns = builder.GetSelectColumns().ToList();
                foreach (var column in item.Columns)
                {
                    Assert.IsTrue(selectColumns.Any(x => x == column.QuotedSqlName), $"Couldn't find column {column.QuotedSqlName} in the selected columns list of table {item.Name}.");
                }
                foreach (var column in selectColumns)
                {
                    Assert.IsTrue(item.Columns.Any(x => x.QuotedSqlName == column), $"Couldn't find column {column} in the table's columns list of table {item.Name}.");

                }

                var keyColumns = builder.GetKeyColumns().ToList();
                foreach (var column in item.Columns)
                {
                    if (column.IsPrimaryKey)
                        Assert.IsTrue(keyColumns.Any(x => x.QuotedSqlName == column.QuotedSqlName), $"Couldn't find column {column.QuotedSqlName} in the key columns list of table {item.Name}.");
                }

            }
        }


    }
}
