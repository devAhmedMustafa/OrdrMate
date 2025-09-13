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
    public DbSet<Pharmacy> Restaurant => Set<Pharmacy>();
    public DbSet<PharmacyProfile> RestaurantProfile => Set<PharmacyProfile>();
    public DbSet<Item> Item => Set<Item>();
    public DbSet<ItemAvailability> ItemAvailabilities => Set<ItemAvailability>();
    public DbSet<Category> Category => Set<Category>();
    public DbSet<Branch> Branch => Set<Branch>();
    public DbSet<BranchRequest> BranchRequest => Set<BranchRequest>();
    public DbSet<Table> Table => Set<Table>();
    public DbSet<TableReservation> TableReservation => Set<TableReservation>();
    public DbSet<Kitchen> Kitchen => Set<Kitchen>();
    public DbSet<KitchenPower> KitchenPower => Set<KitchenPower>();
    public DbSet<Order> Order => Set<Order>();
    public DbSet<Indoor> Indoor => Set<Indoor>();
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

        // Restaurant

        modelBuilder.Entity<Pharmacy>().HasKey(r => r.Id);
        modelBuilder.Entity<Pharmacy>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<Pharmacy>()
            .HasOne(r => r.Manager)
            .WithMany()
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.Cascade);

        // RestaurantProfile
        modelBuilder.Entity<PharmacyProfile>().HasKey(rp => rp.PharmacyId);
        modelBuilder.Entity<PharmacyProfile>()
            .HasOne(rp => rp.Restaurant)
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

        modelBuilder.Entity<Item>()
            .HasOne(i => i.Kitchen)
            .WithMany()
            .HasForeignKey(i => i.KitchenId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Branch

        modelBuilder.Entity<Branch>().HasKey(b => b.Id);
        modelBuilder.Entity<Branch>().HasIndex(b => b.Phone).IsUnique();
        modelBuilder.Entity<Branch>().HasIndex(b => new { b.Latitude, b.Longitude, b.RestaurantId }).IsUnique();
        modelBuilder.Entity<Branch>()
            .HasOne(b => b.Restaurant)
            .WithMany(r => r.Branches)
            .HasForeignKey(b => b.RestaurantId)
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


        // Table

        modelBuilder.Entity<Table>().HasKey(t => new { t.TableNumber, t.BranchId });
        modelBuilder.Entity<Table>().HasIndex(t => new { t.TableNumber, t.BranchId }).IsUnique();
        modelBuilder.Entity<Table>()
            .HasOne(t => t.Branch)
            .WithMany(b => b.Tables)
            .HasForeignKey(t => t.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // TableReservation

        modelBuilder.Entity<TableReservation>().HasKey(tr => tr.ReservationId);
        modelBuilder.Entity<TableReservation>().HasIndex(tr => new { tr.TableNumber, tr.BranchId, tr.ReservationTime }).IsUnique();
        modelBuilder.Entity<TableReservation>()
            .HasOne(tr => tr.Branch)
            .WithMany()
            .HasForeignKey(tr => tr.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TableReservation>()
            .HasOne(tr => tr.Customer)
            .WithMany()
            .HasForeignKey(tr => tr.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TableReservation>()
            .HasOne(tr => tr.Order)
            .WithOne()
            .HasForeignKey<TableReservation>(tr => tr.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Kitchen

        modelBuilder.Entity<Kitchen>().HasKey(k => k.Id);
        modelBuilder.Entity<Kitchen>().HasIndex(k => new { k.Name, k.RestaurantId }).IsUnique();
        modelBuilder.Entity<Kitchen>()
            .HasOne(k => k.Restaurant)
            .WithMany(r => r.Kitchens)
            .HasForeignKey(k => k.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // KitchenPower

        modelBuilder.Entity<KitchenPower>().HasKey(kp => new { kp.BranchId, kp.KitchenId });
        modelBuilder.Entity<KitchenPower>()
            .HasIndex(kp => new { kp.BranchId, kp.KitchenId })
            .IsUnique();
        modelBuilder.Entity<KitchenPower>()
            .HasOne(kp => kp.Branch)
            .WithMany(b => b.KitchenPowers)
            .HasForeignKey(kp => kp.BranchId)
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

        // Indoor

        modelBuilder.Entity<Indoor>().HasKey(i => new { i.TableNumber, i.BranchId, i.OrderId });
        modelBuilder.Entity<Indoor>()
            .HasOne(i => i.Order)
            .WithOne()
            .HasForeignKey<Indoor>(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Indoor>()
            .HasOne(i => i.Branch)
            .WithMany()
            .HasForeignKey(i => i.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Takeaway

        modelBuilder.Entity<Takeaway>().HasKey(t => new { t.OrderId, t.OrderNumber });
        modelBuilder.Entity<Takeaway>()
            .HasOne(t => t.Order)
            .WithOne()
            .HasForeignKey<Takeaway>(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Takeaway>()
            .HasOne(t => t.Order)
            .WithMany()
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // DeliverRequest

        modelBuilder.Entity<DeliverRequest>().HasKey(dr => dr.OrderId);
        modelBuilder.Entity<DeliverRequest>()
            .HasOne(dr => dr.Order)
            .WithOne()
            .HasForeignKey<DeliverRequest>(dr => dr.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemCustomization>().HasKey(ic => new { ic.ItemId, ic.CategoryId });
        modelBuilder.Entity<ItemCustomization>()
            .HasOne(ic => ic.Item)
            .WithMany(i => i.Customizations)
            .HasForeignKey(ic => ic.ItemId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdrMateDbContext).Assembly);

    }


}