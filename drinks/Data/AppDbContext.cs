using drinks.Models.Entities;
using drinks.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace drinks.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Coin> Coins => Set<Coin>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>().HasData(
            new Brand { 
                Id = 1, 
                Name = "Coca-Cola" },

            new Brand { 
                Id = 2, 
                Name = "Dr. Pepper" },

            new Brand { 
                Id = 3, 
                Name = "Fanta" },

            new Brand { 
                Id = 4, 
                Name = "Sprite" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { 
                Id = 1, 
                Name = "Coca-Cola 0.5L", 
                BrandId = 1, 
                Price = 80, 
                Quantity = 10, 
                ImageUrl = "/images/products/coca-cola.png",
                IsInCart = false
            },

            new Product { 
                Id = 2, 
                Name = "Sprite 0.5L", 
                BrandId = 4, 
                Price = 75, 
                Quantity = 8, 
                ImageUrl = "/images/products/sprite.png",
                IsInCart = false
            },

            new Product { 
                Id = 3, 
                Name = "Fanta 0.5L", 
                BrandId = 3, 
                Price = 70, 
                Quantity = 5, 
                ImageUrl = "/images/products/fanta.png",
                IsInCart = false
            },

            new Product { 
                Id = 4, 
                Name = "Dr. Pepper Zero 0.5L", 
                BrandId = 2, 
                Price = 70, 
                Quantity = 5, 
                ImageUrl = "/images/products/pepper.png",
                IsInCart = false
            }
        );

        modelBuilder.Entity<Coin>().HasData(
            new Coin { 
                Id = 1, 
                Denomination = 1,
                Count = 100,
                IsBlocked = false
            },
            new Coin { 
                Id = 2, 
                Denomination = 2,
                Count = 100,
                IsBlocked = false
            },
            new Coin { 
                Id = 3, 
                Denomination = 5,
                Count = 50,
                IsBlocked = false
            },
            new Coin { 
                Id = 4, 
                Denomination = 10,
                Count = 50,
                IsBlocked = false
            }
        );

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId);
    }
}