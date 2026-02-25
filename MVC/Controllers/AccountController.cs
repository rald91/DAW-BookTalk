using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MVC.Models;
using MVC.Services;

namespace MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService apiService;

        public AccountController(ApiService _apiService)
        {
            apiService = _apiService;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            // If already authenticated, redirect to Home
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email,Password")] Utilizador utilizador)
        {
            // Remove validation errors for fields that we are not using
            ModelState.Remove(nameof(Utilizador.IdUtilizador));
            ModelState.Remove(nameof(Utilizador.Nome));
            ModelState.Remove(nameof(Utilizador.TipoUtilizador));
            ModelState.Remove(nameof(Utilizador.Reservas));
            ModelState.Remove(nameof(Utilizador.Reviews));

            // Validate only Email and Password
            if (string.IsNullOrWhiteSpace(utilizador.Email))
            {
                ModelState.AddModelError("Email", "O email é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(utilizador.Password))
            {
                ModelState.AddModelError("Password", "A palavra-passe é obrigatória.");
            }

            if (ModelState.IsValid)
            {
                var loginRequest = new { Email = utilizador.Email, Password = utilizador.Password };
                var user = await apiService.PostAsync<Utilizador>("api/account/login", loginRequest);

                if (user != null && user.IdUtilizador > 0)
                {
                    // Save UserId in session
                    HttpContext.Session.SetInt32("UserId", user.IdUtilizador);
                    HttpContext.Session.SetString("UserName", user.Nome);
                    HttpContext.Session.SetString("UserType", user.TipoUtilizador);

                    // Log app access for readers (Leitor)
                    if (user.TipoUtilizador == "Leitor")
                    {
                        await apiService.PostActionAsync("api/home/accesslog", new { UserId = user.IdUtilizador, Event = "start" });
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email ou palavra-passe incorretos.");
                }
            }

            return View(utilizador);
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                await apiService.PostActionAsync("api/home/accesslog", new { UserId = userId.Value, Event = "end" });
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
