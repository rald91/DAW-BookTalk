using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EditorasController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public EditorasController(ApplicationDbContext _context)
    {
        context = _context;
    }

    // GET: api/Editoras
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Editora>>> GetEditoras()
    {
        return await context.Editoras.ToListAsync();
    }

    // POST: api/Editoras
    [HttpPost]
    public async Task<ActionResult<Editora>> PostEditora(Editora editora)
    {
        context.Editoras.Add(editora);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEditoras), new { id = editora.IdEditora }, editora);
    }
}
