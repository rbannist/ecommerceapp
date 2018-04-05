using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsDirect.Models;

namespace SportsDirect.Controllers
{
    public class ShopController : Controller
    {
        [ActionName("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddToCart(string ItemName, string ItemId)
        {
            //Get the user's current data (containing their shopping cart)
            Users UserData = await CosmosDBGraphClient<Users>.GetItemAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value, "SportsDirectWebsite", "Users");
            //Add the new item to the Shopping Cart dictionary
            UserData.ShoppingCart.Add(ItemName, ItemId);
            //Return the updated data to CosmosDB
            var ShoppingCartUpdate = await CosmosDBGraphClient<Users>.CreateItemAsync(UserData, "SportsDirectWebsite", "Users");

            return View(ShoppingCartUpdate);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrder([FromForm]Orders Order)
        {
            var OrderPost = await CosmosDBGraphClient<Orders>.CreateItemAsync(Order, "SportsDirectWebsite", "Orders");

            return View(OrderPost);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
