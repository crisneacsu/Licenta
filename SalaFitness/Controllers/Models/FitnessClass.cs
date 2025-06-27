namespace SalaFitness.Models
{
    public class FitnessClass
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // de ex. "Yoga", "Pilates"
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxCapacity { get; set; } // de ex. 15

        // Navigare: legătură many-to-many cu utilizatorii înscriși
        public ICollection<UserFitnessClass> Enrollments { get; set; } = new List<UserFitnessClass>();
    }

}
