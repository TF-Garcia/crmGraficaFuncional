using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Data;
using PrintFlowApi.DTOs;
using PrintFlowApi.Model;

namespace PrintFlowApi.Controllers;

[ApiController]
[Route("api/catalogo")]
public class CatalogController(PrintFlowDbContext db) : ControllerBase
{
    [HttpGet("produtos")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await db.Products
            .AsNoTracking()
            .Include(product => product.Options)
            .Include(product => product.Quantities)
            .Where(product => product.Active)
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return Ok(products.Select(ToResponse).ToList());
    }

    [HttpGet("produtos/{slug}")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<ProductResponse>> GetProduct(string slug, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .Include(item => item.Options)
            .Include(item => item.Quantities)
            .FirstOrDefaultAsync(item => item.Slug == slug && item.Active, cancellationToken);

        return product is null ? NotFound(new { message = "Produto nao encontrado." }) : Ok(ToResponse(product));
    }

    public static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Slug,
            product.Name,
            product.Category,
            product.Description,
            product.ImageUrl,
            product.BasePrice,
            product.BaseDeadlineDays,
            product.AllowUpload,
            product.AllowPickupPayment,
            product.Active,
            product.Quantities.OrderBy(item => item.Quantity).Select(item => item.Quantity).ToList(),
            Options(product, "size"),
            Options(product, "material"),
            Options(product, "printMode"),
            Options(product, "finishing"));
    }

    private static List<ProductOptionResponse> Options(Product product, string type)
    {
        return product.Options
            .Where(option => option.Type == type)
            .Select(option => new ProductOptionResponse(option.Name, option.PriceDelta, option.DeadlineDeltaDays))
            .ToList();
    }
}
