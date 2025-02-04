﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class SqlEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<SqlEmployeeRepository> logger;

        public SqlEmployeeRepository(AppDbContext context,
            ILogger<SqlEmployeeRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public Employee AddEmployee(Employee employee)
        {
            context.Employees.Add(employee);
            context.SaveChanges();
            return employee;
        }

        public Employee DeleteEmployee(int id)
        {
            var employee = context.Employees.Find(id);
            
            if (employee != null)
            {
                context.Employees.Remove(employee);
                context.SaveChanges();
            }

            return employee;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return context.Employees;
        }

        public Employee GetEmployee(int id)
        {
            return context.Employees.Find(id);
        }

        public Employee UpdateEmployee(Employee employeeChanges)
        {
            var employee = context.Employees.Update(employeeChanges); 
            context.SaveChanges();

            return employeeChanges;
        }
    }
}
