using MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using MVC.Services;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService apiService;

        public HomeController(ApiService _apiService)
        {
            apiService = _apiService;
        }

        public async Task<IActionResult> Index()
        {
            // Check if session is started
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userType = HttpContext.Session.GetString("UserType");

            // For students show active books on the homepage
            var books = await apiService.GetAsync<List<Livro>>("api/livros");
            if (books != null)
            {
                books = books.Where(l => l.Estado == "ativo").ToList();
            }
            else
            {
                books = new List<Livro>();
            }

            if (userType == "Bibliotecário")
            {
                // Get reservations
                var reservations = await apiService.GetAsync<List<Reserva>>("api/reservas");
                if (reservations != null)
                {
                    // Filter reservations for current week
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var dayOfWeek = ((int)DateTime.Today.DayOfWeek + 6) % 7; // Monday = 0
                    var weekStart = today.AddDays(-dayOfWeek);
                    var weekEnd = weekStart.AddDays(6);

                    var reservationsWeek = reservations
                        .Where(r => !(r.DataFim < weekStart || r.DataInicio > weekEnd))
                        .ToList();

                    ViewBag.ReservasWeek = reservationsWeek;
                }

                // Load pending reviews
                var pendingReviews = await apiService.GetAsync<List<Review>>("api/reviews?status=Pendente");
                ViewBag.ReviewsPending = pendingReviews ?? new List<Review>();
            }

            return View(books);
        }

        public async Task<IActionResult> Estatisticas()
        {
            // Get statistics from API
            var statistics = await apiService.GetAsync<StatisticsResponse>("api/home/statistics");
            if (statistics == null)
            {
                return View(new MVC.Models.StatisticsViewModel());
            }

            var model = new MVC.Models.StatisticsViewModel
            {
                TopClicked = statistics.TopClicked.Select(x => new MVC.Models.StatsItem 
                { 
                    IdLivro = x.IdLivro, 
                    Titulo = x.Titulo, 
                    Autor = x.Autor, 
                    Value = x.Value 
                }).ToList(),
                TopRequested = statistics.TopRequested.Select(x => new MVC.Models.StatsItem 
                { 
                    IdLivro = x.IdLivro, 
                    Titulo = x.Titulo, 
                    Autor = x.Autor, 
                    Value = x.Value 
                }).ToList(),
                TimeOfDayCounts = statistics.TimeOfDayCounts
                ,
                UserTimes = statistics.UserTimes.Select(kv => new MVC.Models.UserTimeItem { UserName = kv.Key, Time = kv.Value }).ToList()
            };

            return View(model);
        }

        public IActionResult Sobre()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // DTOs for API responses
    public class StatisticsResponse
    {
        public List<StatsItemResponse> TopClicked { get; set; } = new List<StatsItemResponse>();
        public List<StatsItemResponse> TopRequested { get; set; } = new List<StatsItemResponse>();
        public Dictionary<string, int> TimeOfDayCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, string> UserTimes { get; set; } = new Dictionary<string, string>();
    }

    public class StatsItemResponse
    {
        public int IdLivro { get; set; }
        public string Titulo { get; set; } = "";
        public string Autor { get; set; } = "";
        public int Value { get; set; }
    }
}
