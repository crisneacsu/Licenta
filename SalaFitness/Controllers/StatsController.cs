using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalaFitness.Context;
using SalaFitness.Models;

namespace SalaFitness.Controllers
{
    //  /Stats  -> Index()
    //  /Stats/GenderDistribution -> JSON
    [Route("[controller]")]          // baza rutei =  "Stats"
    public class StatsController : Controller
    {
        private readonly AppDbContext _ctx;
        public StatsController(AppDbContext ctx) => _ctx = ctx;

        // GET  /Stats        (pagina cu graficul)
        [HttpGet("")]
        public IActionResult Index() => View();


        // GET  /Stats/GenderDistribution   (date JSON pentru Chart.js)
        [Authorize(Roles = "Admin")]
        [HttpGet("GenderDistribution")]
        public async Task<IActionResult> GenderDistribution()
        {
            // doar utilizatorii cu rol Client
            var q = _ctx.Users.Where(u => u.Rol == UserRole.Client);

            int countFem = await q.CountAsync(u => u.Gen == Gender.Feminin);
            int countBarbati = await q.CountAsync(u => u.Gen == Gender.Masculin);
            int total = countFem + countBarbati;

            double pctFem = total == 0 ? 0 : Math.Round(countFem * 100.0 / total, 1);
            double pctBarbati = total == 0 ? 0 : Math.Round(countBarbati * 100.0 / total, 1);

            return Json(new
            {
                labels = new[] { "Femei", "Barbati" },
                valuesPct = new[] { pctFem, pctBarbati },
                countFem,
                countBarbati
            });
        }
        //  GET /Stats/AbonamentDistribution
        [Authorize(Roles = "Admin")]
        [HttpGet("AbonamentDistribution")]
        public async Task<IActionResult> AbonamentDistribution()
        {
            // doar clienţii
            var q = _ctx.Users.Where(u => u.Rol == UserRole.Client &&
                                          u.Abonament != Models.AbonamentType.None);

            // număr pe fiecare plan
            var matinal = await q.CountAsync(u => u.Abonament == Models.AbonamentType.Matinal);
            var matinalParcare = await q.CountAsync(u => u.Abonament == Models.AbonamentType.MatinalParcare);
            var allHours = await q.CountAsync(u => u.Abonament == Models.AbonamentType.AllHours);
            var allHoursParcare = await q.CountAsync(u => u.Abonament == Models.AbonamentType.AllHoursParcare);

            int total = matinal + matinalParcare + allHours + allHoursParcare;
            if (total == 0) total = 1;   // evit divizare la zero

            // conversie în procente – un singur zecimal
            double pct(double n) => Math.Round(n * 100.0 / total, 1);

            return Json(new
            {
                labels = new[] { "Matinal", "Matinal + Parcare", "All Hours", "All Hours + Parcare" },
                valuesPct = new[] {
            pct(matinal),
            pct(matinalParcare),
            pct(allHours),
            pct(allHoursParcare)
        },
                counts = new
                {
                    matinal,
                    matinalParcare,
                    allHours,
                    allHoursParcare
                }
            });
        }

    }
}
