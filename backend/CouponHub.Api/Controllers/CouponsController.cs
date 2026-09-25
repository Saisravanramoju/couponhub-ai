using CouponHub.Api.Contracts.Requests;
using CouponHub.Api.Contracts.Responses;
using CouponHub.Application.Coupons.Commands.CreateCoupon;
using CouponHub.Application.Coupons.Queries.GetCouponById;
using CouponHub.Application.Coupons.Queries.GetCoupons;
using MediatR;
using CouponHub.Application.Personalization;
using CouponHub.Domain.ValueObjects;
using CouponHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CouponHub.Api.Controllers;

[ApiController]
[Route("api/coupons")]
public sealed class CouponsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _user;

    public CouponsController(ISender sender, ApplicationDbContext db, ICurrentUser user)
    {
        _sender = sender; _db = db; _user = user;
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CreateCouponRequest request, CancellationToken ct)
    {
        var coupon = await _db.Coupons.Include(c => c.Brand).SingleOrDefaultAsync(c => c.Id == id, ct);
        if (coupon is null) return NotFound();
        if (coupon.OwnerId != _user.Id && !_user.IsAdmin) return Forbid();
        if (request.BrandId != coupon.BrandId) return BadRequest(new { message = "Brand cannot be changed." });
        coupon.Update(new CouponDetails(request.CouponCode, request.Description, request.Category,
            request.DiscountType, request.DiscountValue, request.MinimumOrderAmount,
            request.MaximumDiscount, request.ExpiryDate, request.CouponSource));
        await _db.SaveChangesAsync(ct);
        return Ok(CouponResponse.FromEntity(coupon));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var coupon = await _db.Coupons.SingleOrDefaultAsync(c => c.Id == id, ct);
        if (coupon is null) return NotFound();
        if (coupon.OwnerId != _user.Id && !_user.IsAdmin) return Forbid();
        _db.Remove(coupon);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var coupon = await _db.Coupons.SingleOrDefaultAsync(c => c.Id == id, ct);
        if (coupon is null) return NotFound();
        coupon.Publish();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
