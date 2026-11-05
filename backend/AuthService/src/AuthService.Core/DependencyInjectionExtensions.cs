using AuthService.Core.Messaging.Publishers;
using AuthService.Core.Services;
using AuthService.Domain.Accounts;
using AuthService.Domain.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Authentication.Authentication;
using Shared.Core.Abstractions;
using AppIdentityOptions = AuthService.Core.Configuration.IdentityOptions;

namespace AuthService.Core;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCore(IConfiguration configuration)
        {
            services.AddHandlers(typeof(DependencyInjectionExtensions).Assembly);
            services.AddValidatorsFromAssembly(typeof(DependencyInjectionExtensions).Assembly);

            services.AddScoped<PlatformSyncService>();

            services.AddIdentityServices(configuration);

            services.AddScoped<IAccountRegisteredEventPublisher, AccountRegisteredEventPublisher>();
            services.AddScoped<IAccountDeletedEventPublisher, AccountDeletedEventPublisher>();
            services.AddScoped<IAccountBlockedEventPublisher, AccountBlockedEventPublisher>();

            return services;
        }

        private IServiceCollection AddIdentityServices(IConfiguration configuration)
        {
            var identitySection = configuration.GetSection(AppIdentityOptions.SECTION_NAME);
            services.Configure<AppIdentityOptions>(identitySection);

            AppIdentityOptions settings = identitySection.Get<AppIdentityOptions>() ?? new AppIdentityOptions();

            services.AddIdentity<Account, Role>(options =>
            {
                options.Password.RequiredLength = settings.Password.RequiredLength;
                options.Password.RequireDigit = settings.Password.RequireDigit;
                options.Password.RequireLowercase = settings.Password.RequireLowercase;
                options.Password.RequireUppercase = settings.Password.RequireUppercase;
                options.Password.RequireNonAlphanumeric = settings.Password.RequireNonAlphanumeric;

                options.Lockout.MaxFailedAccessAttempts = settings.Lockout.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = settings.Lockout.DefaultLockoutTimeSpan;

                options.User.RequireUniqueEmail = settings.User.RequireUniqueEmail;

                options.SignIn.RequireConfirmedEmail = settings.SignIn.RequireConfirmedEmail;

                options.ClaimsIdentity.UserIdClaimType = AuthClaimTypes.SUB;
                options.ClaimsIdentity.UserNameClaimType = AuthClaimTypes.NAME;
                options.ClaimsIdentity.EmailClaimType = AuthClaimTypes.EMAIL;
                options.ClaimsIdentity.RoleClaimType = AuthClaimTypes.ROLE;
                options.ClaimsIdentity.SecurityStampClaimType = AuthClaimTypes.SECURITY_STAMP;
            }).AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

            return services;
        }
    }
}
