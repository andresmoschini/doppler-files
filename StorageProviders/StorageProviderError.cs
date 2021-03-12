namespace StorageProviders
{
    public enum StorageProviderError
    {
        None = 0,
        MaxFileSizeExceed = 1,
        WrongPathFile = 2,
        MissingFileContent = 3,
        MissingFilePath = 4,
        FileAlreadyExist = 5,
        UnableToGetPreSignedURL = 6,
        MaxFilePathExceed = 7
    }
}
