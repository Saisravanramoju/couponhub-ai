using CouponHub.Api.Contracts.Requests;
using CouponHub.Api.Contracts.Responses;
using CouponHub.Application.Coupons.Commands.CreateCoupon;
using CouponHub.Application.Coupons.Queries.GetCouponById;
using CouponHub.Application.Coupons.Queries.GetCoupons;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CouponHub.Api.Controllers;

[ApiController]
[Route("api/coupons")]
public sealed class CouponsController : ControllerBase
{
    private readonly ISender _sender;

    public CouponsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CouponResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CouponResponse>> Create(
        [FromBody] CreateCouponRequest request,
        CancellationToken cancellationToken)
    {
        var coupon = await _sender.Send(
            new CreateCouponCommand(
                request.BrandId,
                request.CouponCode,
                request.Description,
                request.Category,
                request.DiscountType,
                request.DiscountValue,
                request.MinimumOrderAmount,
                request.MaximumDiscount,
                request.ExpiryDate,
                request.CouponSource),
            cancellationToken);

        var response = CouponResponse.FromEntity(coupon);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CouponResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CouponResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var coupons = await _sender.Send(
            new GetCouponsQuery(),
            cancellationToken);

        return Ok(coupons.Select(CouponResponse.FromEntity));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CouponResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var coupon = await _sender.Send(
            new GetCouponByIdQuery(id),
            cancellationToken);

        return Ok(CouponResponse.FromEntity(coupon));
    }
}