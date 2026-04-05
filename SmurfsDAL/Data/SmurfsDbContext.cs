using Microsoft.EntityFrameworkCore;
using Smurfs.SmurfsBL.entities;

namespace Smurfs.SmurfsDAL.Data;

public class SmurfsDbContext : DbContext
{
    public DbSet<Forest>     Forests     { get; set; } = null!;
    public DbSet<Creature>   Creatures   { get; set; } = null!;
    public DbSet<Schtroumpf> Schtroumpfs { get; set; } = null!;
    public DbSet<Spider>     Spiders     { get; set; } = null!;
    public DbSet<BzzFly>     BzzFlies    { get; set; } = null!;
    public DbSet<Item>       Items       { get; set; } = null!;
    public DbSet<RedPotion>  RedPotions  { get; set; } = null!;
    public DbSet<BluePotion> BluePotions { get; set; } = null!;
    public DbSet<Berry>      Berries     { get; set; } = null!;

    public SmurfsDbContext() { }
    public SmurfsDbContext(DbContextOptions<SmurfsDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=SmurfsGameDB;Trusted_Connection=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPH Creature
        modelBuilder.Entity<Creature>()
            .HasDiscriminator<string>("CreatureType")
            .HasValue<Schtroumpf>("Schtroumpf")
            .HasValue<Spider>("Spider")
            .HasValue<BzzFly>("BzzFly");

        // TPH Item
        modelBuilder.Entity<Item>()
            .HasDiscriminator<string>("ItemType")
            .HasValue<RedPotion>("RedPotion")
            .HasValue<BluePotion>("BluePotion")
            .HasValue<Berry>("Berry");

        // Relations Forest
        modelBuilder.Entity<Forest>()
            .HasMany(f => f.Creatures)
            .WithOne(c => c.Forest)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Forest>()
            .HasMany(f => f.Items)
            .WithOne(i => i.Forest)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
