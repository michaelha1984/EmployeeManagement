﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>()
            {
                new Employee() { Id = 1, Name = "Michael", Department = Dept.IT, Email = "michael@email.com"},
                new Employee() { Id = 2, Name = "Michelle", Department = Dept.HR, Email = "michelle@email.com"},
                new Employee() { Id = 3, Name = "Mike", Department = Dept.Payroll, Email = "mike@email.com"}
            };
        }

        public Employee AddEmployee(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);
            return employee;
        }

        public Employee DeleteEmployee(int id)
        {
            var employee = _employeeList.FirstOrDefault(e => e.Id == id);

            if (employee != null)
            {
                _employeeList.Remove(employee);
            }

            return employee;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeList;
        }

        public Employee GetEmployee(int id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == id);
        }

        public Employee UpdateEmployee(Employee employeeChanges)
        {
            var employee = _employeeList.FirstOrDefault(e => e.Id == employeeChanges.Id);

            if (employee != null)
            {
                employee.Name = employeeChanges.Name;
                employee.Email = employeeChanges.Email;
                employee.Department = employeeChanges.Department;
            }

            return employee;
        }
    }
}
