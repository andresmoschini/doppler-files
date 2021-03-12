using System.Threading.Tasks;

namespace StorageProviders
{
    public interface IStorageProvider
    {
        string GetDownloadUrl(string keyFile);
        Task<UploadFileResult> UploadFile(string pathFile, string idUser, byte[] content, bool overwrite = false);
    }
}
