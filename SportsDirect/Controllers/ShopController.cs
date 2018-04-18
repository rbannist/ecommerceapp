using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsDirect.Models;
using System.Text;
using System.Threading;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;

namespace SportsDirect.Controllers
{
    public class ShopController : Controller
    {
        public async Task<ActionResult> Category(string category)
        {
            ViewData["Category"] = category;

            //Fetch items from the specific category
            var CategoryProducts = await CosmosDBGraphClient<Products>.GetItemsAsync("Products", p => p.ProductCategory == category);

            return View(CategoryProducts);
        }

        public async Task<ActionResult> Tag(string tag)
        {
            ViewData["Tag"] = tag;

            //Fetch items containing a specific tag
            var TagProducts = await CosmosDBGraphClient<Products>.GetItemsAsync("Products", p => p.ProductTags.Contains(tag));

            return View(TagProducts);
        }

        public async Task<ActionResult> Product(string selectedProductId)
        {
            //Fetch product data from Cosmos DB
            var ProductDetails = await CosmosDBGraphClient<Products>.GetItemAsync(selectedProductId, "Products");

            return View(ProductDetails);
        }

        [Authorize]
        public async Task<ActionResult> AddToCart(string ItemName, string ItemId, string ItemCategory)
        {
            //Get the user's current data (containing their shopping cart)
            Users UserData = await CosmosDBGraphClient<Users>.GetItemAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value, "Users");

            //Add the new item to the Shopping Cart dictionary
            UserData.ShoppingCart.Add(ItemName, ItemId);

            //Return the updated data to CosmosDB
            var ShoppingCartUpdate = await CosmosDBGraphClient<Users>.UpdateItemAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value, UserData, "Users");

            return RedirectToAction(nameof(ShopController.Category), "Shop", new { category = ItemCategory });
        }

        [Authorize]
        public async Task<ActionResult> CreateOrder(List<string> OrderedProducts)
        {
            //Create a new order model object
            Orders order = new Orders();
            order.OrderedProducts = OrderedProducts;
            order.OrderUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            order.OrderStatus = "Created";

            //Create a new order in Cosmos DB
            var OrderPost = await CosmosDBGraphClient<Orders>.CreateItemAsync(order, "Orders");

            //Place the Order on a Service Bus queue for fulfillment processing
            await ServiceBusClient.SendMessageAsync(order.ToString());

            //Add the newly generated order ID to the user's order history
            var UserProfile = await CosmosDBGraphClient<Users>.GetItemAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value, "Users");
            UserProfile.OrderHistory.Add(OrderPost.Id);

            //Clear their shopping cart
            UserProfile.ShoppingCart.Clear();

            //Update Cosmos DB with the changes
            var UpdatedUserProfile = await CosmosDBGraphClient<Users>.UpdateItemAsync(UserProfile.UserId, UserProfile, "Users");

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
