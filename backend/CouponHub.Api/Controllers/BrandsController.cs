using CouponHub.Api.Contracts.Requests;
using CouponHub.Api.Contracts.Responses;
using CouponHub.Application.Brands.Commands.CreateBrand;
using CouponHub.Application.Brands.Queries.GetBrandById;
using CouponHub.Application.Brands.Queries.GetBrands;
using Microsoft.AspNetCore.Mvc;

namespace CouponHub.Api.Controllers;

[ApiController]
[Route("api/brands")]
public sealed class BrandsController : ControllerBase
{
    private readonly CreateBrandCommandHandler _createBrandHandler;
    private readonly GetBrandByIdQueryHandler _getBrandByIdHandler;
    private readonly GetBrandsQueryHandler _getBrandsHandler;

    public BrandsController(
        CreateBrandCommandHandler createBrandHandler,
        GetBrandByIdQueryHandler getBrandByIdHandler,
        GetBrandsQueryHandler getBrandsHandler)
    {
        _createBrandHandler = createBrandHandler;
        _getBrandByIdHandler = getBrandByIdHandler;
        _getBrandsHandler = getBrandsHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrandResponse>> Create(
        [FromBody] CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _createBrandHandler.Handle(
            new CreateBrandCommand(
                request.Name,
                request.Category,
                request.LogoUrl),
            cancellationToken);

        var response = BrandResponse.FromEntity(brand);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BrandResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var brands = await _getBrandsHandler.Handle(
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
        var brand = await _getBrandByIdHandler.Handle(
            new GetBrandByIdQuery(id),
            cancellationToken);

        return Ok(BrandResponse.FromEntity(brand));
    }
}