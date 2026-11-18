using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TalebElm.Api.Middlewares;
using TalebElm.Application;
using TalebElm.Application.DTOs;
using TalebElm.Infrastructure;
using TalebElm.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Include XML documentation for the API project.
    var apiXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);

    if (File.Exists(apiXmlPath))
    {
        options.IncludeXmlComments(apiXmlPath);
    }

    // Include XML documentation for the Application project DTOs.
    var applicationAssembly = typeof(CreateTrackRequest).Assembly;
    var applicationXmlFile = $"{applicationAssembly.GetName().Name}.xml";
    var applicationXmlPath = Path.Combine(
        AppContext.BaseDirectory,
        applicationXmlFile);

    if (File.Exists(applicationXmlPath))
    {
        options.IncludeXmlComments(applicationXmlPath);
    }

    // Configure JWT Bearer authentication support in Swagger UI.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Enable Swagger only in the Development environment.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DemoDataSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
