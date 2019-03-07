using System;
using System.Data.SQLite;
using System.IO;

namespace Tests
{
    public class Setup
    {
        //TODO: Redesign this to adhere to xUnit conventions.

        const string databaseFileName = "SQLiteTestDatabaseX.sqlite";

        public static void AssemblyCleanup()
        {
            File.Delete(databaseFileName);
        }

        public static void AssemblyInit()
        {
            File.Delete(databaseFileName);

            SQLiteConnection.CreateFile(databaseFileName);

            var dbConnection = new SQLiteConnection("Data Source=SQLiteTestDatabaseX.sqlite;");
            using (dbConnection)
            {
                dbConnection.Open();

                string sql = @"
CREATE TABLE Employee
(
	EmployeeKey INTEGER PRIMARY KEY,
	FirstName nvarChar(25) NOT NULL,
	MiddleName nvarChar(25) NULL,
	LastName nVarChar(25) NOT NULL,
	Title nVarChar(100) null,
    EmployeeId nvarChar(50) NOT NULL,
	ManagerKey INT NULL REferences Employee(EmployeeKey),
    OfficePhone VARCHAR(15) NULL UNIQUE ,
    CellPhone VARCHAR(15) NULL ,
    CreatedDate DateTime NOT NULL DEFAULT CURRENT_TIME,
    UpdatedDate DateTime NULL
)";

                string index = @"CREATE UNIQUE INDEX index_name ON Employee(EmployeeId);";

                string sql2 = @"CREATE TABLE Customer
(
	CustomerKey INTEGER PRIMARY KEY,
    FullName NVARCHAR(100) NULL,
	State Char(2) NOT NULL,

    CreatedByKey INTEGER NULL,
    UpdatedByKey INTEGER NULL,

	CreatedDate DATETIME2 NULL,
    UpdatedDate DATETIME2 NULL,

	DeletedFlag BIT NOT NULL Default 0,
	DeletedDate DateTimeOffset NULL,
	DeletedByKey INTEGER NULL
)";

                string viewSql = @"CREATE VIEW EmployeeWithManager
AS
SELECT  e.EmployeeKey ,
        e.FirstName ,
        e.MiddleName ,
        e.LastName ,
        e.Title ,
        e.ManagerKey ,
        e.OfficePhone ,
        e.CellPhone ,
        e.CreatedDate ,
        e.UpdatedDate ,
        m.EmployeeKey AS ManagerEmployeeKey ,
        m.FirstName AS ManagerFirstName ,
        m.MiddleName AS ManagerMiddleName ,
        m.LastName AS ManagerLastName ,
        m.Title AS ManagerTitle ,
        m.ManagerKey AS ManagerManagerKey ,
        m.OfficePhone AS ManagerOfficePhone ,
        m.CellPhone AS ManagerCellPhone ,
        m.CreatedDate AS ManagerCreatedDate ,
        m.UpdatedDate AS ManagerUpdatedDate
FROM    Employee e
        LEFT JOIN Employee m ON m.EmployeeKey = e.ManagerKey;";

                using (SQLiteCommand command = new SQLiteCommand(sql, dbConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(index, dbConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sql2, dbConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(viewSql, dbConnection))
                    command.ExecuteNonQuery();

                sql = @"INSERT INTO Employee ([EmployeeKey], [FirstName], [MiddleName], [LastName], [Title], [ManagerKey], [EmployeeId]) VALUES (@EmployeeKey, @FirstName, @MiddleName, @LastName, @Title, @ManagerKey, @EmployeeId); SELECT [EmployeeKey], [FirstName], [MiddleName], [LastName], [Title], [ManagerKey] FROM Employee WHERE ROWID = last_insert_rowid();";

                for (var i = 0; i < 10; i++)
                    using (SQLiteCommand command = new SQLiteCommand(sql, dbConnection))
                    {
                        command.Parameters.AddWithValue("@EmployeeKey", DBNull.Value);
                        command.Parameters.AddWithValue("@FirstName", "Tom");
                        command.Parameters.AddWithValue("@MiddleName", DBNull.Value);
                        command.Parameters.AddWithValue("@LastName", "Jones");
                        command.Parameters.AddWithValue("@Title", "CEO");
                        command.Parameters.AddWithValue("@ManagerKey", DBNull.Value);
                        command.Parameters.AddWithValue("@EmployeeId", Guid.NewGuid().ToString());
                        var key = command.ExecuteScalar();
                    }
            }
        }
    }
}
