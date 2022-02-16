﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Models
{
	/// <summary>
	/// This is used to test immutable object constructors
	/// </summary>
	[Table("Employee", Schema = "HR")]
	public class EmployeeLookup
	{
#if SQLITE

        public EmployeeLookup(long employeeKey, string firstName, string lastName)
        {
            EmployeeKey = (int)employeeKey;
            FirstName = firstName;
            LastName = lastName;
        }

#elif MYSQL

		public EmployeeLookup(ulong employeeKey, string firstName, string lastName)
		{
			EmployeeKey = (int)employeeKey;
			FirstName = firstName;
			LastName = lastName;
		}

#else

        public EmployeeLookup(int employeeKey, string firstName, string lastName)
        {
            EmployeeKey = employeeKey;
            FirstName = firstName;
            LastName = lastName;
        }

#endif

		public int EmployeeKey { get; }
		public string FirstName { get; }
		public string LastName { get; }
	}
}
