using System.ComponentModel.DataAnnotations;

namespace SalaFitness.Models
{
    public class UserFitnessClass
    {
        [Required]
        public int Id { get; set; }              // ← cheie primară

        public int UserId { get; set; }          // FK către Users.Id
       
        public int FitnessClassId { get; set; }  // FK către FitnessClasses.Id

        [Required]
        public DateTime EnrolledAt { get; set; } // data înscrierii

        public User User { get; set; }
        public FitnessClass FitnessClass { get; set; }
    }


}
