using Microsoft.EntityFrameworkCore;
using Rishvi.Models;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.ShippingIntegrations.Models;

public class ApplicationDbContext : DbContext
{

    public ApplicationDbContext()
    {

    }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    public DbSet<OrderRoot> OrderRoot { get; set; }

    public DbSet<WebhookResp.Event> Events { get; set; }
    // public DbSet<WebhookOrder> Orders { get; set; }
    public DbSet<WebhookResponse.Run> Runs { get; set; }
    public DbSet<WebhookResp.Subscription> Subscriptions { get; set; }
    public DbSet<GeneralInfo> GeneralInfo { get; set; }
    public DbSet<ShippingInfo> ShippingInfo { get; set; }
    public DbSet<TaxInfo> TaxInfo { get; set; }

    public DbSet<Rishvi.Models.Item> Item { get; set; }
    public DbSet<CustomerInfo> CustomerInfo { get; set; }
    public DbSet<TotalsInfo> TotalsInfo { get; set; }
    public DbSet<Fulfillment> Fulfillment { get; set; }

    public DbSet<ClientAuth> ClientAuth { get; set; }

    public DbSet<Rishvi.Models.Authorization> Authorizations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Event>()
            .HasKey(e => e.id).HasName("PK_Event");
        // Defines primary key using fluent API

        builder.Entity<WebhookOrder>()
            .HasKey(o => o.id).HasName("PK_WebhookOrder");


        builder.Entity<Subscription>()
            .HasKey(s => s.id).HasName("PK_Subscription");

        builder.Entity<Run>()
            .HasKey(r => r.id).HasName("PK_Run");

        builder.Entity<WebhookResp.Event>()
            .HasNoKey();

        builder.Entity<WebhookResponse.Run>()
            .HasNoKey();

        builder.Entity<WebhookResp.Subscription>().HasNoKey();
        //builder.Entity<WebhookOrder>(entity =>
        //{
        //    entity.ToTable("Orders");   // same table
        //    entity.HasNoKey();          // treat as keyless entity
        //});
        builder.Entity<OrderRoot>().ToTable("Orders");
        builder.ShadowProperties();
        base.OnModelCreating(builder);

        // Interface that all of our Entity maps implement
        var assembly = typeof(ApplicationDbContext).Assembly;

        // Types that do entity mapping
        var mappingTypes = assembly.GetTypes().Where(type => type.GetInterfaces().Any() && type.Name.EndsWith("Configuration"));

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

    // public override async Task<int> SaveChangesAsync(CancellationToken cancellation = default)
    // {
    //     await Task.Run(() => TimestampUpdate(), cancellation);
    //     try
    //     {
    //         return await base.SaveChangesAsync(cancellation);
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new InvalidOperationException(ex.Message);
    //     }
    // }

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
