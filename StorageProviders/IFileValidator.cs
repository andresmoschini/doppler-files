using System.IO;

namespace StorageProviders
{
    public interface IFileValidator
    {
        StorageProviderError IsValid(string pathFile, byte[] content);
    }
}
