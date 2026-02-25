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
    public class ReviewsController : Controller
    {
        private readonly ApiService apiService;

        public ReviewsController(ApiService _apiService)
        {
            apiService = _apiService;
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            // Get all reviews from API
            var reviews = await apiService.GetAsync<List<Review>>("api/reviews?status=Pendente");

            // Return the reviews
            return View(reviews ?? new List<Review>());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        { 
            // Get the review from API
            var review = await apiService.GetAsync<Review>($"api/reviews/{id}"); 

            return View(review);
        }

        // GET: Reviews/Create
        public async Task<IActionResult> Create(int? livroId = null)
        {
            // Get the user from API
            var userId = HttpContext.Session.GetInt32("UserId"); 

            // Get the livros from API
            var livros = await apiService.GetAsync<List<Livro>>("api/livros");  

            // Get the utilizadores from API
            var utilizadores = await apiService.GetAsync<List<Utilizador>>($"api/utilizadores");

            // Get the current user
            var currentUser = utilizadores?.FirstOrDefault(u => u.IdUtilizador == userId.Value);

            // Set the livros and utilizadores list in the view data
            ViewData["IdLivro"] = new SelectList(livros ?? new List<Livro>(), "IdLivro", "Titulo", livroId);

            // Set the utilizadores list in the view data
            ViewData["IdUtilizador"] = new SelectList(currentUser != null ? new List<Utilizador> { currentUser } : new List<Utilizador>(), "IdUtilizador", "Nome", userId);

            return View();
        }

        // POST: Reviews/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("IdReview,IdUtilizador,IdLivro,TextoReview,Rating,Status,DataSubmissao,DataAprovacao")] Review review)
        {
            var userId = HttpContext.Session.GetInt32("UserId"); 

            // Set the IdLivro, IdUtilizador, Status and DataSubmissao
            review.IdLivro = review.IdLivro;  
            review.IdUtilizador = userId.Value; 
            review.Status = "Pendente";
            review.DataSubmissao = DateOnly.FromDateTime(DateTime.Now);
            
            // Map the rating from the form
            if (Request.Form.ContainsKey("Rating") && 
                byte.TryParse(Request.Form["Rating"].ToString(), out byte ratingByte))
            {
                review.Rating = ratingByte;
            }
            else
            {
                review.Rating = null;
            }

            ModelState.Remove("Rating");
            ModelState.Remove("IdUtilizador");
            ModelState.Remove("Status");
            ModelState.Remove("DataSubmissao");
            ModelState.Remove("IdLivroNavigation");
            ModelState.Remove("IdUtilizadorNavigation");

            if (ModelState.IsValid)
            {
                // Create the review object
                var reviewInput = new
                {
                    IdUtilizador = review.IdUtilizador,
                    IdLivro = review.IdLivro,
                    TextoReview = review.TextoReview,
                    Rating = review.Rating,
                    Status = review.Status,
                    DataSubmissao = review.DataSubmissao
                };

                // Create the review in the database
                var created = await apiService.PostAsync<Review>("api/reviews", reviewInput);

                // Check if the review was created
                if (created != null)
                {
                    // Add success message
                    TempData["Success"] = "A sua review foi submetida com sucesso e está pendente de aprovação.";
                    // Redirect to the livro details page
                    return RedirectToAction("Details", "Livros", new { id = review.IdLivro });
                }
                else
                {
                    // Add error message
                    TempData["Error"] = "Ocorreu um erro ao guardar a review.";
                    return RedirectToAction("Details", "Livros", new { id = review.IdLivro });
                }
            }

            // If it reaches here then there was an error

            // Redirect to the details page with error message
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["Error"] = "Ocorreu um erro ao submeter a sua review: " + (string.IsNullOrEmpty(errors) ? "Por favor, verifique os dados introduzidos." : errors);

            // Redirect to the details page
            return RedirectToAction("Details", "Livros", new { id = review.IdLivro });
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        { 
            // Get the review from API
            var review = await apiService.GetAsync<Review>($"api/reviews/{id}"); 

            // Get the livros from API
            var livros = await apiService.GetAsync<List<Livro>>("api/livros");

            // Get the utilizadores from API
            var utilizadores = await apiService.GetAsync<List<Utilizador>>("api/utilizadores");

            // Set the livros and utilizadores list in the view data
            ViewData["IdLivro"] = new SelectList(livros ?? new List<Livro>(), "IdLivro", "Titulo", review.IdLivro);
            ViewData["IdUtilizador"] = new SelectList(utilizadores ?? new List<Utilizador>(), "IdUtilizador", "Email", review.IdUtilizador);

            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("IdReview,IdUtilizador,IdLivro,TextoReview,Rating,Status,DataSubmissao,DataAprovacao")] Review review)
        {
            var userId = HttpContext.Session.GetInt32("UserId"); 

            // Set the Status, DataSubmissao and DataAprovacao
            review.Status = "Pendente";
            review.DataSubmissao = DateOnly.FromDateTime(DateTime.Now);
            review.DataAprovacao = null;
            
            // Map the rating from the form
            if (Request.Form.ContainsKey("Rating") && 
                byte.TryParse(Request.Form["Rating"].ToString(), out byte ratingByte))
            {
                review.Rating = ratingByte;
            }
            else
            {
                review.Rating = null;
            }

            ModelState.Remove("Rating");
            ModelState.Remove("Status");
            ModelState.Remove("DataSubmissao");
            ModelState.Remove("DataAprovacao");
            ModelState.Remove("IdLivroNavigation");
            ModelState.Remove("IdUtilizadorNavigation");

            if (ModelState.IsValid)
            {
                // Update the review in the database
                var updated = await apiService.PutAsync($"api/reviews/{id}", review);

                // Check if the review was updated
                if (updated)
                {
                    // Add success message
                    TempData["Success"] = "A sua avaliação foi atualizada com sucesso e está pendente de aprovação.";
                    // Redirect to the livro details page
                    return RedirectToAction("Details", "Livros", new { id = review.IdLivro });
                }
                else
                {
                    // Add error message
                    TempData["Error"] = "Ocorreu um erro ao guardar as alterações.";
                    return RedirectToAction("Details", "Livros", new { id = review.IdLivro });
                }
            }

            // If it reaches here then there was an error

            // Redirect to the details page with error message
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["Error"] = "Ocorreu um erro ao atualizar a sua review: " + (string.IsNullOrEmpty(errors) ? "Por favor, verifique os dados introduzidos." : errors);

            // Redirect to the details page
            return RedirectToAction("Details", "Livros", new { id = review.IdLivro });
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        { 
            // Get the review from API
            var review = await apiService.GetAsync<Review>($"api/reviews/{id}"); 

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Delete the review in the database
            var deleted = await apiService.DeleteAsync($"api/reviews/{id}"); 
            return RedirectToAction(nameof(Index)); 
        }

        // POST: Reviews/Approve/5
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        { 
            var success = await apiService.PostActionAsync($"api/reviews/{id}/approve");
            if (success)
            {
                TempData["Success"] = "Review aprovada com sucesso.";
            }
            else
            {
                TempData["Error"] = "Ocorreu um erro ao aprovar a review.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Reviews/Reject/5
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var success = await apiService.PostActionAsync($"api/reviews/{id}/reject");
            if (success)
            {
                TempData["Success"] = "Review rejeitada com sucesso.";
            }
            else
            {
                TempData["Error"] = "Ocorreu um erro ao rejeitar a review.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
