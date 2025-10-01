using Microsoft.EntityFrameworkCore;
using OrdrMate.Features.ItemAvailability;
using OrdrMate.Features.Preport;
using OrdrMate.Models;
using OrdrMate.Features.Customization;

namespace OrdrMate.Data;

public class OrdrMateDbContext(DbContextOptions<OrdrMateDbContext> options) 
    : DbContext (options)
{
    public DbSet<User> User => Set<User>();
    public DbSet<FirebaseToken> FirebaseToken => Set<FirebaseToken>();
    public DbSet<Store> Store => Set<Store>();
    public DbSet<StoreProfile> StoreProfile => Set<StoreProfile>();
    public DbSet<Item> Item => Set<Item>();
    public DbSet<ItemCustomization> ItemCustomization => Set<ItemCustomization>();
    public DbSet<ItemAvailability> ItemAvailabilities => Set<ItemAvailability>();
    public DbSet<Branch> Branch => Set<Branch>();
    public DbSet<BranchRequest> BranchRequest => Set<BranchRequest>();
    public DbSet<Order> Order => Set<Order>();
    public DbSet<Takeaway> Takeaway => Set<Takeaway>();
    public DbSet<Delivery> Delivery => Set<Delivery>();
    public DbSet<OrderItem> OrderItem => Set<OrderItem>();
    public DbSet<Payment> Payment => Set<Payment>();
    public DbSet<OrderIntent> OrderIntent => Set<OrderIntent>();
    public DbSet<DeliverRequest> DeliverRequest => Set<DeliverRequest>();
    public DbSet<PickupReport> PickupReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Manager

        modelBuilder.Entity<User>().HasKey(m => m.Id);
        modelBuilder.Entity<User>().HasIndex(m => m.Username).IsUnique();

        // FirebaseToken
        modelBuilder.Entity<FirebaseToken>().HasKey(ft => ft.Token);
        modelBuilder.Entity<FirebaseToken>()
            .HasOne(ft => ft.User)
            .WithOne()
            .HasForeignKey<FirebaseToken>(ft => ft.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Store

        modelBuilder.Entity<Store>().HasKey(r => r.Id);
        modelBuilder.Entity<Store>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<Store>()
            .HasOne(r => r.Manager)
            .WithMany()
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.Cascade);

        // StoreProfile
        modelBuilder.Entity<StoreProfile>().HasKey(rp => rp.StoreId);
        modelBuilder.Entity<StoreProfile>()
            .HasOne(rp => rp.Store)
            .WithOne(r => r.Profile)
            .HasForeignKey<StoreProfile>(rp => rp.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Item

        modelBuilder.Entity<Item>().HasKey(i => i.Id);
        modelBuilder.Entity<Item>().HasIndex(i => new { i.Name, i.Category, i.SubCategory, i.StoreId }).IsUnique();

        modelBuilder.Entity<Item>()
            .HasOne(i => i.Store)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Branch

        modelBuilder.Entity<Branch>().HasKey(b => b.Id);
        modelBuilder.Entity<Branch>().HasIndex(b => b.Phone).IsUnique();
        modelBuilder.Entity<Branch>().HasIndex(b => new { b.Latitude, b.Longitude, b.StoreId }).IsUnique();
        modelBuilder.Entity<Branch>()
            .HasOne(b => b.Store)
            .WithMany(r => r.Branches)
            .HasForeignKey(b => b.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Branch>()
            .HasOne(b => b.BranchManager)
            .WithMany()
            .HasForeignKey(b => b.BranchManagerId)
            .OnDelete(DeleteBehavior.Cascade);

        // BranchRequest

        modelBuilder.Entity<BranchRequest>().HasKey(br => br.Id);
        modelBuilder.Entity<BranchRequest>().HasIndex(br => new { br.Latitude, br.Longitude, br.StoreId }).IsUnique();
        modelBuilder.Entity<BranchRequest>()
            .HasOne(br => br.Store)
            .WithMany(r => r.BranchRequests)
            .HasForeignKey(br => br.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderIntent

        modelBuilder.Entity<OrderIntent>().HasKey(oi => oi.Id);
        modelBuilder.Entity<OrderIntent>().HasIndex(oi => oi.OrderId).IsUnique();
        modelBuilder.Entity<OrderIntent>()
            .HasOne(oi => oi.Customer)
            .WithMany()
            .HasForeignKey(oi => oi.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderIntent>()
            .HasOne(oi => oi.Branch)
            .WithMany()
            .HasForeignKey(oi => oi.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order

        modelBuilder.Entity<Order>().HasKey(o => o.Id);
        modelBuilder.Entity<Order>().HasIndex(o => o.OrderDate);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Branch)
            .WithMany(b => b.Orders)
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // OrderItem
        modelBuilder.Entity<OrderItem>().HasKey(oi => new { oi.OrderId, oi.ItemId });
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Item)
            .WithMany()
            .HasForeignKey(oi => oi.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Payment
        modelBuilder.Entity<Payment>().HasKey(p => p.Id);
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // Takeaway

        modelBuilder.Entity<Takeaway>().HasKey(t => t.OrderId);
        modelBuilder.Entity<Takeaway>()
            .HasOne(t => t.Order)
            .WithOne(o => o.Takeaway)
            .HasForeignKey<Takeaway>(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // DeliverRequest

        modelBuilder.Entity<DeliverRequest>().HasKey(dr => dr.OrderId);
        modelBuilder.Entity<DeliverRequest>()
            .HasOne(dr => dr.Order)
            .WithOne()
            .HasForeignKey<DeliverRequest>(dr => dr.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Item Customization

        modelBuilder.Entity<ItemCustomization>().HasKey(ic => new { ic.ItemId, ic.CategoryId });
        modelBuilder.Entity<ItemCustomization>()
            .HasOne(ic => ic.Item)
            .WithMany()
            .HasForeignKey(ic => ic.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Delivery
        modelBuilder.Entity<Delivery>().HasKey(d => d.OrderId);
        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.Order)
            .WithOne(o => o.Delivery)
            .HasForeignKey<Delivery>(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdrMateDbContext).Assembly);

    }


}