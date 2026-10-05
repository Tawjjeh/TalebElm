using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.IntegrationTests
{
    public class UserProgressUniqueIndexTests : IDisposable
    {
        readonly AppDbContext _context;
        readonly SqliteConnection _sqliteConnection;
        public UserProgressUniqueIndexTests()
        {
            _sqliteConnection = new SqliteConnection("Filename=:memory:");
            _sqliteConnection.Open();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                 .UseSqlite(_sqliteConnection)
                 .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();


        }

        public void Dispose()
        {
            _sqliteConnection.Close();
            _sqliteConnection.Dispose();

        }

        [Fact]
        public async Task SaveChangesAsync_DuplicateUserIdAndModuleId_ThrowsDbUpdateException()
        {
            var userid = new Guid();
            var moduleId = new Guid();
            using (var context = _context)

            {

                var user = new UserProgress { UserId = userid, ModuleId = moduleId };
                await _context.AddAsync(user);
                var user1 = new UserProgress { UserId = userid, ModuleId = moduleId };
                await _context.AddAsync(user1);
                var extest = await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());

            }
        }
        [Fact]
        public async Task TwoUsersCanProgressForTheSameModule()
        {

            var moduleId = new Guid();
            using (var context = _context)

            {

                var user = new UserProgress { UserId = Guid.NewGuid(), ModuleId = moduleId };
                await _context.AddAsync(user);
                var user1 = new UserProgress { UserId = Guid.NewGuid(), ModuleId = moduleId };
                await _context.AddAsync(user1);
                await _context.SaveChangesAsync();
                var countusers = await _context.UserProgresses.CountAsync();
                Assert.Equal(2, countusers);

            }
        }


    }
}
