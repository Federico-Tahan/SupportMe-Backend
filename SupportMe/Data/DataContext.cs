using Microsoft.EntityFrameworkCore;
using SupportMe.Models;
using System.Linq.Expressions;

namespace SupportMe.Data
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<EmailSenderConfig> EmailSenderConfig { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<GaleryAssets> GaleryAssets { get; set; }
        public DbSet<Category> Category { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
        }

    }
}
