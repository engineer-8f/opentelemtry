using Microsoft.EntityFrameworkCore;

namespace Osint.Source.One.Db;

public class CarDbContext(DbContextOptions<CarDbContext> options) : DbContext(options)
{
    public DbSet<CarEntity> Cars { get; set; } = null!;
}