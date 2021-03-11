using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace StorageProviders
{
    public class FileValidator : IFileValidator
    {
        private readonly IOptions<ServiceSettings> _serviceSettings;

        public FileValidator(IOptions<ServiceSettings> serviceSettings)
        {
            _serviceSettings = serviceSettings;
        }

        // ^(?:|\\[1]) -- Begin with \
        // [a-z_\-\s0-9\.] -- valid characters are a-z| 0-9|-|.|_
        // (txt|gif|pdf|doc|docx|xls|xlsx) -- Valid extension
        private const string FILE_PATH_EXPRESION = @"^(?:|/[1])(/[a-zA-Z_\-\s0-9\.]+)+\.(txt|jpeg|png|gif|pdf|doc|docx|xls|xlsx)$";

        public StorageProviderError IsValid(string pathFile, byte[] content)
        {
            if (string.IsNullOrWhiteSpace(pathFile))
            {
                return StorageProviderError.MissingFilePath;
            }

            if (pathFile.Length > _serviceSettings.Value.MaxFilePath)
            {
                return StorageProviderError.MaxFilePathExceed;
            }

            if (!Regex.IsMatch(pathFile, FILE_PATH_EXPRESION))
            {
                return StorageProviderError.WrongPathFile;
            }

            if (content == null || content.Length == 0)
            {
                return StorageProviderError.MissingFileContent;
            }

            if (content.Length > _serviceSettings.Value.MaxFileSizeBytes)
            {
                return StorageProviderError.MaxFileSizeExceed;
            }

            return StorageProviderError.None;
        }
    }
}
