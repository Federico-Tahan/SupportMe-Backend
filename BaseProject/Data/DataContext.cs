using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BaseProject.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
        }

    }
}
