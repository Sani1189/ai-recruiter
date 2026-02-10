using Scalar.AspNetCore;
using Recruiter.WebApi.Infrastructure;
using Recruiter.WebApi.Middleware;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.ElevenLabs;
using Recruiter.Application.ElevenLabs.Interfaces;
using Recruiter.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Recruiter.Application.Common.Options;
using Recruiter.Application.Common.Interfaces;
using Azure.Storage.Blobs;
using Azure.Identity;
using Azure.Core;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Configuration Setup
// ============================================================================
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: false, reloadOnChange: false)
    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{builder.Environment.EnvironmentName.ToLower()}.json"), optional: true, reloadOnChange: false)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// ============================================================================
// Azure Storage Configuration
// ============================================================================
builder.Services.Configure<AzureStorageOptions>(builder.Configuration.GetSection(AzureStorageOptions.SectionName));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value);

// ============================================================================
// Python API Configuration
// ============================================================================
builder.Services.Configure<PythonApiOptions>(builder.Configuration.GetSection(PythonApiOptions.SectionName));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<PythonApiOptions>>().Value);

// ============================================================================
// Azure Credentials Configuration
// ============================================================================
builder.Services.AddSingleton<TokenCredential>(sp =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    var logger = sp.GetRequiredService<ILogger<Program>>();

    // Best practice:
    // - Development: use developer credentials (Azure CLI / Visual Studio / etc.). Managed Identity is not available locally.
    // - Production: use Managed Identity only (fastest + avoids accidental auth paths).
    var credentialOptions = new DefaultAzureCredentialOptions
    {
        Diagnostics = { IsLoggingEnabled = environment.IsDevelopment() },

        // Never use browser prompts in server environments
        ExcludeInteractiveBrowserCredential = true
    };

    if (environment.IsDevelopment())
    {
        // Local dev: don't waste time probing managed identity endpoints
        credentialOptions.ExcludeManagedIdentityCredential = true;
    }
    else
    {
        // Production: lock down to Managed Identity (and optional EnvironmentCredential for emergencies/CI)
        credentialOptions.ExcludeAzureCliCredential = true;
        credentialOptions.ExcludeAzureDeveloperCliCredential = true;
        credentialOptions.ExcludeAzurePowerShellCredential = true;
        credentialOptions.ExcludeVisualStudioCredential = true;
        credentialOptions.ExcludeVisualStudioCodeCredential = true;
        credentialOptions.ExcludeWorkloadIdentityCredential = true;

        // If you want to allow service principal auth in specific environments, set env vars and flip this to false.
        credentialOptions.ExcludeEnvironmentCredential = false;
    }

    var credential = new DefaultAzureCredential(credentialOptions);
    
    if (environment.IsDevelopment())
    {
        logger.LogInformation("Azure: DefaultAzureCredential registered for Development. " +
            "Will use: Environment variables → Visual Studio → Azure CLI → Interactive");
    }
    else
    {
        logger.LogInformation("Azure: DefaultAzureCredential registered for Production. " +
            "Will use Managed Identity from App Service.");
    }
    
    return credential;
});

