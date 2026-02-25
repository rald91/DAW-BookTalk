using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using MVC.Models;
using MVC.Services;

namespace MVC.Controllers
{
    public class ReservasController : Controller
    {
        private readonly ApiService apiService;

        public ReservasController(ApiService _apiService)
        {
            apiService = _apiService;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var userType = HttpContext.Session.GetString("UserType");

            List<Reserva>? reservas = null;

            if (userType == "Bibliotecário")
            {
                reservas = await apiService.GetAsync<List<Reserva>>("api/reservas");
            }
            else
            { 
                reservas = await apiService.GetAsync<List<Reserva>>($"api/reservas?userId={userId.Value}");
 
            }

            return View(reservas ?? new List<Reserva>());
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var reserva = await apiService.GetAsync<Reserva>($"api/reservas/{id}"); 

            return View(reserva);
        }

        // GET: Reservas/Create
        public async Task<IActionResult> Create(int? livroId = null)
        { 
            // Get all books from API
            var livros = await apiService.GetAsync<List<Livro>>("api/livros");

            // Get active books
            var activeLivros = livros?.Where(l => l.Estado == "ativo").ToList() ?? new List<Livro>();

            // Set the books list in the view data
            ViewData["Livros"] = new SelectList(activeLivros, "IdLivro", "Titulo", livroId);
            return View();
        }

