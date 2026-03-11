using Application.DTOs.Wallet;
using Domain.Common;
using MediatR;

namespace Application.Features.Wallets.Queries.GetVendorWallet
{
    public record GetVendorWalletQuery : IRequest<Result<VendorWalletDto>>;

}