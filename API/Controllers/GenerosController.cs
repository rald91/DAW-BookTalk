using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GenerosController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public GenerosController(ApplicationDbContext _context)
    {
        context = _context;
    }

    // GET: api/Generos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Genero>>> GetGeneros()
    {
        return await context.Generos.ToListAsync();
    }

    // POST: api/Generos
    [HttpPost]
    public async Task<ActionResult<Genero>> PostGenero(Genero genero)
    {
        context.Generos.Add(genero);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGeneros), new { id = genero.IdGenero }, genero);
    }
}
