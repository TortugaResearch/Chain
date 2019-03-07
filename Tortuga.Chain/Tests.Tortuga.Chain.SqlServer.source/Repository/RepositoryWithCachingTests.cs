﻿using System.Collections.Generic;
using System.Linq;
using Tests.Models;
using System;

#if MSTest

using Microsoft.VisualStudio.TestTools.UnitTesting;

#elif WINDOWS_UWP
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Tests.Repository
{
    [TestClass]
    public class RepositoryWithCachingTests : TestBase
    {
        [TestMethod]
        public void BasicCrud()
        {
            var repo = new RepositoryWithCaching<Employee, int>(DataSource, EmployeeTableName);

            var emp1 = new Employee() { FirstName = "Tom", LastName = "Jones", Title = "President" };
            var echo1 = repo.Insert(emp1);

            Assert.AreNotEqual(0, echo1.EmployeeKey, "EmployeeKey was not set");
            Assert.AreEqual(emp1.FirstName, echo1.FirstName, "FirstName");
            Assert.AreEqual(emp1.LastName, echo1.LastName, "LastName");
            Assert.AreEqual(emp1.Title, echo1.Title, "Title");

            echo1.MiddleName = "G";
            var updatedEcho1 = repo.Update(echo1);

            var cached1 = repo.Get(echo1.EmployeeKey.Value);
            Assert.AreSame(updatedEcho1, cached1, "Item should have been cached");

            var emp2 = new Employee() { FirstName = "Lisa", LastName = "Green", Title = "VP Transportation", ManagerKey = echo1.EmployeeKey };
            var echo2 = repo.Insert(emp2);
            Assert.AreNotEqual(0, echo2.EmployeeKey, "EmployeeKey was not set");
            Assert.AreEqual(emp2.FirstName, echo2.FirstName, "FirstName");
            Assert.AreEqual(emp2.LastName, echo2.LastName, "LastName");
            Assert.AreEqual(emp2.Title, echo2.Title, "Title");
            Assert.AreEqual(emp2.ManagerKey, echo2.ManagerKey, "ManagerKey");

            var list = repo.GetAll();
            Assert.IsTrue(list.Any(e => e.EmployeeKey == echo1.EmployeeKey), "Employee 1 is missing");
            Assert.IsTrue(list.Any(e => e.EmployeeKey == echo2.EmployeeKey), "Employee 2 is missing");

            var list2 = repo.GetAll();
            Assert.AreSame(list, list2, "GetAll should have been cached");

            var cached1b = repo.Get(echo1.EmployeeKey.Value);
            Assert.AreNotSame(echo1, cached1b, "Cached item was replaced");
            Assert.IsTrue(list.Contains(cached1b), "Single item should have been contained in the list we previously cached.");

            var get1 = repo.Get(echo1.EmployeeKey.Value);
            Assert.AreEqual(echo1.EmployeeKey, get1.EmployeeKey);

            repo.Delete(echo2.EmployeeKey.Value);
            repo.Delete(echo1.EmployeeKey.Value);

            var list3 = repo.GetAll();
            Assert.AreEqual(list.Count - 2, list3.Count);
        }

        [TestMethod]
        public void InsertWithDictionary()
        {
            var repo = new RepositoryWithCaching<Employee, int>(DataSource, EmployeeTableName);

            var emp1 = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "FirstName", "Tom" }, { "LastName", "Jones" }, { "Title", "President" }, { "EmployeeId", Guid.NewGuid().ToString() } };
            var echo1 = repo.Insert(emp1);

            Assert.AreNotEqual(0, echo1.EmployeeKey, "EmployeeKey was not set");
            Assert.AreEqual(emp1["FirstName"], echo1.FirstName, "FirstName");
            Assert.AreEqual(emp1["LastName"], echo1.LastName, "LastName");
            Assert.AreEqual(emp1["Title"], echo1.Title, "Title");

            repo.Delete(echo1.EmployeeKey.Value);
        }

        [TestMethod]
        public void UpdateWithDictionary()
        {
            var repo = new RepositoryWithCaching<Employee, int>(DataSource, EmployeeTableName);

            var emp1 = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "FirstName", "Tom" }, { "LastName", "Jones" }, { "Title", "President" }, { "EmployeeId", Guid.NewGuid().ToString() } };
            var echo1 = repo.Insert(emp1);

            Assert.AreNotEqual(0, echo1.EmployeeKey, "EmployeeKey was not set");
            Assert.AreEqual(emp1["FirstName"], echo1.FirstName, "FirstName");
            Assert.AreEqual(emp1["LastName"], echo1.LastName, "LastName");
            Assert.AreEqual(emp1["Title"], echo1.Title, "Title");

            var emp2 = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "EmployeeKey", echo1.EmployeeKey }, { "LastName", "Brown" } };
            repo.Update(emp2);
            var echo2 = repo.Get(echo1.EmployeeKey.Value);

            //these were changed
            Assert.AreEqual(echo1.EmployeeKey, echo2.EmployeeKey, "EmployeeKey was not set");
            Assert.AreEqual(emp2["LastName"], echo2.LastName, "LastName");

            //these should be unchanged
            Assert.AreEqual(emp1["FirstName"], echo2.FirstName, "FirstName");
            Assert.AreEqual(emp1["Title"], echo2.Title, "Title");

            repo.Delete(echo1.EmployeeKey.Value);
        }
    }
}
