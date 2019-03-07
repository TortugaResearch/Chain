using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using Tortuga.Chain;
using Tortuga.Chain.DataSources;
using Tortuga.Chain.SQLite;

namespace Tests
{
    [TestClass]
    public class Setup
    {
        const string databaseFileName = "SQLiteTestDatabase.sqlite";

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            DataSource.GlobalExecutionCanceled += DefaultDispatcher_ExecutionCanceled;
            DataSource.GlobalExecutionError += DefaultDispatcher_ExecutionError;
            DataSource.GlobalExecutionFinished += DefaultDispatcher_ExecutionFinished;
            DataSource.GlobalExecutionStarted += DefaultDispatcher_ExecutionStarted;

#if !Roslyn_Missing
            CompiledMaterializers.MaterializerCompiled += CompiledMaterializers_MaterializerCompiled;
            CompiledMaterializers.MaterializerCompilerFailed += CompiledMaterializers_MaterializerCompiled;
#endif

            File.Delete(databaseFileName);

            SQLiteConnection.CreateFile(databaseFileName);
            var dbConnection = new SQLiteConnection("Data Source=SQLiteTestDatabase.sqlite;");
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
	ManagerKey INT NULL REferences Employee(EmployeeKey),
    CreatedDate DateTime NOT NULL DEFAULT CURRENT_TIME,
    UpdatedDate DateTime NULL,
    EmployeeId nvarChar(50) NOT NULL
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

                using (SQLiteCommand command = new SQLiteCommand(sql, dbConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(index, dbConnection))
                    command.ExecuteNonQuery();

                using (SQLiteCommand command = new SQLiteCommand(sql2, dbConnection))
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

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            File.Delete(databaseFileName);
        }

        static void DefaultDispatcher_ExecutionCanceled(object sender, ExecutionEventArgs e)
        {
            Debug.WriteLine("******");
            Debug.WriteLine($"Execution canceled: {e.ExecutionDetails.OperationName}. Duration: {e.Duration.Value.TotalSeconds.ToString("N3")} sec.");
            //WriteDetails(e);
        }

#if !Roslyn_Missing

        static void CompiledMaterializers_MaterializerCompiled(object sender, MaterializerCompilerEventArgs e)
        {
            Debug.WriteLine("******");
            Debug.WriteLine("Compiled Materializer");
            Debug.Indent();
            Debug.WriteLine("SQL");
            Debug.WriteLine(e.Sql);
            Debug.WriteLine("Code");
            Debug.WriteLine(e.Code);
            Debug.Unindent();
        }

#endif

        static void DefaultDispatcher_ExecutionError(object sender, ExecutionEventArgs e)
        {
            Debug.WriteLine("******");
            Debug.WriteLine($"Execution error: {e.ExecutionDetails.OperationName}. Duration: {e.Duration.Value.TotalSeconds.ToString("N3")} sec.");
            //WriteDetails(e);
        }

        static void DefaultDispatcher_ExecutionFinished(object sender, ExecutionEventArgs e)
        {
            Debug.WriteLine("******");
            Debug.WriteLine($"Execution finished: {e.ExecutionDetails.OperationName}. Duration: {e.Duration.Value.TotalSeconds.ToString("N3")} sec. Rows affected: {(e.RowsAffected != null ? e.RowsAffected.Value.ToString("N0") : "<NULL>")}.");
            //WriteDetails(e);
        }

        static void DefaultDispatcher_ExecutionStarted(object sender, ExecutionEventArgs e)
        {
            Debug.WriteLine("******");
            Debug.WriteLine($"Execution started: {e.ExecutionDetails.OperationName}");
            WriteDetails(e);
        }

        static void WriteDetails(ExecutionEventArgs e)
        {
            if (e.ExecutionDetails is SQLiteCommandExecutionToken)
            {
                Debug.WriteLine("");
                Debug.WriteLine("Command text: ");
                Debug.WriteLine(e.ExecutionDetails.CommandText);
                //Debug.Indent();
                foreach (var item in ((SQLiteCommandExecutionToken)e.ExecutionDetails).Parameters)
                    Debug.WriteLine(item.ParameterName + ": " + (item.Value == null || item.Value == DBNull.Value ? "<NULL>" : item.Value));
                //Debug.Unindent();
                Debug.WriteLine("******");
                Debug.WriteLine("");
            }
        }
    }
}
