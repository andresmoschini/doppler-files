using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StorageProviders;
using System.Threading.Tasks;
using System.Web;

namespace DopplerFiles.Controllers
{
    [ApiController]
    [Authorize]
    public class ManagerController : ControllerBase
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IFileValidator _fileValidator;

        public ManagerController(IStorageProvider storageProvider, IFileValidator fileValidator)
        {
            _storageProvider = storageProvider;
            _fileValidator = fileValidator;
        }

        [HttpPost]
        [Route("/{idUser?}")]
        public async Task<ActionResult> UploadFile([FromBody] UploadFileRequest request, string idUser = null)
        {
            var error = _fileValidator.IsValid(request.PathFile, request.Content);
            if (error != StorageProviderError.None)
            {
                return BadRequest(error);
            }

            var result = await _storageProvider.UploadFile(request.PathFile, idUser ?? string.Empty, request.Content, request.Override);

            if (result.StorageProviderError != StorageProviderError.None)
            {
                return BadRequest(result.StorageProviderError.ToString());
            }

            return Ok(result.PathFile);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/{keyFile}")]
        public ActionResult GetDownloadUrl(string keyFile)
        {
            var url = _storageProvider.GetDownloadUrl(HttpUtility.UrlDecode(keyFile));

            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(StorageProviderError.UnableToGetPreSignedURL.ToString());
            }

            return Ok(url);
        }
    }
}
