using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _db;

        public UsersController(UserDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Повертає список усіх користувачів.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _db.Users.OrderBy(u => u.Id).ToListAsync();
            return Ok(users);
        }

        /// <summary>
        /// Повертає користувача за ідентифікатором.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Створює нового користувача.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(User), 201)]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            user.Id = 0;
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Оновлює дані користувача.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            var existing = await _db.Users.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Email = user.Email;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Видаляє користувача.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
