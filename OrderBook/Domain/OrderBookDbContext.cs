using Microsoft.EntityFrameworkCore;
using OrderBook.Domain.Entities;

namespace OrderBook.Domain
{
    public class OrderBookDbContext : DbContext
    {
        public DbSet<OrderBookItem> OrderBookItems { get; set; }

        public OrderBookDbContext(DbContextOptions<OrderBookDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderBookItem>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,10)");
                entity.Property(e => e.Amount).HasColumnType("decimal(18,10)");
            });
        }
    }
}
