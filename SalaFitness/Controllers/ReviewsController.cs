using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalaFitness.Context;
using SalaFitness.Models;
using System.Security.Claims;

[Authorize]
public class ReviewsController : Controller
{
    private readonly AppDbContext _ctx;
    public ReviewsController(AppDbContext ctx) => _ctx = ctx;
    [Authorize(Roles = "Admin,Antrenor,Client")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(int rating, string comment)
    {
        if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment))
        {
            TempData["ErrorMessage"] = "Recenzia nu este validă.";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _ctx.Reviews.Add(new Review
        {
            Rating = rating,
            Comment = comment.Trim(),
            UserId = userId
        });
        _ctx.SaveChanges();

        return Redirect(Request.Headers["Referer"].ToString());
    }
}
