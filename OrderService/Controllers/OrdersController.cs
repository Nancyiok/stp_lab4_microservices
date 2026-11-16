using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _db;
        private readonly UserClient _userClient;

        public OrdersController(OrderDbContext db, UserClient userClient)
        {
            _db = db;
            _userClient = userClient;
        }

        /// <summary>
        /// Повертає список усіх замовлень.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _db.Orders.OrderBy(o => o.Id).ToListAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Повертає замовлення за ідентифікатором.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Order), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        /// <summary>
        /// Створює замовлення. Перед збереженням перевіряє користувача в UserService.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Order), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            var error = await CheckUser(order.UserId);
            if (error != null)
            {
                return error;
            }

            order.Id = 0;
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        /// <summary>
        /// Оновлює замовлення.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            var existing = await _db.Orders.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var error = await CheckUser(order.UserId);
            if (error != null)
            {
                return error;
            }

            existing.UserId = order.UserId;
            existing.ProductName = order.ProductName;
            existing.Price = order.Price;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Видаляє замовлення.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Отримує інформацію про користувача через UserService.
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<User>> GetUser(int userId)
        {
            try
            {
                var user = await _userClient.GetUserAsync(userId);
                if (user == null)
                {
                    return NotFound($"Користувача {userId} не знайдено в UserService");
                }
                return Ok(user);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "UserService недоступний");
            }
        }

        /// <summary>
        /// Повертає замовлення разом з даними користувача.
        /// </summary>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(OrderDetails), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<OrderDetails>> GetOrderDetails(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var details = new OrderDetails { Order = order };
            try
            {
                details.User = await _userClient.GetUserAsync(order.UserId);
            }
            catch (HttpRequestException)
            {
                // user service is down, return the order anyway
                details.User = null;
            }
            return Ok(details);
        }

        // null = user exists, otherwise the error to return
        private async Task<ActionResult?> CheckUser(int userId)
        {
            try
            {
                var user = await _userClient.GetUserAsync(userId);
                if (user == null)
                {
                    return BadRequest($"Користувача {userId} не існує");
                }
                return null;
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "UserService недоступний");
            }
        }
    }
}
