using System;
using com.b_velop.XmlRpc.Models;
using Microsoft.EntityFrameworkCore;

namespace com.b_velop.XmlRpc.Contexts
{
    public class HomeContext : DbContext
    {
        public DbSet<StateEntity> States { get; set; }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=home.db");
        }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StateEntity>().HasData(
                new StateEntity
                {
                    Id = 1,
                    Name = "Alarm",
                    Created = DateTime.Now,
                    State = false
                });
        }
    }
}
