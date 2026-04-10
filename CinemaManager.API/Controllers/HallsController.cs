using CinemaManager.API.Data;
using CinemaManager.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManager.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class HallsController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public HallsController(CinemaDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Hall>>> Get()
        => Ok(await _db.Halls.AsNoTracking().ToListAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Hall>> Get(int id)
    {
        var hall = await _db.Halls.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
        return hall is null ? NotFound() : Ok(hall);
    }

    [HttpPost]
    public async Task<ActionResult<Hall>> Post([FromBody] Hall hall)
    {
        _db.Halls.Add(hall);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = hall.Id }, hall);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Hall hall)
    {
        if (id != hall.Id) return BadRequest("ID в URL не совпадает с ID в теле запроса.");
        _db.Entry(hall).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var hall = await _db.Halls.FindAsync(id);
        if (hall is null) return NotFound();
        _db.Halls.Remove(hall);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

