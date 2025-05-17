using Customer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Customer.API
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
            : base(options)
        {
        }


        public DbSet<Models.Customer> Customers { get; set; }
        public DbSet<Models.Order> Orders { get; set; }
    }
}
