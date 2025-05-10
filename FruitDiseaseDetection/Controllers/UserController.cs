using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FruitDiseaseDetection.Data;
using FruitDiseaseDetection.Models;

namespace FruitDiseaseDetection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(FruitDbContext context) : ControllerBase
    {
        private readonly FruitDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> AddUser(User newUser)
        {
            if (newUser is null)
                return BadRequest();

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
                return NotFound();

            user.Username = updatedUser.Username;
            user.Role = updatedUser.Role;
            user.Email = updatedUser.Email;
            user.Password = updatedUser.Password;


            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
