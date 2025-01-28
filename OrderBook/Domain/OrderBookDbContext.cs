using Microsoft.EntityFrameworkCore;
using OrderBook.Domain.Entities;

namespace OrderBook.Domain
{
    public class OrderBookDbContext : DbContext
    {
        public DbSet<OrderBookItem> OrderBookItems { get; set; }

        public OrderBookDbContext(DbContextOptions<OrderBookDbContext> options) : base(options) { }
    }
}
