using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers;

public class ReservaInput
{
    public int IdUtilizador { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public bool Confirmada { get; set; }
    public List<int> LivroIds { get; set; } = new List<int>();
}

[Route("api/[controller]")]
[ApiController]
public class ReservasController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public ReservasController(ApplicationDbContext _context)
    {
        context = _context;
    }

    // GET: api/Reservas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reserva>>> GetReservas([FromQuery] int? userId)
    {
        var query = context.Reservas
            .Include(r => r.IdUtilizadorNavigation)
            .Include(r => r.IdLivros)
            .AsQueryable();

        // Filter by user id
        if (userId.HasValue)
        {
            query = query.Where(r => r.IdUtilizador == userId.Value);
        } 

        // Order by data de inicio
        return await query.OrderByDescending(r => r.DataInicio).ToListAsync();
    }

    // GET: api/Reservas/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Reserva>> GetReserva(int id)
    {
        var reserva = await context.Reservas
            .Include(r => r.IdUtilizadorNavigation)
            .Include(r => r.IdLivros)
            .FirstOrDefaultAsync(m => m.IdReserva == id);

        return reserva;
    }

    // POST: api/Reservas
    [HttpPost]
    public async Task<ActionResult<Reserva>> PostReserva([FromBody] ReservaInput input)
    {
        // Create reserva object
        var reserva = new Reserva
        {
            IdUtilizador = input.IdUtilizador,
            DataInicio = input.DataInicio,
            DataFim = input.DataFim,
            Confirmada = input.Confirmada
        };

        // Save reserva to database to get the id of the new reserva
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync(); 
        
        // Add livros to reserva
        if (input.LivroIds != null && input.LivroIds.Any())
        {
            var livros = await context.Livros
                .Where(l => input.LivroIds.Contains(l.IdLivro))
                .ToListAsync();
            
            foreach (var livro in livros)
            {
                reserva.IdLivros.Add(livro);
            } 
        }

        // Save reserva with relations to database
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReserva), new { id = reserva.IdReserva }, reserva);
    }

    // PUT: api/Reservas/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutReserva(int id, Reserva reserva)
    {
        context.Entry(reserva).State = EntityState.Modified;

        await context.SaveChangesAsync();
        
        return NoContent();
    }

    // POST: api/Reservas/5/confirm
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmReserva(int id)
    {
        var reserva = await context.Reservas.FindAsync(id);

        reserva.Confirmada = true;
        context.Update(reserva);
        await context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Reservas/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReserva(int id)
    {
        var reserva = await context.Reservas.FindAsync(id);

        context.Reservas.Remove(reserva);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
