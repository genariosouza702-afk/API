using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Infrastructure.Database;

namespace OrderService.Api.Features.Orders;

public static class CreateOrder
{
    // 1. DTO de Entrada (Command)
    public record Command(string CustomerEmail, List<OrderItemDto> Items) : IRequest<Result>;

    public record OrderItemDto(string ProductName, int Quantity, decimal UnitPrice);

    // 2. DTO de Saída
    public record Result(Guid OrderId, decimal TotalAmount, string Status);

    // 3. Validação declarativa com FluentValidation
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.Items).NotEmpty().WithMessage("O pedido deve conter pelo menos 1 item.");
            RuleForEach(x => x.Items).ChildRules(items =>
            {
                items.RuleFor(i => i.Quantity).GreaterThan(0);
                items.RuleFor(i => i.UnitPrice).GreaterThan(0);
            });
        }
    }

    // 4. Handler da Regra de Negócio
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly AppDbContext _db;

        public Handler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            
            var orderId = Guid.NewGuid();

            // Salva no banco (Entity)
            var order = new Order
            {
                Id = orderId,
                CustomerEmail = request.CustomerEmail,
                TotalAmount = totalAmount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(cancellationToken);

            return new Result(order.Id, order.TotalAmount, order.Status);
        }
    }

    // 5. Endpoint Minimal API
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (Command command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/orders/{result.OrderId}", result);
        })
        .WithName("CreateOrder")
        .WithTags("Orders");
    }
}