using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalaFitness.Context;
using SalaFitness.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Hosting;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.AspNetCore.Authorization;

namespace SalaFitness.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;

        public UsersController(AppDbContext context, IWebHostEnvironment environment ,IConfiguration config)
        {
            _context = context;
            _environment = environment;
            _config = config;
        }

        // GET: Users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Antrenor,Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nume,Prenume,Gen,Varsta,Email,Parola,Rol")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();

            }


            ViewBag.RolList = new SelectList(new List<string> { "Admin", "Antrenor", "Client" }, user.Rol);
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nume,Prenume,Rol")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Admin")]
        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize(Roles = "Admin")]
        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



       

        // POST: /Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificăm dacă există deja un cont cu această adresă de email
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Există deja un cont cu această adresă de email.");
                return View(model);
            }

            // Determinăm rolul: implicit Client
            UserRole assignedRole = UserRole.Client;

            // Citim codurile secrete din configurație
            var adminSecret = _config["AdminRegistration:SecretCode"];
            var trainerSecret = _config["TrainerRegistration:SecretCode"];

            // Dacă model.AdminCode e completat și potrivit => Admin
            if (!string.IsNullOrWhiteSpace(model.AdminCode) &&
                model.AdminCode == adminSecret)
            {
                
                assignedRole = UserRole.Admin;
            }
            // Altfel, dacă model.TrainerCode e completat și potrivit => Antrenor
            else if (!string.IsNullOrWhiteSpace(model.TrainerCode) &&
                     model.TrainerCode == trainerSecret)
            {
               
                assignedRole = UserRole.Antrenor;

            }
            // În rest, rămâne Client

            // Cream obiectul User pe baza ViewModel-ului
            var user = new User
            {
                Nume = model.Nume,
                Prenume = model.Prenume,
                Gen = model.Gen,
                Varsta = model.Varsta,
                Email = model.Email,
                Parola = HashPassword(model.Parola),
                Rol = assignedRole,
                
                SubscriptionExpiry = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow

            };
            if (assignedRole == UserRole.Admin || assignedRole == UserRole.Antrenor)
            {
                user.Abonament = (Models.AbonamentType)AbonamentType.AllHoursParcare;
            }
            else
            {
                // În rest, rămâne fără abonament activ (None)
                user.Abonament = (Models.AbonamentType)AbonamentType.None;
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login", "Users");
        }




        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // GET: /Account/Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> LoginAsync(string Email, string Parola)
        {
            // Verificăm dacă email-ul și parola au fost introduse
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Parola))
            {
                ModelState.AddModelError("", "Adresa de email și parola sunt obligatorii.");
                return View();
            }

            // Hash-uim parola pentru a compara cu valoarea din baza de date
            string hashedPassword = HashPassword(Parola);

            // Căutăm utilizatorul cu email-ul și parola (hash-ul) corespunzătoare
            var user = _context.Users.FirstOrDefault(u => u.Email == Email && u.Parola == hashedPassword);
            if (user == null)
            {
                // Dacă utilizatorul nu a fost găsit, redirecționăm la login cu mesaj de eroare
                return RedirectToAction("Login", new { error = "Adresa de email sau parola sunt incorecte." });
            }

            // Verificăm dacă abonamentul utilizatorului a expirat
            if (user.SubscriptionExpiry.HasValue && user.SubscriptionExpiry.Value < DateTime.UtcNow)
            {
                // Resetează abonamentul
                user.Abonament = (Models.AbonamentType)AbonamentType.None;
                user.SubscriptionExpiry = null;
                _context.Update(user);
                _context.SaveChanges();
            }

            // Creăm claim-urile pentru utilizator
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Nume + " " + user.Prenume),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Rol.ToString()),

        new Claim("Abonament", user.Abonament.ToString()) // opțional, dacă vrei să afișezi și tipul abonamentului
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Proprietăți suplimentare pentru autentificare
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Cookie persistent
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            };

            // Semnăm utilizatorul și setăm cookie-ul de autentificare
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirecționăm utilizatorul către pagina principală
            return RedirectToAction("Index", "Home");
        }


        // O metodă de logout (opțional)

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Users");
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Users/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Emailul este obligatoriu.");
                return View();
            }

            // Căutăm utilizatorul după email
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Nu a fost găsit niciun utilizator cu această adresă de email.");
                return View();
            }

            // Generează un cod de resetare (folosim un GUID)
            string resetCode = Guid.NewGuid().ToString();
            user.PasswordResetCode = resetCode;
            user.PasswordResetCodeExpiration = DateTime.UtcNow.AddHours(1); // cod valabil 1 oră
            _context.SaveChanges();

            // Trimitem email cu codul de resetare
            await SendResetEmail(user.Email, resetCode);

            // Redirecționează către o pagină de confirmare
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // GET: /Users/ForgotPasswordConfirmation
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Users/ResetPassword?code=...
        [HttpGet]
        [HttpGet]
        public IActionResult ResetPassword()
        {
            // Returnăm un view simplu în care utilizatorul introduce manual datele
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Găsim utilizatorul după email și codul de resetare
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email &&
                                                           u.PasswordResetCode == model.Code &&
                                                           u.PasswordResetCodeExpiration > DateTime.UtcNow);

            if (user == null)
            {
                ModelState.AddModelError("", "Codul de resetare este invalid sau a expirat.");
                return View(model);
            }

            // Actualizează parola (folosind aceeași metodă de hashare)
            user.Parola = HashPassword(model.NewPassword);
            // Curățăm codul de resetare
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiration = null;
            _context.SaveChanges();

            // Redirecționează utilizatorul la pagina de login (sau altundeva)
            return RedirectToAction("Login", "Users");
        }

        // Metodă de trimitere a emailului (exemplu simplificat)
        private async Task SendResetEmail(string toEmail, string resetCode)
        {
            // Configurare SMTP exemplificativă – modifică după setările tale
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("crisneacsu11@gmail.com", "lmmp ybtf erol adhd"),
                EnableSsl = true,
            };


            var mailMessage = new MailMessage
            {
                From = new MailAddress("crisneacsu11@gmail.com"),
                Subject = "Resetare Parolă",
                Body = $"Pentru a-ți reseta parola, folosește următorul cod: {resetCode}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public IActionResult CumparaAbonament(int userId, AbonamentType tip)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Setează abonamentul și data de expirare (o lună de la momentul curent)
            user.Abonament = (Models.AbonamentType)tip;
            user.SubscriptionExpiry = DateTime.UtcNow.AddMonths(1);

            _context.Update(user);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", "Home");
        }
        [HttpGet]
        public IActionResult Abonamente()
        {
            return View();
        }
        [Authorize(Roles = "Admin,Antrenor,Client")]
        [HttpGet]
        public IActionResult Profil()
        {
            // Extragem ID-ul userului din claim-uri
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                // Dacă nu putem converti, redirecționăm la Login
                return RedirectToAction("Login", "Users");
            }

            // Căutăm userul în baza de date
            var user = _context.Users
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                // Dacă userul nu e găsit, redirect
                return RedirectToAction("Login", "Users");
            }

            return View(user); // Transmitem userul la view
        }
        [Authorize(Roles = "Admin,Antrenor,Client")]
        public IActionResult EditProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Users");
            }
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(user);
        }

        // POST: /Users/UploadProfilePicture

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile ProfileImage)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Users");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Definește calea unde salvezi fișierele: wwwroot/images/profiles
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "profiles");

                // Asigură-te că folderul există
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Creează un nume unic pentru fișier (poți folosi user ID și timestamp)
                string uniqueFileName = $"{user.Id}_{DateTime.UtcNow.Ticks}{Path.GetExtension(ProfileImage.FileName)}";

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(fileStream);
                }

                // Stochează calea relativă în DB, astfel încât să poți folosi Url.Content() în view-uri
                user.ProfileImagePath = $"/images/profiles/{uniqueFileName}";

                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("EditProfile");
        }



        [HttpGet]
        public IActionResult Motivatie()
        {
            return View();
        }

    }
}
