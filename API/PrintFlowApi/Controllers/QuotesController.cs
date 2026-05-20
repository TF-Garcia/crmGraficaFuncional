using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Data;
using PrintFlowApi.DTOs;
using PrintFlowApi.Services;

namespace PrintFlowApi.Controllers;

[ApiController]
[Route("api/orcamentos")]
public class QuotesController(PrintFlowDbContext db, QuoteService quoteService) : ControllerBase
{
    [HttpPost("calcular")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<QuoteResponse>> Calculate(QuoteRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .Include(item => item.Options)
            .FirstOrDefaultAsync(item => item.Id == request.ProductId && item.Active, cancellationToken);

        if (product is null)
        {
            return NotFound(new { message = "Produto nao encontrado." });
        }

        try
        {
            return Ok(quoteService.Calculate(product, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
