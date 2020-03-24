using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> LogoutAsync()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    City = model.City
                };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var isSigned = signInManager.IsSignedIn(User);
                    var isAdmin = User.IsInRole("Admin"); // Current user

                    if (isSigned && isAdmin) 
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }

                    await signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("index", "home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                     
                    return RedirectToAction("index", "home");
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }

            return View(model);
        }

        [HttpPost][HttpGet]
        [AllowAnonymous]
        // Mvc uses jquery validate method (using an AJAX call) to call server side method.
        // Hence why we need to return a json result.
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(true);
            }

            return Json($"Email {email} is already in use.");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // Redirect to external (google) login
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        // This will be called after successfully logged into external (google) login 
        public async Task<IActionResult> ExternalLoginCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/"); // root url

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login", model);
            }

            var info = await signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                ModelState.AddModelError(string.Empty, $"Error loading external login information: {remoteError}");
                return View("Login", model);
            }

            // Try sign in with external login details 
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                // We already have this person's login. Do nothing more.
                return LocalRedirect(returnUrl);
            }
            else
            { 
                var email = info.Principal.FindFirst(ClaimTypes.Email).Value;

                if (email != null)
                {
                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        // Create a new user
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirst(ClaimTypes.Email).Value,
                            Email = info.Principal.FindFirst(ClaimTypes.Email).Value
                        };

                        await userManager.CreateAsync(user);
                    }

                    // Add user to login and sign in
                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = $"Email claim not received from: {info.LoginProvider}";

                return View("Error");
            }
        }
    }
}