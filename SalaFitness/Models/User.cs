using System.ComponentModel.DataAnnotations;
namespace SalaFitness.Models;


public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Numele este obligatoriu")]
    [MaxLength(50, ErrorMessage = "Numele nu poate avea mai mult de 50 de caractere")]
    public string Nume { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [MaxLength(50, ErrorMessage = "Prenumele nu poate avea mai mult de 50 de caractere")]
    public string Prenume { get; set; } = string.Empty;

    public Gender Gen { get; set; }

    [Range(16, 100, ErrorMessage = "Vârsta trebuie să fie între 16 și 100 de ani")]
    public int Varsta { get; set; }

    [Required(ErrorMessage = "Adresa de email este obligatorie")]
    [EmailAddress(ErrorMessage = "Adresa de email nu este validă")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie")]
    [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere")]
    public string Parola { get; set; } = string.Empty;

    public UserRole Rol { get; set; }

    // Proprietatea pentru tipul abonamentului, inițial "None"
    public AbonamentType Abonament { get; set; } = AbonamentType.None;

    // Data la care expiră abonamentul, null dacă nu există un abonament activ
    public DateTime? SubscriptionExpiry { get; set; } = null;

    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiration { get; set; }

    public string? ProfileImagePath { get; set; }

    public ICollection<UserFitnessClass> FitnessClasses { get; set; } = new List<UserFitnessClass>();
    [Required]
    public DateTime CreatedAt { get; internal set; }
    [Required]
    public DateTime UpdatedAt { get; internal set; }
}

public enum Gender
{
    Masculin,
    Feminin
}

public enum UserRole
{
    Admin,
    Antrenor,
    Client
    
}

// Enum pentru tipul de abonament
public enum AbonamentType
{
    None = 0,
    Matinal,             // 140 lei
    MatinalParcare,      // 170 lei
    AllHours,            // 180 lei
    AllHoursParcare      // 210 lei
}