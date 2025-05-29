using Microsoft.EntityFrameworkCore;
using Rishvi.Models;

namespace Rishvi.Data
{
    public class ApplicationDbContext : DbContext // Fix: Inherit from DbContext  
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<WebhookOrder> Orders { get; set; }
        public DbSet<Run> Runs { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasKey(e => e.id).HasName("PK_Event");
             // Defines primary key using fluent API

            modelBuilder.Entity<WebhookOrder>()
               .HasKey(o => o.id).HasName("PK_WebhookOrder");


            modelBuilder.Entity<Subscription>()
               .HasKey(s => s.id).HasName("PK_Subscription");

            modelBuilder.Entity<Run>()
              .HasKey(r => r.id).HasName("PK_Run");
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=rds-master-admin-panel.c0s0ifu7tzo1.eu-west-2.rds.amazonaws.com;Database=ebay_stream;uid=mdbAdmin;pwd=gYk9bKztXFhANPuMYp8E;Trusted_Connection=false;Integrated Security=false;MultipleActiveResultSets=True;Connect Timeout=200; Pooling=true; Max Pool Size=200;TrustServerCertificate=True");
        }
    }
}
