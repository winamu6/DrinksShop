using drinks.Data;
using drinks.Models;
using drinks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace drinks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentApiController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly CartService _cartService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentApiController(PaymentService paymentService, CartService cartService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _cartService = cartService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] Dictionary<int, int> insertedCoins)
        {
            try
            {
                _logger.LogInformation("Received payment request: {Coins}", insertedCoins);

                var cartItems = _cartService.GetCart();
                var request = new PaymentRequest
                {
                    InsertedCoins = insertedCoins,
                    CartItems = cartItems
                };

                var result = await _paymentService.ProcessPayment(request);

                _logger.LogInformation("Payment result: {Result}", result);

                if (!result.Success)
                    return BadRequest(new { result.Success, result.Message });

                return Ok(new
                {
                    result.Success,
                    result.Message,
                    result.ChangeAmount,
                    result.ChangeCoins,
                    Order = new { result.Order.Id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing error");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Internal server error",
                    Details = ex.Message
                });
            }
        }
    }

    [Route("[controller]")]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("success/{orderId}")]
        public async Task<IActionResult> Success(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Произошла ошибка при обработке запроса");
            }
        }
    }
}