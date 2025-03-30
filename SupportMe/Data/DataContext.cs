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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
        }

    }
}
