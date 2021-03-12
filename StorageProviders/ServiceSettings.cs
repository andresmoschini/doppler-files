namespace StorageProviders
{
    public class ServiceSettings
    {
        public string UsersRootFolder { get; set; }
        public int MaxFileSizeBytes { get; set; }
        public int MaxFilePath { get; set; }
        public int DownloadLinkLifeTimeSec { get; set; }
    }
}
