
namespace EIS.Inventory.Core.Models
{
    public class Credential
    {
        public Credential()
        {
            Port = 21;
        }

        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string RemoteFolder { get; set; }
        public string FileName { get; set; }

        public string RemoteFtpPath
        {
            get
            {
                return string.Format("ftp://{0}:{1}/{2}", Server, Port, string.IsNullOrEmpty(RemoteFolder) ? string.Empty : RemoteFolder);
            }
        }
    }
}
