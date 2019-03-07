﻿using System;
using Tests.Models;
using Tortuga.Chain;

#if MSTest

using Microsoft.VisualStudio.TestTools.UnitTesting;

#elif WINDOWS_UWP
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Tests.Class1Databases
{
    [TestClass]
    public class UpsertTests : TestBase
    {
#if !Roslyn_Missing && !SQLite

        [TestMethod]
        public void UpsertTests_ChangeTrackingTest_Compiled()
        {
            var original = new ChangeTrackingEmployee()
            {
                FirstName = "Test",
                LastName = "Employee" + DateTime.Now.Ticks,
                Title = "Mail Room",
                EmployeeId = Guid.NewGuid().ToString()
            };

            var inserted = DataSource.Upsert(EmployeeTableName, original, UpsertOptions.ChangedPropertiesOnly).Compile().ToObject<ChangeTrackingEmployee>().Execute();
            inserted.FirstName = "Changed";
            inserted.AcceptChanges(); //only properties changed AFTER this line should actually be updated in the database
            inserted.Title = "Also Changed";

            var updated = DataSource.Upsert(EmployeeTableName, inserted, UpsertOptions.ChangedPropertiesOnly).Compile().ToObject<ChangeTrackingEmployee>().Execute();
            Assert.AreEqual(original.FirstName, updated.FirstName, "FirstName shouldn't have changed");
            Assert.AreEqual(original.LastName, updated.LastName, "LastName shouldn't have changed");
            Assert.AreEqual(inserted.Title, updated.Title, "Title should have changed");
        }

        [TestMethod]
        public void UpsertTests_ChangeTrackingTest_NothingChanged_Compiled()
        {
            var original = new ChangeTrackingEmployee()
            {
                FirstName = "Test",
                LastName = "Employee" + DateTime.Now.Ticks,
                Title = "Mail Room",
                EmployeeId = Guid.NewGuid().ToString()
            };

            var inserted = DataSource.Insert(EmployeeTableName, original).Compile().ToObject<ChangeTrackingEmployee>().Execute();

            try
            {
                var updated = DataSource.Upsert(EmployeeTableName, inserted, UpsertOptions.ChangedPropertiesOnly).Compile().ToObject<ChangeTrackingEmployee>().Execute();
                Assert.Fail("Exception Expected");
            }
            catch (ArgumentException)
            {
                //pass
            }
        }

#endif

        [TestMethod]
        public void UpsertTests_ChangeTrackingTest_NothingChanged()
        {
            var original = new ChangeTrackingEmployee()
            {
                FirstName = "Test",
                LastName = "Employee" + DateTime.Now.Ticks,
                Title = "Mail Room",
                EmployeeId = Guid.NewGuid().ToString()
            };

            var inserted = DataSource.Insert(EmployeeTableName, original).ToObject<ChangeTrackingEmployee>().Execute();

            try
            {
                var updated = DataSource.Upsert(EmployeeTableName, inserted, UpsertOptions.ChangedPropertiesOnly).ToObject<ChangeTrackingEmployee>().Execute();
                Assert.Fail("Exception Expected");
            }
            catch (ArgumentException)
            {
                //pass
            }
        }

        [TestMethod]
        public void UpsertTests_ChangeTrackingTest()
        {
            var original = new ChangeTrackingEmployee()
            {
                FirstName = "Test",
                LastName = "Employee" + DateTime.Now.Ticks,
                Title = "Mail Room",
                EmployeeId = Guid.NewGuid().ToString()
            };

            var inserted = DataSource.Upsert(EmployeeTableName, original, UpsertOptions.ChangedPropertiesOnly).ToObject<ChangeTrackingEmployee>().Execute();
            inserted.FirstName = "Changed";
            inserted.AcceptChanges(); //only properties changed AFTER this line should actually be updated in the database
            inserted.Title = "Also Changed";

            var updated = DataSource.Upsert(EmployeeTableName, inserted, UpsertOptions.ChangedPropertiesOnly).ToObject<ChangeTrackingEmployee>().Execute();
            Assert.AreEqual(original.FirstName, updated.FirstName, "FirstName shouldn't have changed");
            Assert.AreEqual(original.LastName, updated.LastName, "LastName shouldn't have changed");
            Assert.AreEqual(inserted.Title, updated.Title, "Title should have changed");
        }
    }
}
