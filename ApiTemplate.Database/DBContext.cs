using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiTemplate.Database;

public class DBContext : IdentityDbContext<IdentityUser>
{
    public DBContext(DbContextOptions<DBContext> options) : base(options) { }

    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}




