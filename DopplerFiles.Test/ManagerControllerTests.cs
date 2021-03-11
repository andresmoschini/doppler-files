using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using DopplerFiles.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using StorageProviders;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DopplerFiles.Test
{
    public class ManagerControllerTests : IClassFixture<ProductionEnvironmentDopplerFilesApplicationFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly Mock<IAmazonS3> _amazonS3 = new Mock<IAmazonS3>();
        private const string ROOT_FOLDER = "Users";
        private const string SUPER_USER_FOLDER = "Su";

        public ManagerControllerTests(ProductionEnvironmentDopplerFilesApplicationFactory factory)
        {
            var fixture = new Fixture();
            var awsS3Settings = new Mock<IOptions<AwsS3Settings>>();
            awsS3Settings.SetupGet(x => x.Value).Returns(new AwsS3Settings
            {
                AccountInfo = new AwsS3AccountInfo
                {
                    AccessKey = fixture.Create<string>(),
                    RegionEndpoint = fixture.Create<string>(),
                    SecretAccessKey = fixture.Create<string>()
                },
                BucketName = fixture.Create<string>()
            }); ;
            var serviceSettings = new Mock<IOptions<ServiceSettings>>();
            serviceSettings.SetupGet(x => x.Value).Returns(new ServiceSettings
            {
                UsersRootFolder = ROOT_FOLDER,
                SuperUserFolder = SUPER_USER_FOLDER,
                MaxFilePath = 1024,
                MaxFileSizeBytes = 25000000,
                DownloadLinkLifeTimeSec = 300
            });
            var storageProvider = new FakeAwsS3StorageProvider(
                awsS3Settings.Object,
                serviceSettings.Object,
                Mock.Of<ILogger<AwsS3StorageProvider>>(), _amazonS3.Object);
            _httpClient = factory
                .WithWebHostBuilder((e) =>
                    e.ConfigureTestServices(services =>
                    {
                        services.AddTransient<IStorageProvider>(s => storageProvider);
                    }))
                .CreateClient();
        }

        [Fact]
        public async Task UploadFile_When_Success_Using_Super_User_Returns_PathFile()
        {
            var fixture = new Fixture();
            var url = $"http://localhost/manager";
            _httpClient.DefaultRequestHeaders.Add("Authorization",
                $"Bearer {TestsHelper.GetSuperUserAuthenticationToken()}");
            var uploadFileRequest = new UploadFileRequest
            {
                PathFile = $"/{fixture.Create<string>()}.pdf",
                Content = fixture.Create<byte[]>()
            };
            _amazonS3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse { });

            var result = await _httpClient.PostAsync(url, CreateUploadFileRequestContent(uploadFileRequest));

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.Equal($"/{ROOT_FOLDER}/{SUPER_USER_FOLDER}{uploadFileRequest.PathFile}", resultContent);
        }

        [Theory()]
        [InlineData("/file1.pdf")]
        [InlineData("/file-2.doc")]
        [InlineData("/file_3.png")]
        [InlineData("/folder1/folder-2/folder_3/FILE4.jpeg")]
        public async Task UploadFile_When_Success_Using_Another_User_Returns_PathFile(string pathFile)
        {
            var fixture = new Fixture();
            var idUser = fixture.Create<string>();
            var url = $"http://localhost/manager/{idUser}";
            _httpClient.DefaultRequestHeaders.Add("Authorization",
                $"Bearer {TestsHelper.GetAnotherUserAuthenticationToken(idUser)}");

            var uploadFileRequest = new UploadFileRequest
            {
                PathFile = pathFile,
                Content = fixture.Create<byte[]>()
            };
            _amazonS3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse { });

            var result = await _httpClient.PostAsync(url, CreateUploadFileRequestContent(uploadFileRequest));

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.Equal($"/{ROOT_FOLDER}/{idUser}{uploadFileRequest.PathFile}", resultContent);
        }

        [Theory()]
        [InlineData("file1.exe")]
        [InlineData("<@file2.png")]
        [InlineData("/file3")]
        [InlineData("file4.pdf")]
        [InlineData("/folder1//file5.png")]
        [InlineData("/folder1*file6.jpeg")]
        public async Task UploadFile_When_Wrong_PathFile_Returns_BadRequest_With_Error_Message(string pathFile)
        {
            var fixture = new Fixture();
            var url = $"http://localhost/manager";
            _httpClient.DefaultRequestHeaders.Add("Authorization",
                $"Bearer {TestsHelper.GetSuperUserAuthenticationToken()}");
            var uploadFileRequest = new UploadFileRequest
            {
                PathFile = pathFile,
                Content = fixture.Create<byte[]>()
            };

            var result = await _httpClient.PostAsync(url, CreateUploadFileRequestContent(uploadFileRequest));

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.Equal(StorageProviderError.WrongPathFile.ToString(), resultContent.Replace("\"", string.Empty));
        }

        [Fact]
        public async Task UploadFile_When_Storage_Provider_Fails_Returns_InternalServerError()
        {
            var fixture = new Fixture();
            var url = $"http://localhost/manager";
            _httpClient.DefaultRequestHeaders.Add("Authorization",
                $"Bearer {TestsHelper.GetSuperUserAuthenticationToken()}");
            var uploadFileRequest = new UploadFileRequest
            {
                PathFile = $"/{fixture.Create<string>()}.xls",
                Content = fixture.Create<byte[]>()
            };

            _amazonS3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            var result = await _httpClient.PostAsync(url, CreateUploadFileRequestContent(uploadFileRequest));

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task UploadFile_When_Missing_File_Content_Returns_BadRequest_With_Error_Message()
        {
            var fixture = new Fixture();
            var url = $"http://localhost/manager";
            _httpClient.DefaultRequestHeaders.Add("Authorization",
                $"Bearer {TestsHelper.GetSuperUserAuthenticationToken()}");
            var uploadFileRequest = new UploadFileRequest
            {
                PathFile = $"/{fixture.Create<string>()}.xls",
            };

            var result = await _httpClient.PostAsync(url, CreateUploadFileRequestContent(uploadFileRequest));

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.Equal(StorageProviderError.MissingFileContent.ToString(), resultContent.Replace("\"", string.Empty));
        }
        [Fact]
        public async Task UploadFile_When_File_Already_Exist_And_Not_Override_Returns_BadRequest_With_Error_Message()
        {
            var fixture = new Fixture();
            var url = $"http://localhost/manager";
            _httpClient.DefaultRequestHeaders.Add("Authorization",
                $"Bearer {TestsHelper.GetSuperUserAuthenticationToken()}");
            var uploadFileRequest = new UploadFileRequest
            {
                PathFile = $"/{fixture.Create<string>()}.png",
                Override = false,
                Content = fixture.Create<byte[]>()
            };
            _amazonS3.Setup(s => s.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse {
                    Key = $"/{ROOT_FOLDER}/{SUPER_USER_FOLDER}{uploadFileRequest.PathFile}"
                });

            var result = await _httpClient.PostAsync(url, CreateUploadFileRequestContent(uploadFileRequest));

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.Equal(StorageProviderError.FileAlreadyExist.ToString(), resultContent.Replace("\"", string.Empty));
        }


        [Fact]
        public async Task GetDownloadUrl_When_Success_Returns_Download_Url()
        {
            var fixture = new Fixture();
            var keyFile = fixture.Create<string>();
            var url = $"http://localhost/manager/{keyFile}";
            var donwloadUrl = fixture.Create<string>();
            _httpClient.DefaultRequestHeaders.Clear();

            _amazonS3.Setup(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>())).Returns(donwloadUrl);

            var result = await _httpClient.GetAsync(url);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.Equal(donwloadUrl, resultContent);
        }

        [Fact]
        public async Task GetDownloadUrl_When_Fails_Returns_InternalServerError()
        {
            var fixture = new Fixture();
            var keyFile = fixture.Create<string>();
            var url = $"http://localhost/manager/{keyFile}";
            var donwloadUrl = fixture.Create<string>();
            _httpClient.DefaultRequestHeaders.Clear();

            _amazonS3.Setup(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
                .Throws(new Exception());

            var result = await _httpClient.GetAsync(url);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task GetDownloadUrl_When_Fails_Returns_BadRequest_With_Error_Message()
        {
            var fixture = new Fixture();
            var keyFile = fixture.Create<string>();
            var url = $"http://localhost/manager/{keyFile}";
            var donwloadUrl = fixture.Create<string>();
            _httpClient.DefaultRequestHeaders.Clear();

            _amazonS3.Setup(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
                .Returns(string.Empty);

            var result = await _httpClient.GetAsync(url);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.Equal(StorageProviderError.UnableToGetPreSignedURL.ToString(), resultContent.Replace("\"", string.Empty));

        }

        private ByteArrayContent CreateUploadFileRequestContent(UploadFileRequest uploadFileRequest)
        {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(uploadFileRequest));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return byteContent;
        }
    }
}
