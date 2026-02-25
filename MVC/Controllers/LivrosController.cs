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
    public class LivrosController : Controller
    {
        private readonly ApiService apiService;

        public LivrosController(ApiService _apiService)
        {
            apiService = _apiService;
        }

        private bool IsBibliotecario()
        {
            var userType = HttpContext.Session.GetString("UserType");
            return userType == "Bibliotecário" ? true : false;
        }

        // GET: Livros
        public async Task<IActionResult> Index(string q)
        {
            // Get all books from API
            var livros = await apiService.GetAsync<List<Livro>>("api/livros") ?? new List<Livro>();

            // If a search query is provided, filter by title or author name (case-insensitive)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var qLower = q.Trim().ToLowerInvariant();
                livros = livros.Where(l =>
                    (!string.IsNullOrEmpty(l.Titulo) && l.Titulo.ToLowerInvariant().Contains(qLower)) ||
                    (l.Autores != null && l.Autores.Any(a => !string.IsNullOrEmpty(a.Name) && a.Name.ToLowerInvariant().Contains(qLower)))
                ).ToList();
            }

            return View(livros);
        }

        // GET: Livros/Details/5
        public async Task<IActionResult> Details(int? id)
        { 
            var livro = await apiService.GetAsync<Livro>($"api/livros/{id}"); 

            // Get reviews for this book
            var reviews = await apiService.GetAsync<List<Review>>($"api/reviews?livroId={id}");
            if (reviews != null)
            {
                livro.Reviews = reviews;
            }

            // Get reservations for this book
            var reservas = await apiService.GetAsync<List<Reserva>>("api/reservas");
            if (reservas != null)
            {
                livro.IdReservas = reservas.Where(r => r.IdLivros.Any(l => l.IdLivro == id)).ToList();
            }

            // Calculate average rating of approved reviews
            var approvedReviews = livro.Reviews.Where(r => r.Status == "Aprovada" && r.Rating.HasValue).ToList();
            var averageRating = approvedReviews.Any()
                ? approvedReviews.Average(r => r.Rating!.Value)
                : (double?)null;

            // Check availability
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var isAvailable = livro.Estado == "ativo" && 
                !livro.IdReservas.Any(r => 
                    r.DataInicio <= hoje && 
                    (r.DataFim ?? r.DataInicio) >= hoje);

            // Check if the current user has a review for this book
            var userId = HttpContext.Session.GetInt32("UserId");
            Review? userReview = null;
            if (userId.HasValue && reviews != null)
            {
                userReview = reviews.FirstOrDefault(r => r.IdUtilizador == userId.Value);
            }

            ViewData["AverageRating"] = averageRating;
            ViewData["IsAvailable"] = isAvailable;
            ViewData["ApprovedReviews"] = approvedReviews;
            ViewData["UserReview"] = userReview;

            return View(livro);
        }

        // GET: Livros/Create
        public async Task<IActionResult> Create()
        { 
            // Get editoras from API
            var editoras = await apiService.GetAsync<List<Editora>>("api/editoras");
            ViewData["Editoras"] = new SelectList(editoras ?? new List<Editora>(), "IdEditora", "Name");
            
            return View(new LivroCreateViewModel());
        }

        // POST: Livros/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("IdLivro,Titulo,AnoPublicacao,Sinopse,Estado,CapaUrl,ISBN,Idioma,IdEditora,AuthorNamesString,GenreNamesString,NovaEditoraNome")] LivroCreateViewModel model)
        { 
            // Convert the date from the form to a DateOnly object
            model.AnoPublicacao = ParseAnoPublicacaoFromForm();
 
            if (ModelState.IsValid)
            {
                // Create a new book object
                var livro = new Livro
                {
                    Titulo = model.Titulo,
                    ISBN = model.ISBN,
                    Idioma = model.Idioma,
                    AnoPublicacao = model.AnoPublicacao,
                    Sinopse = model.Sinopse,
                    Estado = model.Estado,
                    CapaUrl = model.CapaUrl,
                    IdEditora = model.IdEditora
                };

                // Add authors if they exist
                if (!string.IsNullOrWhiteSpace(model.AuthorNamesString))
                {
                    string[] nomes = model.AuthorNamesString.Split(',');
                    
                    foreach (string nome in nomes)
                    {
                        string nomeLimpo = nome.Trim();
                        if (!string.IsNullOrWhiteSpace(nomeLimpo))
                        {
                            var autor = await GetOrCreateAutor(nomeLimpo);
                            if (autor != null)
                            {
                                livro.Autores.Add(autor);
                            }
                        }
                    }
                }

                // Add genres if they exist
                if (!string.IsNullOrWhiteSpace(model.GenreNamesString))
                {
                    string[] nomes = model.GenreNamesString.Split(',');
                    
                    foreach (string nome in nomes)
                    {
                        string nomeLimpo = nome.Trim();
                        if (!string.IsNullOrWhiteSpace(nomeLimpo))
                        {
                            var genero = await GetOrCreateGenero(nomeLimpo);
                            if (genero != null)
                            {
                                livro.Generos.Add(genero);
                            }
                        }
                    }
                }

                // Set editora id if it exists
                if (model.IdEditora.HasValue)
                {
                    livro.IdEditora = model.IdEditora.Value;
                }
                // Create a new editora if it doesn't exist
                else if (!string.IsNullOrWhiteSpace(model.NovaEditoraNome))
                {
                    var editora = await GetOrCreateEditora(model.NovaEditoraNome);
                    if (editora != null)
                    {
                        livro.IdEditora = editora.IdEditora;
                    }
                }

                // Create the book in the database
                var created = await apiService.PostAsync<Livro>("api/livros", livro);
                if (created != null)
                {
                    TempData["Success"] = "Livro adicionado ao catálogo.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Get editoras from API and set the editoras list in the view data
            var editoras = await apiService.GetAsync<List<Editora>>("api/editoras");
            ViewData["Editoras"] = new SelectList(editoras ?? new List<Editora>(), "IdEditora", "Name", model.IdEditora);

            return View(model);
        }

        // GET: Livros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {  
            var livro = await apiService.GetAsync<Livro>($"api/livros/{id}");
            
            // Concatenate the authors and genres to a string
            var autores = livro.Autores?.Any() == true 
                    ? string.Join(", ", livro.Autores.Select(a => a.Name)) 
                    : null;

            var generos = livro.Generos?.Any() == true 
                    ? string.Join(", ", livro.Generos.Select(g => g.Name)) 
                    : null;

            // Create a new model with the book data
            var model = new LivroCreateViewModel
            {
                IdLivro = livro.IdLivro,
                Titulo = livro.Titulo,
                ISBN = livro.ISBN,
                Idioma = livro.Idioma,
                AnoPublicacao = livro.AnoPublicacao,
                Sinopse = livro.Sinopse,
                Estado = livro.Estado,
                CapaUrl = livro.CapaUrl,
                IdEditora = livro.IdEditora,
                AuthorNamesString = autores,
                GenreNamesString = generos,
            };

            // Get editoras from API and set the editoras list in the view data 
            var editoras = await apiService.GetAsync<List<Editora>>("api/editoras");
            ViewData["Editoras"] = new SelectList(editoras ?? new List<Editora>(), "IdEditora", "Name", livro.IdEditora);

            return View(model);
        }

        // POST: Livros/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("IdLivro,Titulo,AnoPublicacao,Sinopse,Estado,CapaUrl,ISBN,Idioma,IdEditora,AuthorNamesString,GenreNamesString,NovaEditoraNome")] LivroCreateViewModel model)
        {
            // Convert the date from the form to a DateOnly object
            model.AnoPublicacao = ParseAnoPublicacaoFromForm();

            if (ModelState.IsValid)
            {
                // Get the existing book to preserve relationships
                var livro = await apiService.GetAsync<Livro>($"api/livros/{id}");
                if (livro == null)
                {
                    return NotFound();
                }
                
                // Update the book properties
                livro.Titulo = model.Titulo;
                livro.ISBN = model.ISBN;
                livro.Idioma = model.Idioma;
                livro.AnoPublicacao = model.AnoPublicacao;
                livro.Sinopse = model.Sinopse;
                livro.Estado = model.Estado;
                livro.CapaUrl = model.CapaUrl;
                
                // Clear and rebuild autores/generos
                livro.Autores.Clear();
                livro.Generos.Clear();
                
                // Add authors 
                if (!string.IsNullOrWhiteSpace(model.AuthorNamesString))
                {
                    string[] nomes = model.AuthorNamesString.Split(',');
                    
                    foreach (string nome in nomes)
                    {
                        string nomeLimpo = nome.Trim();
                        if (!string.IsNullOrWhiteSpace(nomeLimpo))
                        {
                            var autor = await GetOrCreateAutor(nomeLimpo);
                            if (autor != null)
                            {
                                livro.Autores.Add(autor);
                            }
                        }
                    }
                }
                
                // Add genres  
                if (!string.IsNullOrWhiteSpace(model.GenreNamesString))
                {
                    string[] nomes = model.GenreNamesString.Split(',');
                    
                    foreach (string nome in nomes)
                    {
                        string nomeLimpo = nome.Trim();
                        if (!string.IsNullOrWhiteSpace(nomeLimpo))
                        {
                            var genero = await GetOrCreateGenero(nomeLimpo);
                            if (genero != null)
                            {
                                livro.Generos.Add(genero);
                            }
                        }
                    }
                }
                
                // Set editora id if it exists
                if (model.IdEditora.HasValue)
                {
                    livro.IdEditora = model.IdEditora.Value;
                }
                else if (!string.IsNullOrWhiteSpace(model.NovaEditoraNome))
                {
                    // Create a new editora if it doesn't exist
                    var editora = await GetOrCreateEditora(model.NovaEditoraNome);
                    livro.IdEditora = editora.IdEditora; 
                }

                // Update the book in the database
                var updated = await apiService.PutAsync($"api/livros/{id}", livro);
                if (updated)
                {
                    TempData["Success"] = "Livro atualizado com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
            }
            
            // Get editoras from API and set the editoras list in the view data
            var editoras = await apiService.GetAsync<List<Editora>>("api/editoras");
            ViewData["Editoras"] = new SelectList(editoras ?? new List<Editora>(), "IdEditora", "Name", model.IdEditora);

            return View(model);
        }

        // GET: Livros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var livro = await apiService.GetAsync<Livro>($"api/livros/{id}");

            return View(livro);
        }

        // POST: Livros/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await apiService.DeleteAsync($"api/livros/{id}");
            if (deleted)
            {
                TempData["Success"] = "Livro removido com sucesso.";
            }
            else
            {
                TempData["Error"] = "Erro ao remover o livro.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to parse AnoPublicacao from form
        private DateOnly ParseAnoPublicacaoFromForm()
        {
            var dataTexto = Request.Form["AnoPublicacao"].ToString();
            var formatoData = "dd/MM/yyyy";
            return DateOnly.ParseExact(dataTexto, formatoData, System.Globalization.CultureInfo.InvariantCulture);
        }

        // Helper methods to create or get existing entities via API
        private async Task<Autor?> GetOrCreateAutor(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            
            var trimmedName = name.Trim();

            // Get authors from API
            var autores = await apiService.GetAsync<List<Autor>>("api/autores");
            
            // Check if the author exists
            var autor = autores?.FirstOrDefault(a => 
                a.Name.Trim().Equals(trimmedName, StringComparison.OrdinalIgnoreCase));
            
            // Create the author if it doesn't exist
            if (autor == null)
            {
                autor = new Autor { Name = trimmedName };
                var created = await apiService.PostAsync<Autor>("api/autores", autor);
                if (created != null)
                {
                    autor = created;
                }
            }
            
            return autor;
        }

        // Helper method to create or get existing genres via API
        private async Task<Genero?> GetOrCreateGenero(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            
            var trimmedName = name.Trim();

            // Get genres from API
            var generos = await apiService.GetAsync<List<Genero>>("api/generos");

            // Check if the genre exists
            var genero = generos?.FirstOrDefault(g => 
                g.Name.Trim().Equals(trimmedName, StringComparison.OrdinalIgnoreCase));
            
            // Create the genre if it doesn't exist
            if (genero == null)
            {
                genero = new Genero { Name = trimmedName };
                var created = await apiService.PostAsync<Genero>("api/generos", genero);
                if (created != null)
                {
                    genero = created;
                }
            }
            
            return genero;
        }

        // Helper method to create or get existing editoras via API
        private async Task<Editora?> GetOrCreateEditora(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            
            var trimmedName = name.Trim();

            // Get editoras from API
            var editoras = await apiService.GetAsync<List<Editora>>("api/editoras");

            // Check if the editora exists
            var editora = editoras?.FirstOrDefault(e => 
                e.Name.Trim().Equals(trimmedName, StringComparison.OrdinalIgnoreCase));
            
            // Create the editora if it doesn't exist
            if (editora == null)
            {
                editora = new Editora { Name = trimmedName };
                var created = await apiService.PostAsync<Editora>("api/editoras", editora);
                if (created != null)
                {
                    editora = created;
                }
            }
            
            return editora;
        }
    }
}
