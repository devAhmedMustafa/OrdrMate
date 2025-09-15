using Microsoft.EntityFrameworkCore;
using OrdrMate.Features.ItemAvailability;
using OrdrMate.Features.Preport;
using OrdrMate.Models;

namespace OrdrMate.Data;

public class OrdrMateDbContext(DbContextOptions<OrdrMateDbContext> options) 
    : DbContext (options)
{
    public DbSet<User> User => Set<User>();
    public DbSet<FirebaseToken> FirebaseToken => Set<FirebaseToken>();
    public DbSet<Pharmacy> Pharmacy => Set<Pharmacy>();
    public DbSet<PharmacyProfile> PharmacyProfile => Set<PharmacyProfile>();
    public DbSet<Item> Item => Set<Item>();
    public DbSet<ItemAvailability> ItemAvailabilities => Set<ItemAvailability>();
    public DbSet<ItemCustomization> ItemCustomizations => Set<ItemCustomization>();
    public DbSet<Category> Category => Set<Category>();
    public DbSet<Branch> Branch => Set<Branch>();
    public DbSet<BranchRequest> BranchRequest => Set<BranchRequest>();
    public DbSet<Order> Order => Set<Order>();
    public DbSet<Takeaway> Takeaway => Set<Takeaway>();
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

        // Pharmacy

        modelBuilder.Entity<Pharmacy>().HasKey(r => r.Id);
        modelBuilder.Entity<Pharmacy>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<Pharmacy>()
            .HasOne(r => r.Manager)
            .WithMany()
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.Cascade);

        // PharmacyProfile
        modelBuilder.Entity<PharmacyProfile>().HasKey(rp => rp.PharmacyId);
        modelBuilder.Entity<PharmacyProfile>()
            .HasOne(rp => rp.Pharmacy)
            .WithOne(r => r.Profile)
            .HasForeignKey<PharmacyProfile>(rp => rp.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category

        modelBuilder.Entity<Category>().HasIndex(c => new { c.Name, c.PharmacyId }).IsUnique();
        modelBuilder.Entity<Category>().HasKey(c => new { c.Name, c.PharmacyId });
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Pharmacy)
            .WithMany(r => r.Categories)
            .HasForeignKey(c => c.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.Subcategories)
            .HasForeignKey(c => new { c.Parent, c.PharmacyId })
            .OnDelete(DeleteBehavior.Restrict);

        // Item

        modelBuilder.Entity<Item>().HasKey(i => i.Id);
        modelBuilder.Entity<Item>().HasIndex(i => new { i.Name, i.CategoryName, i.PharmacyId }).IsUnique();
        modelBuilder.Entity<Item>()
            .HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => new { i.CategoryName, i.PharmacyId });

        modelBuilder.Entity<Item>()
            .HasOne(i => i.Pharmacy)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Branch

        modelBuilder.Entity<Branch>().HasKey(b => b.Id);
        modelBuilder.Entity<Branch>().HasIndex(b => b.Phone).IsUnique();
        modelBuilder.Entity<Branch>().HasIndex(b => new { b.Latitude, b.Longitude, b.PharmacyId }).IsUnique();
        modelBuilder.Entity<Branch>()
            .HasOne(b => b.Pharmacy)
            .WithMany(r => r.Branches)
            .HasForeignKey(b => b.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Branch>()
            .HasOne(b => b.BranchManager)
            .WithMany()
            .HasForeignKey(b => b.BranchManagerId)
            .OnDelete(DeleteBehavior.Cascade);

        // BranchRequest

        modelBuilder.Entity<BranchRequest>().HasKey(br => br.Id);
        modelBuilder.Entity<BranchRequest>().HasIndex(br => new { br.Latitude, br.Longitude, br.PharmacyId }).IsUnique();
        modelBuilder.Entity<BranchRequest>()
            .HasOne(br => br.Pharmacy)
            .WithMany(r => r.BranchRequests)
            .HasForeignKey(br => br.PharmacyId)
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
            .WithMany(i => i.Customizations)
            .HasForeignKey(ic => ic.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Delivery
        modelBuilder.Entity<Delivery>().HasKey(d => d.OrderId);
        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.Order)
            .WithMany()
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdrMateDbContext).Assembly);

    }


}