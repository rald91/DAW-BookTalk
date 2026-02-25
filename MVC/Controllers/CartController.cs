using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using MVC.Models;
using MVC.Services;

namespace MVC.Controllers
{
    public class CartController : Controller
    {
        private const string SESSION_KEY = "ServerCart";
        private readonly ApiService apiService;
        private readonly Microsoft.Extensions.Logging.ILogger<CartController> _logger;

        public CartController(ApiService _apiService, Microsoft.Extensions.Logging.ILogger<CartController> logger)
        {
            apiService = _apiService;
            _logger = logger;
        }

        private List<CartItem> LoadCart()
        {
            var raw = HttpContext.Session.GetString(SESSION_KEY);
            if (string.IsNullOrEmpty(raw))
            {
                _logger.LogInformation("LoadCart: session key '{key}' empty", SESSION_KEY);
                return new List<CartItem>();
            }
            try
            {
                // stored as simple DTOs with date strings to avoid DateOnly serialization issues
                _logger.LogInformation("LoadCart: raw session length={len}", raw?.Length ?? 0);
                var arr = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(raw!);
                if (arr == null) return new List<CartItem>();
                var list = new List<CartItem>();
                foreach (var d in arr)
                {
                    if (!d.TryGetValue("LivroId", out var idStr)) continue;
                    if (!int.TryParse(idStr, out var id)) continue;
                    d.TryGetValue("LivroText", out var text);
                    d.TryGetValue("DataInicio", out var s);
                    d.TryGetValue("DataFim", out var e);
                    DateOnly sd, ed;
                    if (!DateOnly.TryParse(s, out sd)) sd = DateOnly.FromDateTime(DateTime.Now);
                    if (!DateOnly.TryParse(e, out ed)) ed = sd;
                    list.Add(new CartItem { LivroId = id, LivroText = text ?? string.Empty, DataInicio = sd, DataFim = ed });
                }
                return list;
            }
            catch
            {
                _logger.LogWarning("LoadCart: failed to deserialize session data");
                return new List<CartItem>();
            }
        }

        private void SaveCart(List<CartItem> cart)
        {
            // convert to DTO with date strings
            var arr = cart.Select(c => new Dictionary<string, string>
            {
                ["LivroId"] = c.LivroId.ToString(),
                ["LivroText"] = c.LivroText ?? string.Empty,
                ["DataInicio"] = c.DataInicio.ToString("yyyy-MM-dd"),
                ["DataFim"] = c.DataFim.ToString("yyyy-MM-dd")
            }).ToList();
            var raw = JsonSerializer.Serialize(arr);
            HttpContext.Session.SetString(SESSION_KEY, raw);
            _logger.LogInformation("SaveCart: wrote {count} items to session (raw len={len})", arr.Count, raw.Length);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int livroId, string dataInicio, string dataFim)
        {
            _logger.LogInformation("Cart/Add called with livroId={livroId}, dataInicio={dataInicio}, dataFim={dataFim}", livroId, dataInicio, dataFim);
            var livro = await apiService.GetAsync<Livro>($"api/livros/{livroId}");
            if (livro == null) return NotFound();

            // parse dates
            if (!DateOnly.TryParse(dataInicio, out var sd)) sd = DateOnly.FromDateTime(DateTime.Now);
            if (!DateOnly.TryParse(dataFim, out var ed)) ed = sd;

            var cart = LoadCart();
            cart.Add(new CartItem { LivroId = livroId, LivroText = livro.Titulo ?? string.Empty, DataInicio = sd, DataFim = ed });
            SaveCart(cart);
            TempData["Success"] = "Item adicionado ao carrinho.";
            _logger.LogInformation("Item adicionado ao carrinho: livroId={livroId}", livroId);
            return RedirectToAction("Index", "Home");
        }

        // GET: show confirmation page for removing an item from the cart
        [HttpGet]
        public IActionResult Remove(int? index)
        {
            if (!index.HasValue) return RedirectToAction("Index");
            var cart = LoadCart();
            if (index < 0 || index >= cart.Count) return RedirectToAction("Index");
            var vm = new Models.CartRemoveViewModel { Index = index.Value, Item = cart[index.Value] };
            return View("Remove", vm);
        }

        // POST: actually remove the item
        [HttpPost]
        public IActionResult RemoveConfirmed(int index)
        {
            var cart = LoadCart();
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = LoadCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            _logger.LogInformation("Cart/Checkout called by user {user}", HttpContext.Session.GetString("UserName") ?? "(anonymous)");
            var cart = LoadCart();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Carrinho vazio.";
                return RedirectToAction("Index", "Home");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            if (!int.TryParse(userIdStr, out var userId)) return RedirectToAction("Login", "Account");

            var successCount = 0;
            foreach (var it in cart)
            {
                var livro = await apiService.GetAsync<Livro>($"api/livros/{it.LivroId}");
                if (livro != null)
                {
                    var reservaInput = new
                    {
                        IdUtilizador = userId,
                        DataInicio = it.DataInicio,
                        DataFim = it.DataFim,
                        Confirmada = false,
                        LivroIds = new List<int> { livro.IdLivro }
                    };

                    var created = await apiService.PostAsync<Reserva>("api/reservas", reservaInput);
                    if (created != null)
                    {
                        successCount++;
                    }
                }
            }

            if (successCount > 0)
            {
                SaveCart(new List<CartItem>());
                TempData["Success"] = $"{successCount} reserva(s) criada(s) com sucesso.";
                _logger.LogInformation("Checkout created {count} reservas for user {user}", successCount, HttpContext.Session.GetString("UserName") ?? "(anonymous)");
                return RedirectToAction("CheckoutSuccess");
            }
            else
            {
                TempData["Error"] = "Erro ao criar reservas.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: show confirmation page after successful checkout
        [HttpGet]
        public IActionResult CheckoutSuccess()
        {
            _logger.LogInformation("Showing CheckoutSuccess to user {user}", HttpContext.Session.GetString("UserName") ?? "(anonymous)");
            return View();
        }
    }
}