// ============================================================================
// Authentication Configuration
// ============================================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Azure";
    options.DefaultChallengeScheme = "Azure";
})
.AddJwtBearer("Azure", options =>
{
    var azure = builder.Configuration.GetSection("Azure");
    options.Authority = $"{azure["Instance"]}{azure["TenantId"]}/v2.0";
    options.Audience = azure["Audience"];

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudiences = [azure["Audience"], azure["ClientId"]],
        ValidIssuers =
        [
            $"https://login.microsoftonline.com/{azure["TenantId"]}/v2.0",
            $"https://sts.windows.net/{azure["TenantId"]}/"
        ],
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn",
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        ClockSkew = TimeSpan.FromMinutes(5)
    };

})
.AddJwtBearer("AzureB2C", options =>
{
    var b2c = builder.Configuration.GetSection("AzureB2C");
    var domain = b2c["Domain"]; // osilionrecruitment.ciamlogin.com
    var tenantId = b2c["TenantId"]; // f6e5ae67-...
    var clientId = b2c["ClientId"];

    // CIAM v2 endpoints do not embed the policy in the authority path
    options.Authority = $"https://{domain}/{tenantId}/v2.0";
    options.MetadataAddress = $"https://{domain}/{tenantId}/v2.0/.well-known/openid-configuration?appid={clientId}";

    var expectedIssuerCiam = $"https://{tenantId}.ciamlogin.com/{tenantId}/v2.0";
    var expectedIssuerLogin = $"https://login.microsoftonline.com/{tenantId}/v2.0";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudiences = [b2c["Audience"], clientId],
        ValidIssuers = [expectedIssuerCiam, expectedIssuerLogin],
        NameClaimType = "name",
        RoleClaimType = "roles",
        ClockSkew = TimeSpan.FromMinutes(5)
    };
})
.AddJwtBearer("Google", options =>
{
    var googleAuth = builder.Configuration.GetSection("Google");
    options.Authority = "https://accounts.google.com";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudience = googleAuth["ClientId"],
        NameClaimType = "email",
        RoleClaimType = "roles",
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

// ============================================================================
// Authorization Policies
// ============================================================================
builder.Services.AddAuthorization(options =>
{
    // Default policy should accept any of our configured schemes
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes("Azure", "AzureB2C", "Google")
        .Build();

    // Public API policy - no authentication required
    options.AddPolicy("PublicApi", policy =>
        policy.RequireAssertion(_ => true));

    // Authenticated user policy - any logged-in user
    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser()
               .AddAuthenticationSchemes("Azure", "AzureB2C", "Google"));

    // Azure AD scope-based policy for specific operations
    options.AddPolicy("UserRead", policy =>
        policy.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", "User.Read")
              .AddAuthenticationSchemes("Azure", "AzureB2C"));

    // Role-based policies - clean and intuitive naming
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin", "RecruitmentAdmin")
              .AddAuthenticationSchemes("Azure", "AzureB2C", "Google"));

    options.AddPolicy("Candidate", policy =>
        policy.RequireAssertion(context =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
                return false;

            var roleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            var roleClaims = context.User.Claims.Where(c => c.Type == roleClaimType || c.Type == "roles").ToList();

            // Treat authenticated users without any role claims as Candidate
            if (!roleClaims.Any())
                return true;

            // Or if explicitly has Candidate role
            if (roleClaims.Any(c => c.Value == "Candidate"))
                return true;

            return false;
        })
              .AddAuthenticationSchemes("Azure", "AzureB2C", "Google"));

    // Combined policy for endpoints accessible by both roles
    options.AddPolicy("AdminOrCandidate", policy =>
        policy.RequireAssertion(context =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
                return false;

            var roleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            var roleClaims = context.User.Claims.Where(c => c.Type == roleClaimType || c.Type == "roles").ToList();

            // Allow Admins
            if (roleClaims.Any(c => c.Value == "Admin" || c.Value == "RecruitmentAdmin"))
                return true;

            // Allow Candidates (no roles or explicit Candidate role)
            if (!roleClaims.Any())
                return true;
            if (roleClaims.Any(c => c.Value == "Candidate"))
                return true;

            return false;
        })
              .AddAuthenticationSchemes("Azure", "AzureB2C", "Google"));
});

