using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Osint.Source.One.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<CarDbContext>(opt => opt.UseInMemoryDatabase("database"));
builder.Services.AddSingleton(new ActivitySource("Osint.Source.One", "0.0.1"));

var app = builder.Build();

app.MapControllers();
await DbInit(app);

await app.RunAsync();

async Task DbInit(IHost host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider.GetRequiredService<CarDbContext>();
    await services.Cars.AddAsync(new CarEntity { Owner = "CarOwnerName1", Plate = "12345" });
    await services.Cars.AddAsync(new CarEntity { Owner = "CarOwnerName2", Plate = "678910" });
    await services.SaveChangesAsync();
}