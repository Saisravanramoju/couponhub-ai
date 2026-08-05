using CouponHub.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CouponHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Success = true,
            Message = "CouponHub API is running."
        });
    }

    [HttpGet("error")]
    public IActionResult ThrowError()
    {
        throw new DomainException("This is a test DomainException.");
    }
}