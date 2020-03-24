using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AdministrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }

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

                var isInRole = await userManager.IsInRoleAsync(user, role.Name); // Provided user (not current user)
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

        [HttpGet]
        public async Task<IActionResult> EditUserAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("Not Found");
            }

            var userClaims = await userManager.GetClaimsAsync(user);
            var userRoles = await userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                City = user.City,
                Claims = userClaims.Select(c => c.Value).ToList(),
                Roles = userRoles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserAsync(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("Not Found");
            }

            user.Email = model.Email;
            user.UserName = model.UserName;
            user.City = model.City;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("Not Found");
            }

            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("ListUsers");
        }

        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRoleAsync(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("Not Found");
            }

            try
            {
                var result = await roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListRoles");
            }
            catch (DbUpdateException)
            {
                logger.LogError($"Error delete role {role.Name}.");

                ViewBag.ErrorTitle = $"{role.Name} is in use.";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users in this role. " +
                    $"If you want to delete this role, please remove the users from the role and then try to delete this role.";
                return View("Error")
;           }
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.UserId = userId;

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var modelList = new List<ManageUserRolesViewModel>();

            foreach (var role in roleManager.Roles.ToList())
            {
                var model = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                var isInRole = await userManager.IsInRoleAsync(user, role.Name);

                if (isInRole)
                {
                    model.IsSelected = true;
                }

                modelList.Add(model);
            }

            return View(modelList);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<ManageUserRolesViewModel> modelList, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMEssage = $"User with Id = {userId} not found";
                return View("NotFound");
            }

            var allRoles = await userManager.GetRolesAsync(user);
            var removeResult = await userManager.RemoveFromRolesAsync(user, allRoles);

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing role");
                return View(modelList);
            }

            var rolesToAdd = modelList.Where(m => m.IsSelected).Select(m => m.RoleName);
            var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);

            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add roles to user");
                return View(modelList);
            }

            return RedirectToAction("EditUser", new { Id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            ViewBag.UserId = userId;

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var currentUserClaims = await userManager.GetClaimsAsync(user);

            var model = new ManageUserClaimsViewModel
            {
                UserId = user.Id
            };

            foreach (var claim in ClaimsStore.AllClaims)
            {
                var userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };

                if (currentUserClaims.Any(c => c.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }

                model.UserClaims.Add(userClaim);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(ManageUserClaimsViewModel model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMEssage = $"User with Id = {userId} not found";
                return View("NotFound");
            }

            var allClaims = await userManager.GetClaimsAsync(user);
            var removeResult = await userManager.RemoveClaimsAsync(user, allClaims);

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            var claimsToAdd = model.UserClaims.Where(m => m.IsSelected).Select(m => new Claim(m.ClaimType, m.ClaimType));
            var addResult = await userManager.AddClaimsAsync(user, claimsToAdd);

            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add roles to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = userId });
        }
    }
}