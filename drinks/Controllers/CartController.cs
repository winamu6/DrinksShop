using Drinks.Data;
using Drinks.Models;
using Drinks.Models.ViewModel;
using Drinks.Services.CartServices;
using Drinks.Services.ProductServices.ProductInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace Drinks.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly IProductService _productService;
        private readonly AppDbContext _context;

        public CartController(
            CartService cartService,
            IProductService productService,
            AppDbContext context)
        {
            _cartService = cartService;
            _productService = productService;
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(int productId, int quantity = 1)
        {
            Console.WriteLine($"AddItem called: productId={productId}, quantity={quantity}");

            var product = await _productService.GetProductAsync(productId);
            if (product == null)
            {
                Console.WriteLine("Product not found");
                return BadRequest(new { message = "Товар не найден" });
            }

            Console.WriteLine($"Adding product: {product.Name}");
            _cartService.AddToCart(product, quantity);

            var count = _cartService.GetCartCount();
            Console.WriteLine($"Cart count after add: {count}");

            return Ok(new { count = count });
        }

        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            _cartService.RemoveFromCart(productId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Ok();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateItem(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                _cartService.RemoveFromCart(productId);
            }
            else
            {
                _cartService.UpdateQuantity(productId, quantity);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetCount()
        {
            return Json(new { count = _cartService.GetCartCount() });
        }

        public IActionResult Checkout()
        {
            var cartItems = _cartService.GetCart();
            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var coins = _context.Coins.ToList();
            ViewBag.TotalAmount = _cartService.GetCartTotal();
            ViewBag.CartItems = cartItems;

            return View("~/Views/Payment/Index.cshtml", coins);
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int productId)
        {
            _cartService.IncreaseQuantity(productId);
            return GetUpdatedCartData(productId);
        }

        [HttpPost]
        public IActionResult DecreaseQuantity(int productId)
        {
            _cartService.DecreaseQuantity(productId);
            return GetUpdatedCartData(productId);
        }

        [HttpPost]
        public IActionResult SetQuantity(int productId, int quantity)
        {
            _cartService.SetQuantity(productId, quantity);
            return GetUpdatedCartData(productId);
        }

        private IActionResult GetUpdatedCartData(int productId)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            return Json(new
            {
                success = true,
                quantity = item?.Quantity ?? 0,
                total = item?.Total ?? 0,
                cartTotal = _cartService.GetCartTotal(),
                itemsCount = _cartService.GetCartCount()
            });
        }

        [HttpGet]
        public IActionResult GetCartItems()
        {
            var cartItems = _cartService.GetCart();
            var items = cartItems.Select(item => new
            {
                productId = item.ProductId,
                quantity = item.Quantity
            });

            return Json(items);
        }

        [HttpGet]
        public IActionResult GetCartTotal()
        {
            var total = _cartService.GetCartTotal();
            return Json(total);
        }

    }
}