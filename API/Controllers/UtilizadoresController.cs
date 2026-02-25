using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UtilizadoresController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public UtilizadoresController(ApplicationDbContext _context)
    {
        context = _context;
    }

    // GET: api/Utilizadores
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Utilizador>>> GetUtilizadores()
    {
        return await context.Utilizadors.ToListAsync();
    }

    // GET: api/Utilizadores/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Utilizador>> GetUtilizador(int id)
    {
        var utilizador = await context.Utilizadors.FindAsync(id);
 
        return utilizador;
    }

    // POST: api/Utilizadores
    [HttpPost]
    public async Task<ActionResult<Utilizador>> PostUtilizador(Utilizador utilizador)
    {
        context.Utilizadors.Add(utilizador);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUtilizador), new { id = utilizador.IdUtilizador }, utilizador);
    }

    // PUT: api/Utilizadores/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUtilizador(int id, Utilizador utilizador)
    { 
        context.Entry(utilizador).State = EntityState.Modified;
 
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    // DELETE: api/Utilizadores/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUtilizador(int id)
    {
        var utilizador = await context.Utilizadors.FindAsync(id);
        
        context.Utilizadors.Remove(utilizador);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
