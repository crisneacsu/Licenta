using Microsoft.EntityFrameworkCore;
using SalaFitness.Models;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Design;

namespace SalaFitness.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
       
        public DbSet<Review> Reviews { get; set; } = null!;


        public DbSet<FitnessClass> FitnessClasses { get; set; } = null!;
        public DbSet<UserFitnessClass> UserFitnessClasses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurăm relația many-to-many între User și FitnessClass prin entitatea UserFitnessClass
            modelBuilder.Entity<UserFitnessClass>()
                .HasKey(ufc => new { ufc.UserId, ufc.FitnessClassId });

            modelBuilder.Entity<UserFitnessClass>()
                .HasOne(ufc => ufc.User)
                .WithMany(u => u.FitnessClasses)
                .HasForeignKey(ufc => ufc.UserId);

            modelBuilder.Entity<UserFitnessClass>()
                .HasOne(ufc => ufc.FitnessClass)
                .WithMany(fc => fc.Enrollments)
                .HasForeignKey(ufc => ufc.FitnessClassId);
        }
    }
}
