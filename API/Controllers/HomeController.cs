using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public HomeController(ApplicationDbContext _context)
    {
        context = _context;
    }

    // GET: api/Home/statistics
    [HttpGet("statistics")]
    public async Task<ActionResult<StatisticsResponse>> GetStatistics()
    {
        // Top clicked books
        var topClicked = await context.Livros
            .Include(l => l.Autores)
            .OrderByDescending(l => l.Clicks)
            .Take(10)
            .Select(l => new StatsItem
            {
                IdLivro = l.IdLivro,
                Titulo = l.Titulo,
                Autor = string.Join(", ", l.Autores.Select(a => a.Name)),
                Value = l.Clicks
            })
            .ToListAsync();

        // Top requested books (from reservations)
        var reservations = await context.Reservas
            .Include(r => r.IdLivros)
                .ThenInclude(l => l.Autores)
            .ToListAsync();

        var topRequested = reservations
            .SelectMany(r => r.IdLivros)
            .GroupBy(l => new { l.IdLivro, l.Titulo, Autores = l.Autores.Select(a => a.Name).ToList() })
            .Select(g => new StatsItem
            {
                IdLivro = g.Key.IdLivro,
                Titulo = g.Key.Titulo,
                Autor = string.Join(", ", g.Key.Autores),
                Value = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToList();

        // Access logs by time of day (count only 'start' events from users with TipoUtilizador == "Leitor")
        var hours = await context.AccessLogs
            .Where(a => a.UserId != null && a.Event == "start")
            .Join(context.Utilizadors.Where(u => u.TipoUtilizador == "Leitor"),
                  log => log.UserId,
                  u => u.IdUtilizador,
                  (log, u) => log)
            .GroupBy(a => a.Timestamp.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .ToListAsync();

        int manha = hours.Where(h => h.Hour >= 6 && h.Hour < 12).Sum(h => h.Count);
        int tarde = hours.Where(h => h.Hour >= 12 && h.Hour < 18).Sum(h => h.Count);
        int noite = hours.Where(h => h.Hour >= 18 && h.Hour < 24).Sum(h => h.Count);

        var timeOfDayCounts = new Dictionary<string, int>
        {
            ["Manhã"] = manha,
            ["Tarde"] = tarde,
            ["Noite"] = noite
        };

        // Calculate total time per user using start/end events
        // First, get all logs ordered by user and then by timestamp
        var logsWithUser = await context.AccessLogs
            .Where(a => a.UserId != null && a.Event != null)
            .OrderBy(a => a.UserId).ThenBy(a => a.Timestamp)
            .ToListAsync();

        // Group logs by user
        var userGroups = logsWithUser.GroupBy(a => a.UserId!.Value).ToList();
        var userTimesDict = new Dictionary<string, string>();

        if (userGroups.Any())
        {
            // Get user information to filter only "Leitores" (Readers)
            var userIds = userGroups.Select(g => g.Key).ToList();
            var users = await context.Utilizadors
                .Where(u => userIds.Contains(u.IdUtilizador))
                .ToDictionaryAsync(u => u.IdUtilizador, u => new { u.Nome, u.TipoUtilizador });

            // For each user, calculate total time
            foreach (var g in userGroups)
            {
                DateTime? start = null;
                double totalSeconds = 0;
                
                // Go through user events in chronological order
                // When finding "start", save the timestamp
                // When finding "end", calculate the difference and add to total
                foreach (var entry in g)
                {
                    if (string.Equals(entry.Event, "start", StringComparison.OrdinalIgnoreCase))
                    {
                        start = entry.Timestamp;
                    }
                    else if (string.Equals(entry.Event, "end", StringComparison.OrdinalIgnoreCase) && start != null)
                    {
                        totalSeconds += (entry.Timestamp - start.Value).TotalSeconds;
                        start = null; // Reset for next start/end pair
                    }
                }

                // Exclude users that are not "Leitores" (e.g., admin/bibliotecário)
                if (!users.ContainsKey(g.Key) || !string.Equals(users[g.Key].TipoUtilizador, "Leitor", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Convert seconds to hours:minutes:seconds format
                var ts = TimeSpan.FromSeconds(totalSeconds);
                var name = users[g.Key].Nome ?? ("User " + g.Key);
                
                // Explicitly skip the "admin" user
                if (string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                userTimesDict[name] = ts.ToString(@"hh\:mm\:ss");
            }
        }

        return Ok(new StatisticsResponse
        {
            TopClicked = topClicked,
            TopRequested = topRequested,
            TimeOfDayCounts = timeOfDayCounts,
            UserTimes = userTimesDict
        });
    }

    // POST: api/Home/accesslog
    [HttpPost("accesslog")]
    public async Task<IActionResult> LogAccess([FromBody] AccessLog? log)
    {
        var accessLog = log ?? new AccessLog();
        accessLog.Timestamp = DateTime.Now;
        context.AccessLogs.Add(accessLog);
        await context.SaveChangesAsync();

        return NoContent();
    }
}

public class StatisticsResponse
{
    public List<StatsItem> TopClicked { get; set; } = new List<StatsItem>();
    public List<StatsItem> TopRequested { get; set; } = new List<StatsItem>();
    public Dictionary<string, int> TimeOfDayCounts { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, string> UserTimes { get; set; } = new Dictionary<string, string>();
}

public class StatsItem
{
    public int IdLivro { get; set; }
    public string Titulo { get; set; } = "";
    public string Autor { get; set; } = "";
    public int Value { get; set; }
}
