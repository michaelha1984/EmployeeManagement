using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository employeeRepository;

        public HomeController(IEmployeeRepository employeeRepository)
        {
            this.employeeRepository = employeeRepository;
        }
        public string Index()
        {
            return employeeRepository.GetEmployee(1).Name;
        }

        public ObjectResult Details()
        {
            var model = employeeRepository.GetEmployee(1);
            return new ObjectResult(model);
        }
    }
}