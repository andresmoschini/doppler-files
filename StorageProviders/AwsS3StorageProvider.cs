using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StorageProviders
{
    public class AwsS3StorageProvider : IStorageProvider
    {
        private readonly AwsS3Settings _awsS3Settings;
        private readonly ServiceSettings _serviceSettings;
        private readonly ILogger<AwsS3StorageProvider> _logger;
        private readonly Dictionary<string, RegionEndpoint> _regionEndpoints = new Dictionary<string, RegionEndpoint>
        {
            {"USEast1",  RegionEndpoint.USEast1},
            {"USWest1",  RegionEndpoint.USWest1},
            {"USWest2",  RegionEndpoint.USWest2},
            {"EUWest1",  RegionEndpoint.EUWest1},
            {"EUCentral1",  RegionEndpoint.EUCentral1},
            {"APNortheast1",  RegionEndpoint.APNortheast1},
            {"APSoutheast1",  RegionEndpoint.APSoutheast1},
            {"APSoutheast2",  RegionEndpoint.APSoutheast2},
            {"SAEast1",  RegionEndpoint.SAEast1},
            {"USGovCloudWest1",  RegionEndpoint.USGovCloudWest1},
            {"CNNorth1",  RegionEndpoint.CNNorth1}
        };

        public AwsS3StorageProvider(IOptions<AwsS3Settings> awsS3settings, IOptions<ServiceSettings> serviceSettings, ILogger<AwsS3StorageProvider> logger)
        {
            _awsS3Settings = awsS3settings.Value;
            _serviceSettings = serviceSettings.Value;
            _logger = logger;
        }

        protected virtual IAmazonS3 GetClient()
        {
            return  new AmazonS3Client(_awsS3Settings.AccountInfo.AccessKey, _awsS3Settings.AccountInfo.SecretAccessKey, _regionEndpoints[_awsS3Settings.AccountInfo.RegionEndpoint]);
        }

        public async Task<UploadFileResult> UploadFile(string pathFile, string idUser, byte[] content, bool overwrite = false)
        {
            var request = new PutObjectRequest
            {
                InputStream = new MemoryStream(content),
                BucketName = _awsS3Settings.BucketName,
                Key = $"/{_serviceSettings.UsersRootFolder}/{idUser}{pathFile}"
            };

            using var client = GetClient();

            var getObjectResponse = await client.GetObjectAsync(_awsS3Settings.BucketName, request.Key);

            var existingFile = getObjectResponse != null && getObjectResponse.Key.Equals(request.Key);

            if (!existingFile || (existingFile && overwrite))
            {
                await client.PutObjectAsync(request);
            }
            else
            {
                return new UploadFileResult { PathFile = request.Key, StorageProviderError = StorageProviderError.FileAlreadyExist };
            }

            _logger.LogTrace($"AWS call: Method = 'PutObjectAsync', BucketName = '{request.BucketName}', Key = '{request.Key}'");

            return new UploadFileResult { PathFile = request.Key, StorageProviderError = StorageProviderError.None };
        }

        public string GetDownloadUrl(string keyFile)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _awsS3Settings.BucketName,
                Key = keyFile,
                Expires = DateTime.UtcNow.AddSeconds(_serviceSettings.DownloadLinkLifeTimeSec)
            };

            using var client = GetClient();
            var url = client.GetPreSignedURL(request);

            _logger.LogTrace($"AWS call: Method = 'GetPreSignedURL', BucketName = '{request.BucketName}', Key = '{request.Key}', Expires = '{request.Expires}'");

            return url;
        }
    }
}
