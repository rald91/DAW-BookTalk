using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LivrosController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public LivrosController(ApplicationDbContext _context)
    {
        context = _context;
    }

    // GET: api/Livros
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Livro>>> GetLivros()
    {
        return await context.Livros
            .Include(l => l.Autores)
            .Include(l => l.Generos)
            .Include(l => l.EditoraNavigation)
            .ToListAsync();
    }

    // GET: api/Livros/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Livro>> GetLivro(int id)
    {
        var livro = await context.Livros
            .Include(l => l.Reviews)
                .ThenInclude(r => r.IdUtilizadorNavigation)
            .Include(l => l.IdReservas)
            .Include(l => l.Autores)
            .Include(l => l.Generos)
            .Include(l => l.EditoraNavigation)
            .FirstOrDefaultAsync(m => m.IdLivro == id);

        if (livro == null)
        {
            return NotFound();
        }

        // Increment click counter 
        livro.Clicks += 1;
        context.Update(livro);
        await context.SaveChangesAsync(); 

        return livro;
    }

    // POST: api/Livros
    [HttpPost]
    public async Task<ActionResult<Livro>> PostLivro(Livro livro)
    {
        // Validate and normalize IdEditora
        if (livro.IdEditora.HasValue && livro.IdEditora.Value <= 0)
        {
            livro.IdEditora = null;
        }
        else if (livro.IdEditora.HasValue && livro.IdEditora.Value > 0)
        {
            var editoraExists = await context.Editoras.AnyAsync(e => e.IdEditora == livro.IdEditora.Value);
            if (!editoraExists)
            {
                return BadRequest(new { message = $"Editora com ID {livro.IdEditora.Value} não existe." });
            }
        }

        // Handle many-to-many relationships
        var autoresIds = livro.Autores?.Select(a => a.IdAutor).ToList() ?? new List<int>();
        var generosIds = livro.Generos?.Select(g => g.IdGenero).ToList() ?? new List<int>();
        
        if (livro.Autores != null) livro.Autores.Clear();
        if (livro.Generos != null) livro.Generos.Clear();

        context.Livros.Add(livro);
        await context.SaveChangesAsync();

        // Add autores
        if (autoresIds.Any() && livro.Autores != null)
        {
            var autores = await context.Autores.Where(a => autoresIds.Contains(a.IdAutor)).ToListAsync();
            foreach (var autor in autores)
            {
                livro.Autores.Add(autor);
            }
        }

        // Add generos
        if (generosIds.Any() && livro.Generos != null)
        {
            var generos = await context.Generos.Where(g => generosIds.Contains(g.IdGenero)).ToListAsync();
            foreach (var genero in generos)
            {
                livro.Generos.Add(genero);
            }
        }

        await context.SaveChangesAsync();

        // Reload with relationships
        await context.Entry(livro).Collection(l => l.Autores).LoadAsync();
        await context.Entry(livro).Collection(l => l.Generos).LoadAsync();

        return CreatedAtAction(nameof(GetLivro), new { id = livro.IdLivro }, livro);
    }

    // PUT: api/Livros/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutLivro(int id, Livro livro)
    {
        var existingLivro = await context.Livros
            .Include(l => l.Autores)
            .Include(l => l.Generos)
            .FirstOrDefaultAsync(l => l.IdLivro == id);

        // Validate and normalize IdEditora
        int? editoraIdToSet = null;
        if (livro.IdEditora.HasValue && livro.IdEditora.Value > 0)
        {
            var editoraExists = await context.Editoras.AnyAsync(e => e.IdEditora == livro.IdEditora.Value);
            if (!editoraExists)
            {
                return BadRequest(new { message = $"Editora com ID {livro.IdEditora.Value} não existe." });
            }
            editoraIdToSet = livro.IdEditora.Value;
        }

        // Update basic properties
        existingLivro.Titulo = livro.Titulo;
        existingLivro.ISBN = livro.ISBN;
        existingLivro.Idioma = livro.Idioma;
        existingLivro.AnoPublicacao = livro.AnoPublicacao;
        existingLivro.Sinopse = livro.Sinopse;
        existingLivro.Estado = livro.Estado;
        existingLivro.CapaUrl = livro.CapaUrl;
        existingLivro.IdEditora = editoraIdToSet;

        // Handle many-to-many relationships
        var autoresIds = livro.Autores?.Select(a => a.IdAutor).ToList() ?? new List<int>();
        var generosIds = livro.Generos?.Select(g => g.IdGenero).ToList() ?? new List<int>();

        existingLivro.Autores.Clear();
        existingLivro.Generos.Clear();

        // Add autores
        if (autoresIds.Any())
        {
            var autores = await context.Autores.Where(a => autoresIds.Contains(a.IdAutor)).ToListAsync();
            foreach (var autor in autores)
            {
                existingLivro.Autores.Add(autor);
            }
        }

        // Add geneeros
        if (generosIds.Any())
        {
            var generos = await context.Generos.Where(g => generosIds.Contains(g.IdGenero)).ToListAsync();
            foreach (var genero in generos)
            {
                existingLivro.Generos.Add(genero);
            }
        }
 
        await context.SaveChangesAsync();
 
        return NoContent();
    }

    // DELETE: api/Livros/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLivro(int id)
    {
        var livro = await context.Livros.FindAsync(id);

        context.Livros.Remove(livro);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
