using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC.Models;
using MVC.Services;

namespace MVC.Controllers
{
    public class UtilizadoresController : Controller
    {
        private readonly ApiService apiService;

        public UtilizadoresController(ApiService _apiService)
        {
            apiService = _apiService;
        }

        // GET: Utilizadores
        public async Task<IActionResult> Index()
        {
            // Get all utilizadores from API
            var utilizadores = await apiService.GetAsync<List<Utilizador>>("api/utilizadores");

            // Return the utilizadores
            return View(utilizadores ?? new List<Utilizador>());
        }

        // GET: Utilizadores/Details/5
        public async Task<IActionResult> Details(int? id)
        {  
            // Get the utilizador from API
            var utilizador = await apiService.GetAsync<Utilizador>($"api/utilizadores/{id}"); 

            return View(utilizador);
        }

        // GET: Utilizadores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Utilizadores/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("IdUtilizador,TipoUtilizador,Nome,Email,Password")] Utilizador utilizador)
        {
            if (ModelState.IsValid)
            {
                var created = await apiService.PostAsync<Utilizador>("api/utilizadores", utilizador);
                if (created != null)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(utilizador);
        }

        // GET: Utilizadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilizador = await apiService.GetAsync<Utilizador>($"api/utilizadores/{id}");
            if (utilizador == null)
            {
                return NotFound();
            }
            return View(utilizador);
        }

        // POST: Utilizadores/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("IdUtilizador,TipoUtilizador,Nome,Email,Password")] Utilizador utilizador)
        {
            if (id != utilizador.IdUtilizador)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var updated = await apiService.PutAsync($"api/utilizadores/{id}", utilizador);
                if (updated)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(utilizador);
        }

        // GET: Utilizadores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilizador = await apiService.GetAsync<Utilizador>($"api/utilizadores/{id}");
            if (utilizador == null)
            {
                return NotFound();
            }

            return View(utilizador);
        }

        // POST: Utilizadores/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await apiService.DeleteAsync($"api/utilizadores/{id}");
            if (deleted)
            {
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
    }
}
