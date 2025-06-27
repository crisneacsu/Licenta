using System.ComponentModel.DataAnnotations;

namespace SalaFitness.Models
{
    public class Doctor
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu")]
        [MaxLength(50, ErrorMessage = "Numele nu poate avea mai mult de 50 de caractere")]
        public string Nume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prenumele este obligatoriu")]
        [MaxLength(50, ErrorMessage = "Prenumele nu poate avea mai mult de 50 de caractere")]
        public string Prenume { get; set; } = string.Empty;
        public DoctorRole Rol { get; set; }
    }
    public enum DoctorRole
    {
        Admin,
        Antrenor,
        Client
        
    }
}
