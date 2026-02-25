using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers;

public class ReviewInput
{
    public int IdUtilizador { get; set; }
    public int IdLivro { get; set; }
    public string? TextoReview { get; set; }
    public byte? Rating { get; set; }
    public string Status { get; set; } = "Pendente";
    public DateOnly DataSubmissao { get; set; }
}

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public ReviewsController(ApplicationDbContext _context)
    {
        context = _context;
    }

    // GET: api/Reviews
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviews([FromQuery] string? status, [FromQuery] int? livroId, [FromQuery] int? utilizadorId)
    {
        var query = context.Reviews
            .Include(r => r.IdLivroNavigation)
                .ThenInclude(l => l.Autores)
            .Include(r => r.IdUtilizadorNavigation)
            .AsQueryable();

        // Filter by status
        if (status is not null and not "")
        {
            query = query.Where(r => r.Status == status);
        }

        // Filter by livro id
        if (livroId.HasValue)
        {
            query = query.Where(r => r.IdLivro == livroId.Value);
        }

        // Filter by utilizador id
        if (utilizadorId.HasValue)
        {
            query = query.Where(r => r.IdUtilizador == utilizadorId.Value);
        }

        return await query.ToListAsync();
    }

    // GET: api/Reviews/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Review>> GetReview(int id)
    {
        var review = await context.Reviews
            .Include(r => r.IdLivroNavigation)
            .Include(r => r.IdUtilizadorNavigation)
            .FirstOrDefaultAsync(m => m.IdReview == id);

        return review;
    }

    // POST: api/Reviews
    [HttpPost]
    public async Task<ActionResult<Review>> PostReview([FromBody] ReviewInput input)
    {
        // Create review object
        var review = new Review
        {
            IdUtilizador = input.IdUtilizador,
            IdLivro = input.IdLivro,
            TextoReview = input.TextoReview,
            Rating = input.Rating,
            Status = string.IsNullOrEmpty(input.Status) ? "Pendente" : input.Status,
            DataSubmissao = input.DataSubmissao == default ? DateOnly.FromDateTime(DateTime.Now) : input.DataSubmissao
        };

        // Save review to database to get the id of the new review
        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReview), new { id = review.IdReview }, review);
    }

    // PUT: api/Reviews/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutReview(int id, Review review)
    {
        context.Entry(review).State = EntityState.Modified;

        await context.SaveChangesAsync();
       
        return NoContent();
    }

    // POST: api/Reviews/5/approve
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveReview(int id)
    {
        var review = await context.Reviews.FindAsync(id);

        review.Status = "Aprovada";
        review.DataAprovacao = DateOnly.FromDateTime(DateTime.Now);
        context.Update(review);
        await context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Reviews/5/reject
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectReview(int id)
    {
        var review = await context.Reviews.FindAsync(id);

        review.Status = "Rejeitada";
        review.DataAprovacao = DateOnly.FromDateTime(DateTime.Now);
        context.Update(review);
        await context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Reviews/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await context.Reviews.FindAsync(id);

        context.Reviews.Remove(review);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
