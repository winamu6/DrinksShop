using drinks.Models.Entities;
using drinks.Models.ViewModel;
using Drinks.Models.ViewModel;
using Drinks.Services.CartServices;
using Drinks.Services.ProductServices.ProductInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace drinks.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly CartService _cartService;

        public ProductsController(IProductService productService, CartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index(int? brandId, decimal? maxPrice)
        {
            try
            {
                var products = await _productService.GetProductsAsync(brandId, maxPrice: maxPrice);
                var brands = await _productService.GetBrandsAsync();
                var priceRange = await _productService.GetPriceRangeAsync();

                var cartItems = _cartService.GetCart();
                var cartProductIds = cartItems.Select(i => i.ProductId).ToHashSet();

                var productViewModels = products?.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    IsInCart = cartProductIds.Contains(p.Id)
                }).ToList() ?? new List<ProductViewModel>();

                ViewBag.Brands = brands ?? new List<Brand>();
                ViewBag.SelectedBrand = brandId ?? 0;
                ViewBag.MaxPrice = maxPrice ?? priceRange.MaxPrice;
                ViewBag.MinPrice = priceRange.MinPrice;
                ViewBag.MaxPriceRange = priceRange.MaxPrice;

                return View(productViewModels);
            }
            catch (Exception)
            {
                ViewBag.Brands = new List<Brand>();
                ViewBag.SelectedBrand = 0;
                ViewBag.MaxPrice = 100;
                ViewBag.MinPrice = 0;
                ViewBag.MaxPriceRange = 100;

                return View(new List<ProductViewModel>());
            }
        }
    }
}