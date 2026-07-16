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

var secret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret must be supplied through environment or secret storage.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new()
{
    ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidateAudience = true,
    ValidAudience = builder.Configuration["Jwt:Audience"], ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(1)
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
app.Run();

public partial class Program;
