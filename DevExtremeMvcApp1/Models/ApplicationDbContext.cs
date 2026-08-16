using System.Data.Entity; // EF6'nın kütüphanesi bu
using DevExtremeMvcApp1.Models;

namespace DevExtremeMvcApp1.Data
{
    public class ApplicationDbContext : DbContext
    {
        // "DefaultConnection" ismi Web.config'deki ile aynı olmalı
        public ApplicationDbContext() : base("name=DefaultConnection") { }

        public DbSet<CalculationResult> CalculationResults { get; set; }

        public DbSet<AppUser> AppUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalculationResult>()
                .HasOptional(x => x.AppUser)
                .WithMany(x => x.CalculationResults)
                .HasForeignKey(x => x.AppUserId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
