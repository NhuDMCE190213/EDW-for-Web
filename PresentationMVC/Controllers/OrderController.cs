using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    public class OrderController : Controller
    {
        private readonly ICartService _cartService;

        private int CustomerId
        {
            get
            {
                var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(val, out int id)) return id;
                return 1;
            }
        }

        public OrderController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(List<int> selectedCartItemIds)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("CustomerLogin", "Auth", new { returnUrl = "/Cart" });
            }

            if (selectedCartItemIds == null || selectedCartItemIds.Count == 0)
            {
                TempData["CheckoutError"] = "Please select at least one item to proceed.";
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                var result = await _cartService.CheckoutAsync(CustomerId, selectedCartItemIds);
                return View("CheckoutSuccess", result);
            }
            catch (Exception ex)
            {
                TempData["CheckoutError"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }
    }
}