        // POST: Reservas/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("IdReserva,DataInicio,DataFim")] Reserva reserva, int livroId)
        {
            var userId = GetCurrentUserId(); 

            // Get the book from API
            var livro = await apiService.GetAsync<Livro>($"api/livros/{livroId}");

            // Set the end date to 6 days after the start date
            if (!reserva.DataFim.HasValue)
            {
                reserva.DataFim = reserva.DataInicio.AddDays(6);
            }

            // Get the start and end dates
            var start = reserva.DataInicio;
            var end = reserva.DataFim.Value;

            // Check if the end date is before the start date
            if (end < start)
            {
                ModelState.AddModelError(string.Empty, "The end date cannot be before the start date.");
            }

            // Check if reservation duration does not exceed maximum allowed (14 days)
            var maxDays = 14;
            var days = end.DayNumber - start.DayNumber + 1;
            if (days > maxDays)
            {
                ModelState.AddModelError(string.Empty, $"A duração máxima de reserva é de {maxDays} dias.");
            }

            // Check availability via API
            var allReservas = await apiService.GetAsync<List<Reserva>>("api/reservas");
            if (allReservas != null)
            {
                // Check if the book is already reserved for the selected period
                var overlapping = allReservas
                    .Where(r => r.IdLivros.Any(l => l.IdLivro == livroId))
                    .Any(r =>
                    {
                        // Check if there is date overlap
                        var reservaFim = r.DataFim ?? r.DataInicio;
                        var noOverlap = end < r.DataInicio || start > reservaFim;
                        var hasOverlap = !noOverlap;
                        return hasOverlap;
                    });

                if (overlapping)
                {
                    ModelState.AddModelError(string.Empty, "O livro já está reservado nesse período.");
                }
            }

            // Check if the model is valid 
            if (ModelState.IsValid)
            {
                // Create the reservation input
                var reservaInput = new
                {
                    IdUtilizador = userId.Value,
                    DataInicio = reserva.DataInicio,
                    DataFim = reserva.DataFim,
                    Confirmada = false,
                    LivroIds = new List<int> { livro.IdLivro }
                };

                // Create the reservation in the database
                var created = await apiService.PostAsync<Reserva>("api/reservas", reservaInput);
                if (created != null)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // Get all books from API
            var livros = await apiService.GetAsync<List<Livro>>("api/livros");
            // Get active books
            var activeLivros = livros?.Where(l => l.Estado == "ativo").ToList() ?? new List<Livro>();

            // Set the books list in the view data
            ViewData["Livros"] = new SelectList(activeLivros, "IdLivro", "Titulo", livroId);

            return View(reserva);
        }

        // POST: Reservas/Checkout
        [HttpPost]
        [Route("Reservas/Checkout")]
        public async Task<IActionResult> Checkout([FromBody] List<MVC.Models.ReservaDto> items)
        {
            var userId = GetCurrentUserId(); 

            if (items == null || items.Count == 0)
                return BadRequest(new { message = "Nenhuma reserva enviada." });

            if (items.Count > 3)
                return BadRequest(new { message = "Máximo de 3 reservas por checkout." });

            // Get all reservations from API
            var allReservas = await apiService.GetAsync<List<Reserva>>("api/reservas");

            // Create a list of created reservations
            var created = new List<int>();
             
            foreach (var it in items)
            {
                // Get the book from API
                var livro = await apiService.GetAsync<Livro>($"api/livros/{it.LivroId}");

                // Check if the book is valid
                if (livro == null || livro.Estado != "ativo")
                {
                    return BadRequest(new { message = $"Livro {it.LivroId} inválido ou não disponível." });
                }

                // Get the start and end dates
                var start = it.DataInicio;
                var end = it.DataFim;

                // Check if the end date is before the start date
                if (end < start)
                    return BadRequest(new { message = "Data de fim anterior à data de início." });

                // Check if reservation duration does not exceed maximum allowed (14 days)
                var days = end.DayNumber - start.DayNumber + 1;
                if (days > 14)
                    return BadRequest(new { message = "Duração máxima de 14 dias." });

                if (allReservas != null)
                {
                    // Check if the book is already reserved for the selected period
                    var overlapping = allReservas
                        .Where(r => r.IdLivros.Any(l => l.IdLivro == it.LivroId))
                        .Any(r =>
                        {
                            // Check if there is date overlap
                            var reservaFim = r.DataFim ?? r.DataInicio;
                            var noOverlap = end < r.DataInicio || start > reservaFim;
                            var hasOverlap = !noOverlap;
                            return hasOverlap;
                        });

                    if (overlapping)
                    {
                        return BadRequest(new { message = $"Livro '{livro.Titulo}' já reservado nesse período." });
                    }
                }

                // Create the reservation input
                var reservaInput = new
                {
                    IdUtilizador = userId.Value,
                    DataInicio = start,
                    DataFim = end,
                    Confirmada = false,
                    LivroIds = new List<int> { livro.IdLivro }
                };

                // Create the reservation in the database
                var createdReserva = await apiService.PostAsync<Reserva>("api/reservas", reservaInput);
                if (createdReserva != null)
                {
                    // Add the created reservation to the list
                    created.Add(createdReserva.IdReserva);
                }
            }

            // Return the created reservations
            return Ok(new { created });
        }

        // POST: Reservas/Confirm/5
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        { 
            // Confirm the reservation in the database
            var success = await apiService.PostActionAsync($"api/reservas/{id}/confirm");
            if (success)
            {
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {  
            // Get the reservation from API
            var reserva = await apiService.GetAsync<Reserva>($"api/reservas/{id}"); 
            
            // Get the utilizadores from API
            var utilizadores = await apiService.GetAsync<List<Utilizador>>("api/utilizadores");

            // Set the utilizadores list in the view data
            ViewData["IdUtilizador"] = new SelectList(utilizadores ?? new List<Utilizador>(), "IdUtilizador", "Email", reserva.IdUtilizador);

            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("IdReserva,IdUtilizador,DataInicio,DataFim")] Reserva reserva)
        { 
            // Update the reservation in the database
            if (ModelState.IsValid)
            {
                var updated = await apiService.PutAsync($"api/reservas/{id}", reserva);
                if (updated)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            
            // Get the utilizadores from API
            var utilizadores = await apiService.GetAsync<List<Utilizador>>("api/utilizadores");
            ViewData["IdUtilizador"] = new SelectList(utilizadores ?? new List<Utilizador>(), "IdUtilizador", "Email", reserva.IdUtilizador);

            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {  
            // Get the reservation from API
            var reserva = await apiService.GetAsync<Reserva>($"api/reservas/{id}"); 

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Delete the reservation in the database
            var deleted = await apiService.DeleteAsync($"api/reservas/{id}");

            // Check if the reservation was deleted
            if (deleted)
            {
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
    }
}
