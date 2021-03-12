using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StorageProviders;

namespace DopplerFiles.Test
{
    public class FakeAwsS3StorageProvider : AwsS3StorageProvider
    {
        private readonly IAmazonS3 _amazonS3;
        public FakeAwsS3StorageProvider(IOptions<AwsS3Settings> awsS3settings,
            IOptions<ServiceSettings> serviceSettings,
            ILogger<AwsS3StorageProvider> logger, IAmazonS3 amazonS3) : base(awsS3settings, serviceSettings, logger)
        {
            _amazonS3 = amazonS3;
        }

        protected override IAmazonS3 GetClient()
        {
            return _amazonS3;
        }
    }
}
