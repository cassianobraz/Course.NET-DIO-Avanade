using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entity;

namespace minimal_api.Infra.Db;

public class DbContextDio : DbContext
{
    private readonly IConfiguration _configuration;
    public DbContextDio(IConfiguration configuration) => _configuration = configuration;

    public DbSet<Admin> Admins { get; set; } = default!;
    public DbSet<Vehicle> Vehicles { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var conn = _configuration.GetConnectionString("DefaultConnection")?.ToString();

            if (!string.IsNullOrEmpty(conn))
                optionsBuilder.UseSqlServer(conn);
        }
    }
}
