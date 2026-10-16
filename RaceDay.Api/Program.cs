
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Register services.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RaceDayConnection")));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

// Enable Swagger in development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseAuthorization();

app.MapControllers();

// Check the database connection.
app.MapGet("/api/health/database", async (RaceDayContext db) =>
{
    try
    {
        bool connected = await db.Database.CanConnectAsync();

        return connected
            ? Results.Ok(new { status = "Connected" })
            : Results.Problem("Could not connect to the database.");
    }
    catch (Exception ex)
    {
        string detail;
        return Results.Problem(
            detail = ex.Message,
"Database connection failed");
    }
});

app.Run();
