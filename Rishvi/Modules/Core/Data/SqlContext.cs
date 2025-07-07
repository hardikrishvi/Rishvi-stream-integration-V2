using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rishvi.Models;
using Rishvi.Modules.ShippingIntegrations.Models;
using Address = Rishvi.Models.Address;
using Item = Rishvi.Models.Item;

namespace Rishvi.Modules.Core.Data
{
    public class SqlContext : DbContext
    { 
        public SqlContext(DbContextOptions<SqlContext> context) : base(context)
        {
        }


        public DbSet<Event> Events { get; set; }
        public DbSet<WebhookOrder> WebhookOrders { get; set; }
        public DbSet<Run> Runs { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Models.Authorization> Authorizations { get; set; }


        public DbSet<Address> Address { get; set; }
        public DbSet<OrderRoot> OrderRoot { get; set; }
        public DbSet<GeneralInfo> GeneralInfo { get; set; }
        public DbSet<ShippingInfo> ShippingInfo { get; set; }
        public DbSet<TaxInfo> TaxInfo { get; set; }
        public DbSet<Item> Item { get; set; }
    
        public DbSet<CustomerInfo> CustomerInfo { get; set; }
        public DbSet<TotalsInfo> TotalsInfo { get; set; }
        public DbSet<Fulfillment> Fulfillment { get; set; }
        
        public DbSet<IntegrationSettings> IntegrationSettings { get; set; }
        public DbSet<LinnworksSettings> LinnworksSettings { get; set; }
        public DbSet<StreamSettings> StreamSettings { get; set; }
        public DbSet<SyncSettings> SyncSettings { get; set; }
        public DbSet<Ebay> Ebay { get; set; }
        public DbSet<ReportModel> ReportModel { get; set; }
        public DbSet<StreamOrderRecord> StreamOrderRecord { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReportModel>().HasKey(x => x._id);
            builder.ShadowProperties();
            base.OnModelCreating(builder);
            builder.Entity<ReportModel>(); 
            builder.Entity<IntegrationSettings>(); 
            builder.Entity<LinnworksSettings>(); 
            builder.Entity<StreamSettings>(); 
            builder.Entity<SyncSettings>(); 
            builder.Entity<Ebay>();
            builder.Entity<Models.Authorization>();
            
            builder.Entity<OrderRoot>(); 
            builder.Entity<Address>(); 
            builder.Entity<GeneralInfo>(); 
            builder.Entity<ShippingInfo>(); 
            builder.Entity<TaxInfo>(); 
            builder.Entity<Item>(); 
            builder.Entity<OrderRoot>(); 
            builder.Entity<CustomerInfo>(); 
            builder.Entity<TotalsInfo>(); 
            builder.Entity<Fulfillment>(); 
            builder.Entity<CustomerInfo>()
                    .HasOne(c => c.Address)
                    .WithMany()
                    .HasForeignKey(c => c.AddressId)
                    .OnDelete(DeleteBehavior.Cascade); // Keep cascade for this
            
            builder.Entity<CustomerInfo>()
                    .HasOne(c => c.BillingAddress)
                    .WithMany()
                    .HasForeignKey(c => c.BillingAddressId)
                    .OnDelete(DeleteBehavior.Restrict); // Pre

            // Interface that all of our Entity maps implement
            var mappingInterface = typeof(IEntityTypeConfiguration<>);

            // Types that do entity mapping
            var mappingTypes = typeof(SqlContext).GetTypeInfo().Assembly.GetTypes()
                .Where(x => x.GetInterfaces().Any(y => y.GetTypeInfo().IsGenericType && y.GetGenericTypeDefinition() == mappingInterface));

            // Get the generic Entity method of the ModelBuilder type
            var entityMethod = typeof(ModelBuilder).GetMethods()
                .Single(x => x.Name == "Entity" &&
                        x.IsGenericMethod &&
                        x.ReturnType.Name == "EntityTypeBuilder`1");

            foreach (var mappingType in mappingTypes)
            {
                // Get the type of entity to be mapped
                var genericTypeArg = mappingType.GetInterfaces().Single().GenericTypeArguments.Single();

                // Get the method builder.Entity<TEntity>
                var genericEntityMethod = entityMethod.MakeGenericMethod(genericTypeArg);

                // Invoke builder.Entity<TEntity> to get a builder for the entity to be mapped
                var entityBuilder = genericEntityMethod.Invoke(builder, null);

                // Create the mapping type and do the mapping
                var mapper = Activator.CreateInstance(mappingType);
                mapper.GetType().GetMethod("Map").Invoke(mapper, new[] { entityBuilder });
            }
        }

        #region SaveChanges
        public override int SaveChanges()
        {
            TimestampUpdate();
            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellation = default)
        {
            await Task.Run(() => TimestampUpdate(), cancellation);
            try
            {
                return await base.SaveChangesAsync(cancellation);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        private void TimestampUpdate()
        {
            foreach (var history in ChangeTracker.Entries()
                .Where(e => e.Entity is IModificationHistory &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified))
                .Select(e => e.Entity as IModificationHistory))
            {
                if (history == null)
                {
                    continue;
                }

                history.UpdatedAt = DateTime.Now;

                if (history.CreatedAt == DateTime.MinValue)
                {
                    history.CreatedAt = DateTime.Now;
                }
            }
        }
        #endregion
    }
}