using System.ComponentModel.DataAnnotations;
using SalaFitness.Models;

namespace SalaFitness.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Numele este obligatoriu")]
        [MaxLength(50, ErrorMessage = "Numele nu poate avea mai mult de 50 de caractere")]
        public string Nume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prenumele este obligatoriu")]
        [MaxLength(50, ErrorMessage = "Prenumele nu poate avea mai mult de 50 de caractere")]
        public string Prenume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genul este obligatoriu")]
        public Gender Gen { get; set; }

        [Range(16, 100, ErrorMessage = "Vârsta trebuie să fie între 16 și 100")]
        public int Varsta { get; set; }

        [Required(ErrorMessage = "Emailul este obligatoriu")]
        [EmailAddress(ErrorMessage = "Email invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola este obligatorie")]
        [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere")]
        public string Parola { get; set; } = string.Empty;

        [Display(Name = "Cod Admin (opțional)")]
        public string? AdminCode { get; set; }

        [Display(Name = "Cod Antrenor (opțional)")]
        public string? TrainerCode { get; set; }
    }
}
