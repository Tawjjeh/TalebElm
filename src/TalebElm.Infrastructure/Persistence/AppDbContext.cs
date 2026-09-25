
using Microsoft.EntityFrameworkCore;


namespace TalebElm.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
