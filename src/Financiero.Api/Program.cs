using System.Text;
using Financiero.Api;
using Financiero.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, _, log) => log.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Financiero.Api").Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(new JsonFormatter()).WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));
builder.Services.AddHealthChecks();
builder.Services.AddFinancialInfrastructure(builder.Configuration);

var authority = builder.Configuration["Jwt:Authority"] ?? builder.Configuration["JWT_AUTHORITY"];
var audience = builder.Configuration["Jwt:Audience"]
    ?? builder.Configuration["JWT_AUDIENCE"]
    ?? "portal-corporativo-clients";
var requireHttpsMetadataValue = builder.Configuration["Jwt:RequireHttpsMetadata"]
    ?? builder.Configuration["JWT_REQUIRE_HTTPS_METADATA"];
var requireHttpsMetadata = !bool.TryParse(requireHttpsMetadataValue, out var parsedRequireHttpsMetadata)
    || parsedRequireHttpsMetadata;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    if (!string.IsNullOrWhiteSpace(authority))
    {
        options.Authority = authority.Trim();
        options.Audience = audience;
        options.RequireHttpsMetadata = requireHttpsMetadata;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true, ValidateAudience = true, ValidAudience = audience,
            ValidateIssuerSigningKey = true, ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(1)
        };
        return;
    }

    var secret = builder.Configuration["Jwt:Secret"]
        ?? builder.Configuration["JWT_SECRET"]
        ?? throw new InvalidOperationException("Configure Jwt:Authority/JWT_AUTHORITY for OIDC or provide Jwt:Secret/JWT_SECRET for local JWT mode.");
    var issuer = builder.Configuration["Jwt:Issuer"]
        ?? builder.Configuration["JWT_ISSUER"]
        ?? "portal-corporativo";
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true, ValidIssuer = issuer, ValidateAudience = true,
        ValidAudience = audience, ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});
builder.Services.AddFinancialRuntimeAuthorization();

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = x => x.Tags.Contains("ready") }).AllowAnonymous();
app.MapHealthChecks("/health/sri", new HealthCheckOptions { Predicate = x => x.Tags.Contains("sri") }).AllowAnonymous();
app.MapHealthChecks("/health/content-file", new HealthCheckOptions { Predicate = x => x.Tags.Contains("content-file") }).AllowAnonymous();
app.MapGet("/", () => Results.Ok(new { service = "Financiero.Api", status = "bootstrap" })).AllowAnonymous();
app.MapChartOfAccounts();
app.MapFiscalPeriods();
app.MapJournalEntries();
app.MapElectronicDocuments();
app.MapTaxReporting();
app.MapTaxLegalReview();
app.MapPurchaseTaxDocuments();
app.MapFinancialTaxCatalogs();
app.MapExternalApprovals();
app.MapPortalIntegration();
app.Run();

public partial class Program;
