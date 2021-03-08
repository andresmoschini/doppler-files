using DopplerFiles.DopplerSecurity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DopplerSecurityServiceCollectionExtensions
    {
        public static IServiceCollection AddDopplerSecurity(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, IsOwnResourceAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, IsSuperUserAuthorizationHandler>();
            services.ConfigureOptions<ConfigureDopplerSecurityOptions>();

            IEnumerable<SecurityKey> issuerSigningKeys = new List<SecurityKey>();
            bool skipLifetimeValidation = false;

            services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<DopplerSecurityOptions>>((o, securityOptions) =>
                {
                    issuerSigningKeys = securityOptions.Value.SigningKeys;
                    skipLifetimeValidation = !securityOptions.Value.SkipLifetimeValidation;
                });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKeys = issuerSigningKeys,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = skipLifetimeValidation,
                    ValidateAudience = false,
                    ValidateIssuer = false
                };
            });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .AddRequirements(new DopplerAuthorizationRequirement
                    {
                        AllowSuperUser = true,
                        AllowOwnResource = true
                    })
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }
    }
}
