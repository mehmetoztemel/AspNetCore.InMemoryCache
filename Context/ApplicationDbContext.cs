using AspNetCore.InMemoryCache.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.InMemoryCache.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasKey(p => p.Id);
            base.OnModelCreating(modelBuilder);
        }


        public DbSet<Person> Persons { get; set; }
    }
}