using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Radial.Data.Entities;
using Radial.Models;
using Radial.Services;

namespace Radial.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<RadialUser> _signInManager;
        private readonly UserManager<RadialUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSenderEx _emailSender;
        private readonly IWorld _world;

        public RegisterModel(
            UserManager<RadialUser> userManager,
            SignInManager<RadialUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSenderEx emailSender,
            IWorld world)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _world = world;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public List<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(20, MinimumLength = 4)]
            [RegularExpression(@"^[a-zA-Z0-9_\-]*$", ErrorMessage = "Username can only contain letters, numbers, hyphens, and underscores.")]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                if (Input.Username.ToLower().Contains("system") ||
                    Input.Username.ToLower().Contains("admin"))
                {
                    ModelState.AddModelError("Input.Username", "Invalid username.");
                    return Page();
                }

                if (await _userManager.FindByEmailAsync(Input.Email) != null)
                {
                    ModelState.AddModelError("Input.Email", "Email address is already in use.");
                    return Page();
                }

                var characterGuid = Guid.NewGuid();

                var user = new RadialUser 
                {
                    UserName = Input.Username, 
                    Email = Input.Email,
                    CharacterId = characterGuid
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var startLocation = _world.Locations.Find(x => x.XYZ == "0,0,0");

                    var character = new PlayerCharacter()
                    {
                        Id = characterGuid,
                        CoreEnergy = 100,
                        EnergyCurrent = 100,
                        Name = Input.Username,
                        Type = Enums.CharacterType.Player,
                        UserId = user.Id
                    };

                    startLocation.Characters.Add(character);
                    await _world.Save();

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    var emailResult = await _emailSender.TrySendEmail(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (!emailResult)
                    {
                        await _userManager.ConfirmEmailAsync(user, token);
                    }

                    if (emailResult && _userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
