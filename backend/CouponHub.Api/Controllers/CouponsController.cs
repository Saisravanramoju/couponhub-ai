using CouponHub.Api.Contracts.Requests;
using CouponHub.Api.Contracts.Responses;
using CouponHub.Application.Coupons.Commands.CreateCoupon;
using CouponHub.Application.Coupons.Queries.GetCouponById;
using CouponHub.Application.Coupons.Queries.GetCoupons;
using Microsoft.AspNetCore.Mvc;

namespace CouponHub.Api.Controllers;

[ApiController]
[Route("api/coupons")]
public sealed class CouponsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CouponResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CouponResponse>> Create(
        CreateCouponRequest request,
        [FromServices] CreateCouponCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new CreateCouponCommand(
            request.BrandId,
            request.CouponCode,
            request.Description,
            request.Category,
            request.DiscountType,
            request.DiscountValue,
            request.MinimumOrderAmount,
            request.MaximumDiscount,
            request.ExpiryDate,
            request.Source), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ApiErrorResponse(
                StatusCodes.Status400BadRequest,
                "Coupon creation failed.",
                result.Error!,
                HttpContext.TraceIdentifier));
        }

        var response = CouponResponse.FromEntity(result.Value!);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CouponResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CouponResponse>>> GetAll(
        [FromServices] GetCouponsQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var coupons = await handler.Handle(new GetCouponsQuery(), cancellationToken);
        return Ok(coupons.Select(CouponResponse.FromEntity));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CouponResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponResponse>> GetById(
        Guid id,
        [FromServices] GetCouponByIdQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var coupon = await handler.Handle(new GetCouponByIdQuery(id), cancellationToken);

        return coupon is null
            ? NotFound(new ApiErrorResponse(
                StatusCodes.Status404NotFound,
                "Coupon not found.",
                "No coupon exists with the specified identifier.",
                HttpContext.TraceIdentifier))
            : Ok(CouponResponse.FromEntity(coupon));
    }
}
