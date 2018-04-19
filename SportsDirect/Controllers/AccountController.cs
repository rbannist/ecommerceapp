using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SportsDirect.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SportsDirect.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        //Below calls configuration appsettings for AADB2C
        private readonly AzureAdB2COptions _b2cOptions;

        public AccountController(IOptions<AzureAdB2COptions> b2cOptions)
        {
            _b2cOptions = b2cOptions.Value;
        }
        //--------------------------------------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult SignIn()//Or sign up
        {
            var redirectUrl = Url.Action(nameof(AccountController.NewUser));//Checks if they are new user
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<ActionResult> NewUser()
        {
            //Added authentication check below
            bool newUser = false;
            bool.TryParse(User.FindFirst("newUser")?.Value, out newUser);//Try and find the newUser boolean claim and put into variable

            if (User.Identity.IsAuthenticated)
            {
                if (newUser)
                {
                    //Create user profile in Cosmos DB
                    Users user = new Users();
                    user.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    user.FirstName = User.Identity.Name;
                    user.LastName = User.FindFirst(ClaimTypes.Surname).Value;
                    user.ShoppingCart = new Dictionary<string, string>();
                    user.OrderHistory = new List<string>();

                    var newUserProfile = await CosmosDBClient<Users>.CreateItemAsync(user, "Users");

                    return RedirectToAction(nameof(HomeController.Index));
                }

                return RedirectToAction(nameof(HomeController.Index));
            }
            return RedirectToAction(nameof(HomeController.Index));
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items[AzureAdB2COptions.PolicyAuthenticationProperty] = _b2cOptions.ResetPasswordPolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items[AzureAdB2COptions.PolicyAuthenticationProperty] = _b2cOptions.EditProfilePolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
