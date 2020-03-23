using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoleAsync(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                var result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("listroles", "administration");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            
            return View(model);

        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoleAsync(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach (var user in userManager.Users.ToList())
            {
                var isInRole = await userManager.IsInRoleAsync(user, role.Name);
                
                if (isInRole)
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditRoleAsync(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;

                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
                else
                {
                    ModelState.AddModelError("", "Update role unsuccesful.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRoleAsync(string id)
        {
            ViewBag.RoleId = id;

            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("Not Found");
            }

            var userRoleList = new List<UserRoleViewModel>();

            foreach (var user in userManager.Users.ToList())
            {
                var model = new UserRoleViewModel()
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                var isInRole = await userManager.IsInRoleAsync(user, role.Name);
                if (isInRole)
                {
                    model.IsSelected = true;
                }
                else
                {
                    model.IsSelected = false;
                }

                userRoleList.Add(model);
            }

            return View(userRoleList);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRoleAsync(List<UserRoleViewModel> userRoleList, string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("Not Found");
            }

            foreach (var model in userRoleList)
            {
                var user = await userManager.FindByIdAsync(model.UserId);
                var isInRole = await userManager.IsInRoleAsync(user, role.Name);

                if (model.IsSelected && !isInRole)
                {
                    var addResult = await userManager.AddToRoleAsync(user, role.Name);

                    if (!addResult.Succeeded)
                    {
                        ModelState.AddModelError("", $"Could not add user {user.UserName} to role");
                    }
                }
                else if (!model.IsSelected && isInRole)
                {
                    var removeResult = await userManager.RemoveFromRoleAsync(user, role.Name);

                    if (!removeResult.Succeeded)
                    {
                        ModelState.AddModelError("", $"Could not remove user {user.UserName} from role");
                    }
                }
                else
                {
                    continue;
                }
            }

            return RedirectToAction("EditRole", new { id = id });
        }
    }
}