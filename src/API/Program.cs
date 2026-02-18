using System.Text;
using AspNetCoreRateLimit;
using DirectPayGateway.API.Middleware;
using DirectPayGateway.Core.Constants;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Infrastructure;
using DirectPayGateway.Infrastructure.Data;
using DirectPayGateway.Infrastructure.Data.Seeding;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

static async Task EnsureDonationReferencesTableAsync(ApplicationDbContext context)
{
    if (!context.Database.IsSqlite()) return;
    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS DonationReferences (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ReferenceNumber TEXT NOT NULL,
            CampaignId INTEGER NOT NULL,
            IntendedAmount decimal(18,3),
            PayerEmail TEXT,
            Status TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            ExpiresAt TEXT NOT NULL,
            DonationId INTEGER,
            FOREIGN KEY (CampaignId) REFERENCES Campaigns(Id),
            FOREIGN KEY (DonationId) REFERENCES Donations(Id)
        )");
    await context.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_DonationReferences_ReferenceNumber ON DonationReferences(ReferenceNumber)");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_DonationReferences_CampaignId ON DonationReferences(CampaignId)");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_DonationReferences_Status ON DonationReferences(Status)");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_DonationReferences_ExpiresAt ON DonationReferences(ExpiresAt)");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_DonationReferences_DonationId ON DonationReferences(DonationId)");
}

static async Task EnsureCtmApiLogsTableAsync(ApplicationDbContext context)
{
    if (!context.Database.IsSqlite()) return;
    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS CtmApiLogs (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Timestamp TEXT NOT NULL DEFAULT (datetime('now')),
            Endpoint TEXT NOT NULL,
            HttpMethod TEXT,
            RequestBody TEXT,
            ResponseBody TEXT,
            HttpStatusCode INTEGER NOT NULL DEFAULT 0,
            BillingNo TEXT,
            JOEBPPSTrx TEXT,
            ErrorMessage TEXT,
            ClientIp TEXT,
            UserAgent TEXT,
            ResponseTimeMs INTEGER NOT NULL DEFAULT 0
        )");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_CtmApiLogs_Timestamp ON CtmApiLogs(Timestamp)");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_CtmApiLogs_Endpoint ON CtmApiLogs(Endpoint)");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_CtmApiLogs_JOEBPPSTrx ON CtmApiLogs(JOEBPPSTrx)");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_CtmApiLogs_BillingNo ON CtmApiLogs(BillingNo)");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DirectPay Gateway API",
        Version = "v1",
        Description = "eFAWATEERcom DirectPay Payment Gateway + CTM JSON Integration"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Description = "Basic Authentication for CTM endpoints. Enter your credentials.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "basic"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSecret = builder.Configuration["JwtSettings:Secret"]
        ?? throw new InvalidOperationException("JWT Secret not configured");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
})
.AddScheme<AuthenticationSchemeOptions, CtmBasicAuthHandler>("CtmBasic", null);

builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();

builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        // Ensure tables exist (for existing DBs created before these features)
        await EnsureDonationReferencesTableAsync(context);
        await EnsureCtmApiLogsTableAsync(context);

        // Seed roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed admin user
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = "admin@admin.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                Console.WriteLine("Admin user created: admin@admin.com / Admin123!");
            }
        }

        // Seed campaigns from JSON file
        var campaignSeeder = services.GetRequiredService<CampaignSeeder>();
        await campaignSeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DirectPay Gateway API v1");
    });
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseIpRateLimiting();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
