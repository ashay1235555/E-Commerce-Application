using Microsoft.EntityFrameworkCore;
using UserService.Entity;

namespace UserService.Context
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }

        public DbSet<Entity.User> Users { get; set; }
    }
}
