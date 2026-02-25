using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public AccountController(ApplicationDbContext _context)
    {
        context = _context;
    }

    // POST: api/Account/login
    [HttpPost("login")]
    public async Task<ActionResult<Utilizador>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email e palavra-passe são obrigatórios." });
        }

        var user = await context.Utilizadors
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "Email ou palavra-passe incorretos." });
        }

        // Não retornar a password
        user.Password = "";
        return Ok(user);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
