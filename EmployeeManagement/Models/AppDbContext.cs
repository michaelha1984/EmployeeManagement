using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        { 

        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "Michael",
                    Department = Dept.IT,
                    Email = "michael@company.com"
                },
                new Employee
                {
                    Id = 2,
                    Name = "Mick",
                    Department = Dept.Payroll,
                    Email = "mick@company.com"
                },
                new Employee
                {
                    Id = 3,
                    Name = "Michelle",
                    Department = Dept.HR,
                    Email = "michelle@company.com"
                }
                );
        }
    }
}
