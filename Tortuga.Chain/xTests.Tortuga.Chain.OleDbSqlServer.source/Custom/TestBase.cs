using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.AuditRules;
using Tortuga.Chain.Core;
using Tortuga.Chain.DataSources;
using Tortuga.Chain.SqlServer;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Tests
{
    public abstract partial class TestBase
    {


        static public readonly string AssemblyName = "OleDb SQL Server";
        static protected readonly Dictionary<string, OleDbSqlServerDataSource> s_DataSources = new Dictionary<string, OleDbSqlServerDataSource>();
        protected static readonly OleDbSqlServerDataSource s_PrimaryDataSource;

        static TestBase()
        {

            foreach (ConnectionStringSettings con in ConfigurationManager.ConnectionStrings)
            {
                var ds = new OleDbSqlServerDataSource(con.Name, con.ConnectionString);
                s_DataSources.Add(con.Name, ds);
                if (s_PrimaryDataSource == null) s_PrimaryDataSource = ds;
            }



        }

        public static string CustomerTableName { get { return "Sales.Customer"; } }

        public static string EmployeeTableName { get { return "HR.Employee"; } }

        public string MultiResultSetProc1Name { get { return "Sales.CustomerWithOrdersByState"; } }

        public string TableFunction1Name { get { return "Sales.CustomersByState"; } }

        public string TableFunction2Name { get { return "Sales.CustomersByStateInline"; } }

        public string ScalarFunction1Name { get { return "HR.EmployeeCount"; } }

        public OleDbSqlServerDataSource AttachRules(OleDbSqlServerDataSource source)
        {
            return source.WithRules(
                new DateTimeRule("CreatedDate", DateTimeKind.Local, OperationTypes.Insert),
                new DateTimeRule("UpdatedDate", DateTimeKind.Local, OperationTypes.InsertOrUpdate),
                new UserDataRule("CreatedByKey", "EmployeeKey", OperationTypes.Insert),
                new UserDataRule("UpdatedByKey", "EmployeeKey", OperationTypes.InsertOrUpdate),
                new ValidateWithValidatable(OperationTypes.InsertOrUpdate)
                );
        }

        public OleDbSqlServerDataSource DataSource(string name, [CallerMemberName] string caller = null)
        {
            WriteLine($"{caller} requested Data Source {name}");

            return AttachTracers(s_DataSources[name]);
        }

        public OleDbSqlServerDataSourceBase DataSource(string name, DataSourceType mode, [CallerMemberName] string caller = null)
        {
            WriteLine($"{caller} requested Data Source {name} with mode {mode}");

            var ds = s_DataSources[name];
            switch (mode)
            {
                case DataSourceType.Normal: return AttachTracers(ds);
                case DataSourceType.Transactional: return AttachTracers(ds.BeginTransaction());
                case DataSourceType.Open:
                    var root = (IRootDataSource)ds;
                    return AttachTracers((OleDbSqlServerDataSourceBase)root.CreateOpenDataSource(root.CreateConnection(), null));
            }
            throw new ArgumentException($"Unkown mode {mode}");
        }

        public async Task<OleDbSqlServerDataSourceBase> DataSourceAsync(string name, DataSourceType mode, [CallerMemberName] string caller = null)
        {
            WriteLine($"{caller} requested Data Source {name} with mode {mode}");

            var ds = s_DataSources[name];
            switch (mode)
            {
                case DataSourceType.Normal: return AttachTracers(ds);
                case DataSourceType.Transactional: return AttachTracers(await ds.BeginTransactionAsync());
                case DataSourceType.Open:
                    var root = (IRootDataSource)ds;
                    return AttachTracers((OleDbSqlServerDataSourceBase)root.CreateOpenDataSource(await root.CreateConnectionAsync(), null));
            }
            throw new ArgumentException($"Unkown mode {mode}");
        }

        void WriteDetails(ExecutionEventArgs e)
        {
            if (e.ExecutionDetails is OleDbCommandExecutionToken)
            {
                WriteLine("");
                WriteLine("Command text: ");
                WriteLine(e.ExecutionDetails.CommandText);
                //Indent();
                foreach (var item in ((OleDbCommandExecutionToken)e.ExecutionDetails).Parameters)
                    WriteLine(item.ParameterName + ": " + (item.Value == null || item.Value == DBNull.Value ? "<NULL>" : item.Value) + " [" + item.OleDbType + "]");
                //Unindent();
                WriteLine("******");
                WriteLine("");
            }
        }
    }
}
