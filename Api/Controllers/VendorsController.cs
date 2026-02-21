using Api.Controllers.Requests;
using Application.Common.Interfaces;
using Application.Features.Vendors.Commands.BecomeVendor;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VendorsController(IMediator mediator, IFileService fileService) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly IFileService _fileService = fileService;

        [HttpPost("apply")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BecomeVendor([FromForm] BecomeVendorRequest request)
        {
            string logoUrl = await _fileService.UploadFileAsync(request.LogoFile, "logos");
            string licenseUrl = await _fileService.UploadFileAsync(request.LicenseFile, "licenses");

            var command = new BecomeVendorCommand
            {
                StoreName = request.StoreName,
                Description = request.Description,
                LogoUrl = logoUrl,
                LicenseUrl = licenseUrl,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _fileService.DeleteFile(logoUrl);
                _fileService.DeleteFile(licenseUrl);
                return BadRequest(result);
            }

            return Ok(result);
            
        }
    }
}