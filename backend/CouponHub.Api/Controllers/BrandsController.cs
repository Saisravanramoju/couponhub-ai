using CouponHub.Api.Contracts.Requests;
using CouponHub.Api.Contracts.Responses;
using CouponHub.Application.Brands.Commands.CreateBrand;
using CouponHub.Application.Brands.Queries.GetBrandById;
using CouponHub.Application.Brands.Queries.GetBrands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CouponHub.Api.Controllers;

[ApiController]
[Route("api/brands")]
public sealed class BrandsController : ControllerBase
{
    private readonly ISender _sender;

    public BrandsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrandResponse>> Create(
        [FromBody] CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _sender.Send(
            new CreateBrandCommand(
                request.Name,
                request.Category,
                request.LogoUrl),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = brand.Id },
            BrandResponse.FromEntity(brand));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BrandResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var brands = await _sender.Send(
            new GetBrandsQuery(),
            cancellationToken);

        return Ok(brands.Select(BrandResponse.FromEntity));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var brand = await _sender.Send(
            new GetBrandByIdQuery(id),
            cancellationToken);

        return Ok(BrandResponse.FromEntity(brand));
    }
}