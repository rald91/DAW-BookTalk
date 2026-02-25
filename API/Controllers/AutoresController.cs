using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AutoresController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public AutoresController(ApplicationDbContext _context)
    {
        context = _context;
    }

    // GET: api/Autores
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Autor>>> GetAutores()
    {
        return await context.Autores.ToListAsync();
    }

    // POST: api/Autores
    [HttpPost]
    public async Task<ActionResult<Autor>> PostAutor(Autor autor)
    {
        context.Autores.Add(autor);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAutores), new { id = autor.IdAutor }, autor);
    }
}
