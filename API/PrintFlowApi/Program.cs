using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrintFlowApi.Data;
using PrintFlowApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection com uma conexao SQLite.");
}

builder.Services.AddDbContext<PrintFlowDbContext>(options =>
{
    EnsureSqliteDirectory(connectionString);
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<QuoteService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<SecurityService>();
builder.Services.AddScoped<MercadoPagoPaymentService>();

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException("Configure Jwt:Secret com pelo menos 32 caracteres.");
    }

    jwtSecret = "dev-secret-change-me-dev-secret-change-me";
}

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException("Configure Jwt:Secret com pelo menos 32 caracteres.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? ["http://localhost:5173", "http://127.0.0.1:5173"];

        policy
            .WithOrigins(origins.Where(origin => origin != "*").ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = 8,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
    }));
    options.AddPolicy("write", context => RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = 30,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
    }));
    options.AddPolicy("public-read", context => RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = 160,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
    }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await DatabaseSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    name = "PrintFlow API",
    status = "online",
    message = "API online. Use /api/catalogo/produtos, /api/orcamentos/calcular e demais rotas /api."
}));

app.MapControllers();
app.Run();

static string ClientKey(HttpContext context)
{
    var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    return string.IsNullOrWhiteSpace(forwardedFor)
        ? context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        : forwardedFor.Split(',')[0].Trim();
}

static void EnsureSqliteDirectory(string connectionString)
{
    const string prefix = "Data Source=";
    var dataSource = connectionString
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .FirstOrDefault(part => part.TrimStart().StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    if (dataSource is null)
    {
        return;
    }

    var path = dataSource[(dataSource.IndexOf('=') + 1)..].Trim();
    if (string.IsNullOrWhiteSpace(path) || path.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    var directory = Path.GetDirectoryName(Path.GetFullPath(path));
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }
}
