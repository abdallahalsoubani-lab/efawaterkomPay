using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Core.Services;
using DirectPayGateway.Infrastructure.Data;
using DirectPayGateway.Infrastructure.Data.Seeding;
using DirectPayGateway.Infrastructure.ExternalServices;
using DirectPayGateway.Infrastructure.Repositories;
using DirectPayGateway.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectPayGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        
        if (connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) || 
            (connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) && !connectionString.Contains("Server", StringComparison.OrdinalIgnoreCase)))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            options.User.RequireUniqueEmail = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<DirectPaySettings>(
            configuration.GetSection("DirectPay"));

        // Existing repositories and services
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IDirectPayService, DirectPayService>();

        services.AddScoped<PaymentService>();
        services.AddScoped<AuthService>();

        // CTM Integration repositories and services
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IDonationRepository, DonationRepository>();
        services.AddScoped<ICtmAuditLogRepository, CtmAuditLogRepository>();
        services.AddScoped<IDonationReferenceRepository, DonationReferenceRepository>();
        services.AddScoped<IDonationReferenceService, DonationReferenceService>();
        services.AddScoped<ICtmBillerService, CtmBillerService>();
        services.AddScoped<CampaignSeeder>();

        return services;
    }
}
