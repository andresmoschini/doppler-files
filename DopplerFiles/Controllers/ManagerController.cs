using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StorageProviders;
using System.Threading.Tasks;

namespace DopplerFiles.Controllers
{
    [ApiController]
    [Authorize]
    public class ManagerController : ControllerBase
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IFileValidator _fileValidator;
        private readonly IOptions<ServiceSettings> _serviceSettings;

        public ManagerController(IStorageProvider storageProvider, IFileValidator fileValidator, IOptions<ServiceSettings> serviceSettings)
        {
            _storageProvider = storageProvider;
            _fileValidator = fileValidator;
            _serviceSettings = serviceSettings;
        }

        [HttpPost]
        [Route("[controller]/{idUser?}")]
        public async Task<ActionResult> UploadFile([FromBody] UploadFileRequest request, string idUser = null)
        {
            var error = _fileValidator.IsValid(request.PathFile, request.Content);
            if (error != StorageProviderError.None)
            {
                return BadRequest(error);
            }

            var result = await _storageProvider.UploadFile(request.PathFile, idUser ?? _serviceSettings.Value.SuperUserFolder, request.Content, request.Override);

            if (result.StorageProviderError != StorageProviderError.None)
            {
                return BadRequest(result.StorageProviderError.ToString());
            }

            return Ok(result.PathFile);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("[controller]/{keyFile}")]
        public ActionResult GetDownloadUrl(string keyFile)
        {
            var url = _storageProvider.GetDownloadUrl(keyFile);

            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(StorageProviderError.UnableToGetPreSignedURL.ToString());
            }

            return Ok(url);
        }
    }
}
