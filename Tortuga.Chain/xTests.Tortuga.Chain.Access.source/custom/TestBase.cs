using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tests.Models;
using Tortuga.Chain;
using Tortuga.Chain.Access;
using Tortuga.Chain.AuditRules;
using Tortuga.Chain.DataSources;

namespace Tests
{
    public abstract partial class TestBase
    {
        static public readonly string AssemblyName = "Access";
        static protected readonly Dictionary<string, AccessDataSource> s_DataSources = new Dictionary<string, AccessDataSource>();
        protected static readonly AccessDataSource s_PrimaryDataSource;

        static TestBase()
        {
            Setup.AssemblyInit();
            foreach (ConnectionStringSettings con in ConfigurationManager.ConnectionStrings)
            {
                var ds = new AccessDataSource(con.Name, con.ConnectionString);
                s_DataSources.Add(con.Name, ds);
                if (s_PrimaryDataSource == null) s_PrimaryDataSource = ds;
            }
            BuildEmployeeSearchKey1000(s_PrimaryDataSource);
        }

        public static string CustomerTableName { get { return "Customer"; } }

        public static string EmployeeTableName { get { return "Employee"; } }
        public static string EmployeeViewName { get { return "EmployeeLookup"; } }

        public AccessDataSource AttachRules(AccessDataSource source)
        {
            return source.WithRules(
                new DateTimeRule("CreatedDate", DateTimeKind.Local, OperationTypes.Insert),
                new DateTimeRule("UpdatedDate", DateTimeKind.Local, OperationTypes.InsertOrUpdate),
                new UserDataRule("CreatedByKey", "EmployeeKey", OperationTypes.Insert),
                new UserDataRule("UpdatedByKey", "EmployeeKey", OperationTypes.InsertOrUpdate),
                new ValidateWithValidatable(OperationTypes.InsertOrUpdate)
                );
        }

        public AccessDataSource AttachSoftDeleteRulesWithUser(AccessDataSource source)
        {
            var currentUser1 = source.From(EmployeeTableName).WithLimits(1).ToObject<Employee>().Execute();

            return source.WithRules(
                new SoftDeleteRule("DeletedFlag", true, OperationTypes.SelectOrDelete),
                new UserDataRule("DeletedByKey", "EmployeeKey", OperationTypes.Delete),
                new DateTimeRule("DeletedDate", DateTimeKind.Local, OperationTypes.Delete)
                ).WithUser(currentUser1);
        }

        public AccessDataSource DataSource(string name, [CallerMemberName] string caller = null)
        {
            WriteLine($"{caller} requested Data Source {name}");

            return AttachTracers(s_DataSources[name]);
        }

        public AccessDataSourceBase DataSource(string name, DataSourceType mode, [CallerMemberName] string caller = null)
        {
            WriteLine($"{caller} requested Data Source {name} with mode {mode}");

            var ds = s_DataSources[name];
            switch (mode)
            {
                case DataSourceType.Normal: return AttachTracers(ds);
                case DataSourceType.Strict: return AttachTracers(ds).WithSettings(new AccessDataSourceSettings() { StrictMode = true });
                case DataSourceType.Transactional: return AttachTracers(ds.BeginTransaction());
                case DataSourceType.Open:
                    var root = (IRootDataSource)ds;
                    return AttachTracers((AccessDataSourceBase)root.CreateOpenDataSource(root.CreateConnection(), null));
            }
            throw new ArgumentException($"Unkown mode {mode}");
        }

        public async Task<AccessDataSourceBase> DataSourceAsync(string name, DataSourceType mode, [CallerMemberName] string caller = null)
        {
            WriteLine($"{caller} requested Data Source {name} with mode {mode}");

            var ds = s_DataSources[name];
            switch (mode)
            {
                case DataSourceType.Normal: return AttachTracers(ds);
                case DataSourceType.Strict: return AttachTracers(ds).WithSettings(new AccessDataSourceSettings() { StrictMode = true });
                case DataSourceType.Transactional: return AttachTracers(await ds.BeginTransactionAsync());
                case DataSourceType.Open:
                    var root = (IRootDataSource)ds;
                    return AttachTracers((AccessDataSourceBase)root.CreateOpenDataSource(await root.CreateConnectionAsync(), null));
            }
            throw new ArgumentException($"Unkown mode {mode}");
        }

        void WriteDetails(ExecutionEventArgs e)
        {
            var token = e.ExecutionDetails as AccessCommandExecutionToken;
            if (token == null)
                return;

            WriteLine("");
            WriteLine("Command text: ");
            WriteLine(e.ExecutionDetails.CommandText);
            //m_Output.Indent();
            if (token.Parameters != null)
                foreach (var item in token.Parameters)
                    WriteLine(item.ParameterName + ": " + (item.Value == null || item.Value == DBNull.Value ? "<NULL>" : item.Value));
            //m_Output.Unindent();
            WriteLine("******");
            WriteLine("");
        }
    }
}
