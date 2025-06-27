using System;
using System.ComponentModel.DataAnnotations;

namespace SalaFitness.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }          // 1‑5
        [MaxLength(500)]
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // relație cu utilizatorul
        public int UserId { get; set; }
        public User User { get; set; } = default!;
    }

}
