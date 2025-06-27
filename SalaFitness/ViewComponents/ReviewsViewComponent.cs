using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalaFitness.Context;     // namespace‑ul tău pentru DbContext
using SalaFitness.Models;      // Review model
using System.Threading.Tasks;

namespace SalaFitness.ViewComponents
{
    // SUFIXUL "ViewComponent" e obligatoriu!
    public class ReviewsViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public ReviewsViewComponent(AppDbContext context)
        {
            _context = context;
        }

        // Va fi apelat din Razor cu: @await Component.InvokeAsync("Reviews")
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // cele mai noi 20 de recenzii
            var reviews = await _context.Reviews
                                        .Include(r => r.User)
                                        .OrderByDescending(r => r.CreatedAt)
                                        .Take(20)
                                        .ToListAsync();

            return View(reviews);   // va căuta Views/Shared/Components/Reviews/Default.cshtml
        }
    }
}
