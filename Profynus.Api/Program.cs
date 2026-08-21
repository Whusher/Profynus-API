using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Profynus.Application.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Profynus.Application.Auth.Commands;
using Profynus.Application.Common.Helpers;
using Profynus.Application.Common.Exception;
using Profynus.Infrastructure.Cache.Context;
using Profynus.Application.Common.RefreshToken;
using Profynus.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Profynus.Application.Common.EncryptionService;
using Profynus.Application.Common.TokenizationService;
using Profynus.Application.Common.TokenizationService.RecordTypes;
using Profynus.Application.YTAudio;
using StackExchange.Redis;
using YoutubeExplode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ── JWT Configuration ─────────────────────────────────────────────────────────────
var jwtSecret  = builder.Configuration["Jwt:Secret"]
                 ?? throw new InvalidOperationException("Jwt:Secret is required.");
var jwtIssuer  = builder.Configuration["Jwt:Issuer"]   ?? "profynus";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "profynus-clients";


// ── Auth services ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton(new TokenConfig(jwtSecret, jwtIssuer, jwtAudience));
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<DeviceResolver>();
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<RefreshTokenDelivery>();
 
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<RegisterHandler>();
builder.Services.AddScoped<RefreshTokenHandler>();


// --- Youtube & HTTP services -----------------------------------------------------------
builder.Services.AddHttpClient("Profynus-Client",
    client =>
    {
        client.Timeout = TimeSpan.FromSeconds(40);
        // User agent must be handled by YouTube Explode dependency
        // client.DefaultRequestHeaders.UserAgent
        //     .ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36");
    }
).AddStandardResilienceHandler();

builder.Services.AddSingleton<YoutubeClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();

    var httpClient = factory.CreateClient("Profynus-Client");
    
    return new YoutubeClient(httpClient);
});




// --- User services -----------------------------------------------------------
builder.Services.AddScoped<UserService>();

// --- Music services -----------------------------------------------------------
builder.Services.AddScoped<YTAudioService>();

// --- Caching services --------------------------------------------------------
builder.Services.AddScoped<CacheService>();

// ── JWT bearer validation ─────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer   = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer      = jwtIssuer,
            ValidAudience    = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

        opt.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"[JWT] Auth failed: {ctx.Exception.GetType().Name}: {ctx.Exception.Message}, Full Exception: {ctx.Exception}");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Console.WriteLine($"[JWT] Token valid for: {ctx.Principal?.FindFirst("sub")?.Value}");
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// ---- Rate Limiting --------------------------------------------------
builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("username-check", o =>
    {
        o.PermitLimit      = 10;
        o.Window           = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit       = 0;
    });
});


builder.Services.AddControllers();

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: eyJhbGci..."
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


 
// ── CORS — tighten for production ─────────────────────────────────────────────
builder.Services.AddCors(opt => opt.AddPolicy("profynus", p =>
    p.WithOrigins(
            "https://app.profynus.com",
            "https://profynus.vercel.app",
            "http://localhost:5173")          // dev
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
));               // required for cross-origin cookie sending


// ── Error handling ────────────────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();



// ---------------------------------------------------- Redis Database connection

// Direct ConnectionMultiplexer (more control)
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisDB")!)
);

// Distributed Cache (simpler, recommended for most APIs)
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("RedisDB");
//     options.InstanceName = "Profynus:";  // Optional key prefix
// });


// ---------------------------------------------------- Database configuration [service injection] 
// Write context — primary database
builder.Services.AddDbContext<MasterDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresMaster"))
        .UseSnakeCaseNamingConvention());
// Read context — can point to a read replica in production
builder.Services.AddDbContext<QueryDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresQuery"))
        .UseSnakeCaseNamingConvention());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Main services
app.UseExceptionHandler();
app.UseCors("profynus");
// app.UseHttpsRedirection();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Core central
app.UseRateLimiter();
app.MapControllers();
app.Run();