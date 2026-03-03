using Domain.Common;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct
{
    public record CreateProductCommand : IRequest<Result<Guid>>
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public int StockQuantity { get; init; }
        public List<Guid> CategoryIds { get; init; } = [];
        public List<string> ImageUrls { get; init; } = [];
    }
}