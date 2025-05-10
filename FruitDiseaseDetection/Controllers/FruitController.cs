using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FruitDiseaseDetection.Data;
using FruitDiseaseDetection.Models;

namespace FruitDiseaseDetection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FruitController(FruitDbContext context) : ControllerBase
    {
        private readonly FruitDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<List<Fruit>>> GetFruits()
        {
            return Ok(await _context.Fruits
                .Include(f=> f.FruitDetails)
                .Include(f => f.Diseases)
                .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Fruit>> GetFruitById(int id)
        {
            var fruit = await _context.Fruits.FindAsync(id);
            if (fruit is null)
                return NotFound();

            return Ok(fruit);
        }

        [HttpPost]
        public async Task<ActionResult<Fruit>> AddFruit(Fruit newFruit)
        {
            if (newFruit is null)
                return BadRequest();

            _context.Fruits.Add(newFruit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFruitById), new { id = newFruit.Id }, newFruit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFruit(int id, Fruit updatedFruit)
        {
            var fruit = await _context.Fruits.FindAsync(id);
            if (fruit is null)
                return NotFound();

            fruit.Name = updatedFruit.Name;
            fruit.Species = updatedFruit.Species;
            fruit.FruitDetails = updatedFruit.FruitDetails;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFruit(int id)
        {
            var fruit = await _context.Fruits.FindAsync(id);
            if (fruit is null)
                return NotFound();

            _context.Fruits.Remove(fruit);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
