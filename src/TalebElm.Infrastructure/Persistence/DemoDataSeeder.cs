using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence;

public static class DemoDataSeeder
{
    private static readonly Guid TrackId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid Module1Id = Guid.Parse("22222222-2222-2222-2222-222222222101");
    private static readonly Guid Module2Id = Guid.Parse("22222222-2222-2222-2222-222222222102");
    private static readonly Guid Module3Id = Guid.Parse("22222222-2222-2222-2222-222222222103");
    private static readonly Guid Exam1Id = Guid.Parse("33333333-3333-3333-3333-333333333101");
    private static readonly Guid CSharpTechId = Guid.Parse("44444444-4444-4444-4444-444444444101");
    private static readonly Guid AspNetTechId = Guid.Parse("44444444-4444-4444-4444-444444444102");

    public static async Task SeedAsync(AppDbContext context, CancellationToken ct = default)
    {
        if (!await context.Tracks.AnyAsync(t => t.Id == TrackId, ct))
        {
            var track = new Track
            {
                Id = TrackId,
                Name = ".NET Backend Engineering",
                Description = "Demo local development track covering C#, ASP.NET Core, and clean architecture.",
                Status = TrackStatus.Published
            };

            var module1 = new Module { Id = Module1Id, TrackId = TrackId, Title = "C# Foundations", Summary = "Core language basics.", Order = 1 };
            var module2 = new Module { Id = Module2Id, TrackId = TrackId, Title = "ASP.NET Core APIs", Summary = "Building REST endpoints.", Order = 2 };
            var module3 = new Module { Id = Module3Id, TrackId = TrackId, Title = "Data Access with EF Core", Summary = "Repositories and migrations.", Order = 3 };

            var lessons = new[]
            {
                new Lesson { Id = Guid.Parse("55555555-5555-5555-5555-555555555101"), ModuleId = Module1Id, Title = "Types and Control Flow", Content = "Introduction to C# types.", Order = 1 },
                new Lesson { Id = Guid.Parse("55555555-5555-5555-5555-555555555102"), ModuleId = Module1Id, Title = "LINQ Basics", Content = "Querying collections cleanly.", Order = 2 },
                new Lesson { Id = Guid.Parse("55555555-5555-5555-5555-555555555201"), ModuleId = Module2Id, Title = "Controllers and Routing", Content = "Attribute routing in ASP.NET Core.", Order = 1 },
                new Lesson { Id = Guid.Parse("55555555-5555-5555-5555-555555555202"), ModuleId = Module2Id, Title = "Middleware Pipeline", Content = "Handling requests and errors.", Order = 2 },
                new Lesson { Id = Guid.Parse("55555555-5555-5555-5555-555555555301"), ModuleId = Module3Id, Title = "DbContext and Entities", Content = "Configuring models with Fluent API.", Order = 1 },
                new Lesson { Id = Guid.Parse("55555555-5555-5555-5555-555555555302"), ModuleId = Module3Id, Title = "Migrations Workflow", Content = "Managing schema changes safely.", Order = 2 }
            };

            var exam = new Exam
            {
                Id = Exam1Id,
                ModuleId = Module1Id,
                Title = "C# Foundations Assessment",
                PassThreshold = 70
            };

            var question = new ExamQuestion
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666101"),
                ExamId = Exam1Id,
                Prompt = "Which keyword defines an immutable DTO type in C#?",
                Order = 1,
                Points = 100
            };

            var options = new[]
            {
                new ExamOption { Id = Guid.Parse("77777777-7777-7777-7777-777777777101"), ExamQuestionId = question.Id, Text = "record", IsCorrect = true },
                new ExamOption { Id = Guid.Parse("77777777-7777-7777-7777-777777777102"), ExamQuestionId = question.Id, Text = "volatile", IsCorrect = false }
            };

            await context.Tracks.AddAsync(track, ct);
            await context.Modules.AddRangeAsync([module1, module2, module3], ct);
            await context.Lessons.AddRangeAsync(lessons, ct);
            await context.Exams.AddAsync(exam, ct);
            await context.ExamQuestions.AddAsync(question, ct);
            await context.ExamOptions.AddRangeAsync(options, ct);
        }

        if (!await context.Technologies.AnyAsync(t => t.Id == CSharpTechId, ct))
        {
            var csharp = new Technology
            {
                Id = CSharpTechId,
                Name = "C#",
                Slug = "csharp",
                Description = "Modern object-oriented language on .NET.",
                Kind = TechnologyKind.Language,
                Order = 1,
                ParentId = null
            };

            var aspnet = new Technology
            {
                Id = AspNetTechId,
                Name = "ASP.NET Core",
                Slug = "aspnet-core",
                Description = "Cross-platform web API framework.",
                Kind = TechnologyKind.Framework,
                Order = 1,
                ParentId = CSharpTechId
            };

            var resources = new[]
            {
                new Resource
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888101"),
                    TechnologyId = CSharpTechId,
                    Title = "Official C# Documentation",
                    Url = "https://learn.microsoft.com/dotnet/csharp/",
                    Language = "en",
                    Kind = ResourceKind.Documentation,
                    Level = ResourceLevel.Beginner,
                    Status = ResourceStatus.Approved
                },
                new Resource
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888102"),
                    TechnologyId = AspNetTechId,
                    Title = "ASP.NET Core Overview",
                    Url = "https://learn.microsoft.com/aspnet/core/",
                    Language = "en",
                    Kind = ResourceKind.Documentation,
                    Level = ResourceLevel.Intermediate,
                    Status = ResourceStatus.Approved
                }
            };

            await context.Technologies.AddRangeAsync([csharp, aspnet], ct);
            await context.Resources.AddRangeAsync(resources, ct);
        }

        await context.SaveChangesAsync(ct);
    }
}
