using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalaFitness.Context;
using SalaFitness.Models;
using System.Security.Claims;

namespace SalaFitness.Controllers
{
    public class FitnessClassesController : Controller
    {
        private readonly AppDbContext _context;

        public FitnessClassesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /FitnessClasses/Create

        [Authorize(Roles = "Admin,Antrenor")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /FitnessClasses/Create
        [Authorize(Roles = "Admin,Antrenor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FitnessClass model)
        {
            if (!ModelState.IsValid)
                return View(model);


            model.StartTime = DateTime.SpecifyKind(model.StartTime, DateTimeKind.Utc);
            model.EndTime = DateTime.SpecifyKind(model.EndTime, DateTimeKind.Utc);

            _context.FitnessClasses.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: /FitnessClasses/Index
        // Afișează doar clasele care încă nu au expirat

        [Authorize(Roles = "Admin,Antrenor,Client")]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var classes = await _context.FitnessClasses
                .Where(fc => fc.EndTime > now) // doar clasele a căror EndTime e în viitor
                .Include(fc => fc.Enrollments)
                .ToListAsync();

            return View(classes);
        }

        [Authorize(Roles = "Admin,Antrenor,Client")]
        // GET: /FitnessClasses/Details/5
        
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var fitnessClass = await _context.FitnessClasses
                .Include(fc => fc.Enrollments)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(fc => fc.Id == id);

            if (fitnessClass == null)
                return NotFound();

            return View(fitnessClass);
        }

        // POST: /FitnessClasses/Enroll/5
        [Authorize(Roles = "Admin,Antrenor,Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int id)
        {
            // id este ID-ul clasei
            var fitnessClass = await _context.FitnessClasses
                .Include(fc => fc.Enrollments)
                .FirstOrDefaultAsync(fc => fc.Id == id);

            if (fitnessClass == null)
                return NotFound();

            // Verificăm capacitatea
            if (fitnessClass.Enrollments.Count >= fitnessClass.MaxCapacity)
            {
                TempData["ErrorMessage"] = "Clasa este deja plină!";
                return RedirectToAction("Details", new { id = id });
            }

            // Obținem user-ul curent (presupunem că userul este autentificat)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["ErrorMessage"] = "Nu te poți înscrie, te rog loghează-te.";
                return RedirectToAction("Login", "Users");
            }

            // Verificăm dacă userul este deja înscris la această clasă
            if (fitnessClass.Enrollments.Any(e => e.UserId == userId))
            {
                TempData["ErrorMessage"] = "Ești deja înscris la această clasă!";
                return RedirectToAction("Details", new { id = id });
            }

            // Adăugăm înscrierea
            fitnessClass.Enrollments.Add(new UserFitnessClass
            {
                UserId = userId,
                FitnessClassId = id
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Te-ai înscris cu succes!";
            return RedirectToAction("Details", new { id = id });
        }
    }

}
