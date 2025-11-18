using Microsoft.EntityFrameworkCore;

namespace ProductService.Context
{
    public class ProductContext:DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }
        public DbSet<Entity.Product> Products { get; set; }
    }
}
