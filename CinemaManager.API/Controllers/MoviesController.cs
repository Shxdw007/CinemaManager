using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManager.API.Data;
using CinemaManager.API.Models;
using System.Linq;

namespace CinemaManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MoviesController : ControllerBase
    {
        private readonly CinemaDbContext _context;

        // Внедрение зависимости DbContext, чтобы контроллер мог общаться с БД PostgreSQL
        public MoviesController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: api/movies
        // Получить список всех фильмов
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
        {
            return await _context.Movies.ToListAsync();
        }

        // GET: api/movies/5
        // Получить один конкретный фильм по его ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Movie>> GetMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound("Фильм не найден.");
            }

            return movie;
        }

        // POST: api/movies
        // Добавить новый фильм в БД
        [HttpPost]
        public async Task<ActionResult<Movie>> PostMovie([FromForm] CreateMovieForm form)
        {
            byte[]? posterBytes = null;
            if (form.Poster is not null && form.Poster.Length > 0)
            {
                await using var ms = new MemoryStream();
                await form.Poster.CopyToAsync(ms);
                posterBytes = ms.ToArray();
            }

            var movie = new Movie
            {
                Title = form.Title ?? string.Empty,
                Genre = form.Genre ?? string.Empty,
                AgeRating = form.AgeRating ?? string.Empty,
                Duration = form.Duration ?? 0,
                Description = form.Description ?? string.Empty,
                Director = form.Director ?? string.Empty,
                PosterImage = posterBytes
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Возвращает статус 201 Created и ссылку на новый фильм
            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
        }

        // GET: api/movies/5/poster
        // Получить постер фильма
        [HttpGet("{id:int}/poster")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPoster(int id)
        {
            var movie = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (movie is null) return NotFound();
            if (movie.PosterImage is null || movie.PosterImage.Length == 0) return NotFound();

            // Минимальное требование ТЗ: вернуть image/jpeg. При желании можно улучшить определение типа по сигнатуре.
            return File(movie.PosterImage, "image/jpeg");
        }

        // PUT: api/movies/5
        // Обновить существующий фильм
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovie(int id, Movie movie)
        {
            if (id != movie.Id)
            {
                return BadRequest("ID в URL не совпадает с ID в теле запроса.");
            }

            _context.Entry(movie).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(id))
                {
                    return NotFound("Фильм не найден.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Успешно обновлено, возвращаем 204 No Content
        }

        // DELETE: api/movies/5
        // Удалить фильм из БД
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound("Фильм не найден.");
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Вспомогательный метод проверки существования фильма
        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        public sealed class CreateMovieForm
        {
            public string? Title { get; init; }
            public string? Description { get; init; }
            public string? Genre { get; init; }
            public int? Duration { get; init; }
            public string? AgeRating { get; init; }
            public string? Director { get; init; }
            public IFormFile? Poster { get; init; }
        }
    }
}