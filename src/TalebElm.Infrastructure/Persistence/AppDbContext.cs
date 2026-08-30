
using Microsoft.EntityFrameworkCore;


namespace TalebElm.Infrastructure.Persistence;

internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
