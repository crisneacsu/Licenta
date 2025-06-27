using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SalaFitness.Context;
using SalaFitness.Models; // Pentru PaymentRequest, AbonamentType, AbonamentInfo etc.
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SalaFitness.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public PaymentsController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // GET: /Payments/Test
        [Authorize(Roles = "Client")]
        [HttpGet]
        public IActionResult Test()
        {
            return View(); // Views/Payments/Test.cshtml
        }

        // POST: /Payments/TestEndpoint
        [Authorize(Roles = "Client")]
        [HttpPost]
        public IActionResult TestEndpoint([FromBody] Dictionary<string, string> payload)
        {
            if (payload.ContainsKey("test"))
            {
                Console.WriteLine("Am primit: " + payload["test"]);
            }
            else
            {
                Console.WriteLine("Am primit un payload fără cheie 'test'");
            }
            return Json(new { message = "Salut, am primit datele!" });
        }

        // GET: /Payments/Checkout
        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] PaymentRequest request)
        {
            Console.WriteLine($"Abonament tip primit: {request.Tip}");

            var domain = "https://localhost:7013/";

            if (!Enum.TryParse<AbonamentType>(request.Tip, out var tipAbonament))
            {
                return BadRequest("Tip abonament invalid.");
            }

            decimal pretLei = AbonamentInfo.GetPret(tipAbonament);
            string numeAbonament = AbonamentInfo.GetNume(tipAbonament);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(pretLei * 100), // Calcul corect: 140 lei -> 14000 bani
                    Currency = "ron",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Abonament SalaFitness - " + numeAbonament,
                    },
                },
                Quantity = 1,
            },
        },
                Mode = "payment",
                SuccessUrl = domain + "Payments/Success?session_id={CHECKOUT_SESSION_ID}&abonament=" + tipAbonament.ToString(),
                CancelUrl = domain + "Payments/Cancel",
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return Json(new { id = session.Id });
        }



        // GET: /Payments/Success
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> Success(string session_id, string abonament)
        {
            Console.WriteLine($"[Success] session_id: {session_id}, abonament: {abonament}");

            if (string.IsNullOrEmpty(session_id) || string.IsNullOrEmpty(abonament))
            {
                ViewData["ErrorMessage"] = "Parametrii de plată lipsesc (session_id sau abonament).";
                return View("Error");
            }

            var service = new SessionService();
            var session = await service.GetAsync(session_id);
            Console.WriteLine($"[Success] PaymentStatus: {session.PaymentStatus}");

            if (session.PaymentStatus != "paid")
            {
                ViewData["ErrorMessage"] = "Plata nu a fost finalizată cu succes.";
                return View("Error");
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"[Success] User ID from claims: {userIdString}");

            if (!int.TryParse(userIdString, out int userId))
            {
                ViewData["ErrorMessage"] = "Nu s-a putut determina ID-ul userului.";
                return View("Error");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                ViewData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                return View("Error");
            }

            if (Enum.TryParse<AbonamentType>(abonament, out var tipAbonament))
            {
                user.Abonament = (Models.AbonamentType)tipAbonament;
                user.SubscriptionExpiry = DateTime.UtcNow.AddMonths(1);

                _context.Update(user);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[Success] User {user.Id} actualizat: Abonament = {user.Abonament}, Expiry = {user.SubscriptionExpiry}");
            }
            else
            {
                ViewData["ErrorMessage"] = "Tip abonament invalid.";
                return View("Error");
            }

            return View(); // Poți afișa un view de confirmare a plății
        }


        // GET: /Payments/Cancel
        [Authorize(Roles = "Client")]
        [HttpGet]
        public IActionResult Cancel()
        {
            return View(); // Views/Payments/Cancel.cshtml
        }

        // POST: /Payments/CreateAbonamentSession

        [HttpPost]
        public async Task<IActionResult> CreateAbonamentSession([FromBody] AbonamentRequest request)
        {
            if (!Enum.TryParse<AbonamentType>(request.Tip, out var tipAbonament))
            {
                return BadRequest("Tip abonament invalid.");
            }

            decimal pretLei = AbonamentInfo.GetPret(tipAbonament);
            string numeAbonament = AbonamentInfo.GetNume(tipAbonament);
            Console.WriteLine($"Abonament ales: {numeAbonament}, pret: {pretLei} lei");

            var domain = "https://localhost:7013/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(pretLei * 100),
                            Currency = "ron",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Abonament SalaFitness - " + numeAbonament,
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + "Payments/Success?session_id={CHECKOUT_SESSION_ID}&abonament=" + tipAbonament.ToString(),
                CancelUrl = domain + "Payments/Cancel",
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return Json(new { id = session.Id });
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CreateMatinalCheckoutSession()
        {
            return await CreateAbonamentCheckoutSession(AbonamentType.Matinal);
        }
        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CreateMatinalParcareCheckoutSession()
        {
            return await CreateAbonamentCheckoutSession(AbonamentType.MatinalParcare);
        }
        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CreateAllHoursCheckoutSession()
        {
            return await CreateAbonamentCheckoutSession(AbonamentType.AllHours);
        }
        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CreateAllHoursParcareCheckoutSession()
        {
            return await CreateAbonamentCheckoutSession(AbonamentType.AllHoursParcare);
        }
        [Authorize(Roles = "Client")]
        private async Task<IActionResult> CreateAbonamentCheckoutSession(AbonamentType tipAbonament)
        {
            decimal pretLei = AbonamentInfo.GetPret(tipAbonament);
            string numeAbonament = AbonamentInfo.GetNume(tipAbonament);
            Console.WriteLine($"Abonament ales: {numeAbonament}, pret: {pretLei} lei");

            var domain = "https://localhost:7013/";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(pretLei * 20), // lei -> bani
                    Currency = "ron",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Abonament SalaFitness - " + numeAbonament,
                    },
                },
                Quantity = 1,
            },
        },
                Mode = "payment",
                SuccessUrl = domain + "Payments/Success" + tipAbonament.ToString(),
                CancelUrl = domain + "Payments/Cancel"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return Json(new { id = session.Id });
        }
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> SuccessMatinal(string session_id)
        {
            return await UpdateUserAbonamentAfterPayment(session_id, AbonamentType.Matinal);
        }
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> SuccessMatinalParcare(string session_id)
        {
            return await UpdateUserAbonamentAfterPayment(session_id, AbonamentType.MatinalParcare);
        }
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> SuccessAllHours(string session_id)
        {
            return await UpdateUserAbonamentAfterPayment(session_id, AbonamentType.AllHours);
        }
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> SuccessAllHoursParcare(string session_id)
        {
            return await UpdateUserAbonamentAfterPayment(session_id, AbonamentType.AllHoursParcare);
        }
        
        private async Task<IActionResult> UpdateUserAbonamentAfterPayment(string session_id, AbonamentType tipAbonament)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return RedirectToAction("Index", "Home");
            }

            var service = new SessionService();
            var session = await service.GetAsync(session_id);
            if (session.PaymentStatus != "paid")
            {
                ViewData["ErrorMessage"] = "Plata nu a fost finalizată cu succes.";
                return View("Error");
            }

            // Obținem user-ul curent
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                ViewData["ErrorMessage"] = "Nu s-a putut determina ID-ul userului.";
                return View("Error");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                ViewData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                return View("Error");
            }

            user.Abonament = (Models.AbonamentType)tipAbonament;
            user.SubscriptionExpiry = DateTime.UtcNow.AddMonths(1);


            _context.Update(user);
            await _context.SaveChangesAsync();

            return View(); // Returnează view-ul de succes specific
        }
        // In PaymentsController
        [Authorize(Roles = "Client")]
        [HttpGet]
        public IActionResult Checkout(string abonament)
        {
            ViewData["PublishableKey"] = _configuration["Stripe:PublishableKey"];

            // În mod normal, ai un helper AbonamentInfo să convertești string -> AbonamentType -> pret
            if (Enum.TryParse<AbonamentType>(abonament, out var tipAbonament))
            {
                decimal pretLei = AbonamentInfo.GetPret(tipAbonament);
                ViewData["PretLei"] = pretLei;
                ViewData["AbonamentTip"] = abonament;
            }
            else
            {
                // fallback la 0 sau ceva
                ViewData["PretLei"] = 0;
                ViewData["AbonamentTip"] = "Necunoscut";
            }
            return View();
        }

        

    }
}
