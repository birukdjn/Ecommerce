using Domain.Common;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public List<Guid> CategoryIds { get; init; } = [];
}