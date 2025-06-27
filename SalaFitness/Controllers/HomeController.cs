using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalaFitness.Context;
using SalaFitness.Models;

namespace SalaFitness.Controllers
{
    public class HomeController : Controller

    {

      
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("/Home/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            Response.StatusCode = statusCode;
            // Poți trimite un viewModel cu mesaje personalizate pe cod
            ViewData["StatusCode"] = statusCode;
            return View("Error");   // view-ul Views/Home/Error.cshtml
        }


    }
}
