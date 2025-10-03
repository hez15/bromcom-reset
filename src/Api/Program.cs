
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Azure.Identity;
using BromcomReset.Api.Services;
using BromcomReset.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

// Binding config
var azureAd = builder.Configuration.GetSection("AzureAd");
var allowedGroup = builder.Configuration.GetValue<string>("Reset:AllowedGroupObjectId") ?? string.Empty;

// AuthN + AuthZ (JWT via Entra ID)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.ValidAudience = builder.Configuration["AzureAd:Audience"];
    },
    options => { builder.Configuration.Bind("AzureAd", options); });

builder.Services.AddAuthorization(options =>
{
    AuthorizationExtensions.AddGroupPolicy(options, allowedGroup);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Graph client (App-only with client credentials)
builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var tenantId = builder.Configuration["AzureAd:TenantId"];
    var clientId = builder.Configuration["AzureAd:ClientId"];
    var clientSecret = builder.Configuration["AzureAd:ClientSecret"];

    if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        throw new InvalidOperationException("AzureAd TenantId/ClientId/ClientSecret must be configured.");

    var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
    return new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
});

// Services
builder.Services.AddSingleton<AuditLogger>();
builder.Services.AddScoped<IPasswordResetService, GraphPasswordResetService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// health
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
