using Microsoft.EntityFrameworkCore;
using PS.OrderEvents.API.Extensions;
using PS.OrderEvents.Infrastructure;
using PS.OrderEvents.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    typeof(PS.OrderEvents.Application.Orders.Commands.CreateOrder.CreateOrderCommand).Assembly));
builder.Services.AddApplication();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173", "https://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderEventsDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
