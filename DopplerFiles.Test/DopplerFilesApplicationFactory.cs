using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace DopplerFiles.Test
{
    public class DopplerFilesApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly string _environmentName;

        public DopplerFilesApplicationFactory(string environmentName)
        {
            _environmentName = environmentName;
        }

        protected override IHostBuilder CreateHostBuilder() => base
            .CreateHostBuilder()
            .UseEnvironment(_environmentName)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>()
                {
                    ["PublicKeysFolder"] = "public-keys-dev"
                });
            });
    }

    public class ProductionEnvironmentDopplerFilesApplicationFactory : DopplerFilesApplicationFactory
    {
        public ProductionEnvironmentDopplerFilesApplicationFactory()
            : base("Production")
        { }
    }
}
