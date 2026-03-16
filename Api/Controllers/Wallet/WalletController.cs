using Api.Controllers.Common;
using Application.Features.Wallets.Commands.RequestPayout;
using Application.Features.Wallets.Queries.GetVendorWallet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Wallet
{
    [Authorize(Policy = "VendorOnly")]
    [Route("api/vendor/wallet")]
    public class WalletController(ISender mediator) : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetWallet()
            => HandleResult(await mediator.Send(new GetVendorWalletQuery()));

        [HttpPost("payouts")]
        public async Task<ActionResult> RequestPayout([FromBody] RequestPayoutCommand command)
            => HandleResult(await mediator.Send(command));

    }
}