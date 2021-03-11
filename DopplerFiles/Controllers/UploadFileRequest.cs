namespace DopplerFiles.Controllers
{
    public class UploadFileRequest
    {
        public byte[] Content { get; set; }
        public string PathFile { get; set; }
        public bool Override { get; set; }
    }
}
