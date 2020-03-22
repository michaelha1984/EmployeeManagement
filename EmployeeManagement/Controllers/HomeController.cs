using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository employeeRepository;
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(IEmployeeRepository employeeRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            this.employeeRepository = employeeRepository;
            this.webHostEnvironment = webHostEnvironment;
        }
    
        public ViewResult Index()
        {
            var model = employeeRepository.GetAllEmployees();
            return View(model);
        }

        public ViewResult Details(int? id)
        {
            var homeDetailsViewModel = new HomeDetailsViewModel() 
            {
                Employee = employeeRepository.GetEmployee(id ?? 1),
                PageTitle = "Employee Details"
            };

            return View(homeDetailsViewModel);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string fileName = null;
                if (model.Photo != null)
                {
                    var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                    fileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    var fileStream = new FileStream(filePath, FileMode.Create);

                    model.Photo.CopyTo(fileStream);

                }

                var newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = fileName
                };


                employeeRepository.AddEmployee(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }

            return View();
        }
    }
}