// ============================================================================
// Database & Infrastructure
// ============================================================================
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// ============================================================================
// Azure Blob Storage Configuration
// ============================================================================
builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var options = sp.GetRequiredService<AzureStorageOptions>();
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var environment = sp.GetRequiredService<IWebHostEnvironment>();

    // Prefer connection string for local development to avoid permission issues
    // Use DefaultAzureCredential (Managed Identity) for production
    if (options.UseConnectionString)
    {
        // Use connection string authentication (for local dev or when explicitly configured)
        logger.LogInformation("Azure Storage: Using connection string authentication");
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException(
                "AzureStorage:ConnectionString is required but is empty. " +
                "Please provide a connection string in appsettings.json or use StorageAccountName with Managed Identity.");
        }
        return new BlobServiceClient(options.ConnectionString);
    }
    else
    {
        // Use DefaultAzureCredential from DI (Managed Identity) - recommended for production
        if (string.IsNullOrWhiteSpace(options.StorageAccountName))
        {
            throw new InvalidOperationException(
                "AzureStorage:StorageAccountName is required when ConnectionString is not provided. " +
                "Please configure the storage account name in appsettings.json");
        }

        var storageAccountUrl = $"https://{options.StorageAccountName}.blob.core.windows.net";
        var credential = sp.GetRequiredService<TokenCredential>();
        
        if (environment.IsDevelopment())
        {
            logger.LogWarning("Azure Storage: Using DefaultAzureCredential in Development. " +
                "If you encounter permission errors, use ConnectionString instead. " +
                "Account: {StorageAccount}", options.StorageAccountName);
        }
        else
        {
            logger.LogInformation("Azure Storage: Using DefaultAzureCredential (Managed Identity) for account: {StorageAccount}", 
                options.StorageAccountName);
        }

        return new BlobServiceClient(new Uri(storageAccountUrl), credential);
    }
});

// ============================================================================
// Application Services
// ============================================================================
builder.Services.AddApplicationServices();
builder.Services.AddAutoMapperProfiles();
builder.Services.AddValidation();
builder.Services.AddControllers()
    .AddJsonOptions(static options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddMemoryCache();

// Configure ElevenLabs HTTP Client
builder.Services
    .AddOptions<ElevenLabsOptions>()
    .Bind(builder.Configuration.GetSection(ElevenLabsOptions.SectionName))
    .ValidateDataAnnotations();
builder.Services.AddHttpClient<IElevenLabsService, ElevenLabsService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ElevenLabsOptions>>().Value;

    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("ElevenLabs BaseUrl is not configured.");
    }

    client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/')
        ? options.BaseUrl
        : options.BaseUrl + "/");

    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

    if (!string.IsNullOrWhiteSpace(options.ApiKey))
    {
        client.DefaultRequestHeaders.Remove("xi-api-key");
        client.DefaultRequestHeaders.Add("xi-api-key", options.ApiKey);
    }

    client.Timeout = TimeSpan.FromSeconds(90);
});

// ============================================================================
// Python API Authentication Services
// ============================================================================
builder.Services.AddPythonApiAuthentication();

// ============================================================================
// Configure Python API HTTP Client (CV Processing) with Authentication
// ============================================================================
builder.Services
    .AddHttpClient<Recruiter.Application.CvProcessing.Interfaces.ICvProcessingService, Recruiter.Application.CvProcessing.Services.CvProcessingService>((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<PythonApiOptions>>().Value;
        
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new InvalidOperationException("PythonApi:BaseUrl is not configured in appsettings.json");
        }

        client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    })
    .AddHttpMessageHandler<PythonApiAuthenticationHandler>();

// ============================================================================
// Configure Python API HTTP Client (Webhook Proxy)
// ============================================================================
builder.Services.AddHttpClient("PythonWebhookProxy", (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<PythonApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// ============================================================================
// API Documentation & CORS
// ============================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://localhost:3001") // your frontend URL
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // if you use cookies or auth headers
        });
});


// ============================================================================
// Build Application
// ============================================================================
var app = builder.Build();

// ============================================================================
// Middleware Pipeline
// ============================================================================
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// ============================================================================
// Database Initialization
// ============================================================================
await app.EnsureDatabaseAsync();

// ============================================================================
// API Documentation (Development Only)
// ============================================================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ============================================================================
// HTTP Pipeline
// ============================================================================
app.UseHttpsRedirection();
app.UseGlobalExceptionHandler();

// ============================================================================
// Endpoints
// ============================================================================
app.MapGet("/", () => "Hello world!");
app.MapGet("/api/auth-test", () => "Hello world!").RequireAuthorization();
app.MapControllers().RequireAuthorization();

// ============================================================================
// Run Application
// ============================================================================
app.Run();