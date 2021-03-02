using DopplerFiles.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using StorageProviders;
using System.Security.Cryptography;

namespace DopplerFiles
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AwsS3Settings>(Configuration.GetSection(nameof(AwsS3Settings)));
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));
            services.AddSingleton<IAuthorizationHandler, IsDopplerFilesUserHandler>();
            services.AddOptions();
            var signingCredentials = GetPublicKeyFromPemFile();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingCredentials.Key,
                ValidateIssuer = false,
                ValidateAudience = false
            };

        });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(IsDopplerFilesUserRequirement), policy =>
                    policy.Requirements.Add(new IsDopplerFilesUserRequirement()));
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private SigningCredentials GetPublicKeyFromPemFile()
        {
            var section = Configuration
                .GetSection(nameof(Authorization.AuthorizationOptions));
            var rsaKeys = section
                .Get<Authorization.AuthorizationOptions>().RsaPublicKey;

            using var rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.FromXmlString(rsaKeys);
            var rsaParameters = rsaProvider.ExportParameters(false);
            var key = new RsaSecurityKey(RSA.Create(rsaParameters));

            return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        }
    }
}
