using Microsoft.EntityFrameworkCore;
using SupportMe.Models;
using SupportMe.Models.Interface;
using System.Linq.Expressions;
using System.Reflection;

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
        public DbSet<UserMercadoPago> UserMercadoPago { get; set; }
        public DbSet<MercadopagoSetup> MercadopagoSetup { get; set; }
        public DbSet<CampaignTags> CampaignTags { get; set; }
        public DbSet<PaymentDetail> PaymentDetail { get; set; }
        public DbSet<PaymentComments> PaymentComments { get; set; }
        public DbSet<CampaignView> CampaignView { get; set; }
        public DbSet<CampaignNotification> CampaignNotification { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var entityType = entity.ClrType;
                if (typeof(IsDeleted).IsAssignableFrom(entityType))
                {
                    var method = typeof(DataContext)
                        .GetMethod(nameof(SetIsDeletedFilter),
                        BindingFlags.NonPublic | BindingFlags.Static
                           )?.MakeGenericMethod(entityType);

                    var filter = method?.Invoke(null, new object[] { this })!;
                    entity.SetQueryFilter((LambdaExpression)filter);
                }
            }
        }


        private static LambdaExpression SetIsDeletedFilter<TEntity>(DataContext context)
        where TEntity : class, IsDeleted
        {

            Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;

            return filter;
        }

    }
}
