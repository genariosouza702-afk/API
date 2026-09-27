using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Features.Orders;
using OrderService.Api.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

// DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));

// Registra o MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Registra os validadores do FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Mapeia o endpoint da feature CreateOrder
CreateOrder.MapEndpoint(app);

app.Run();