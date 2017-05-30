using System;

namespace EIS.Inventory.DAL.Database
{
    public partial class vendorproductfileinventorytask
    {
        private string _fileName;

        /// <summary>
        /// Get the time stamped file name
        /// </summary>
        public string FormattedFileName
        {
            get
            {
                if (!string.IsNullOrEmpty(_fileName))
                    return _fileName;

                _fileName = string.Format(FileName, DateTime.Now) + FileType;
                return _fileName;
            }
        }

        /// <summary>
        /// Get the file full path with formatted file name from the FTP server
        /// </summary>
        public string FtpFileFullPath
        {
            get
            {
                return string.Format("ftp://{0}:{1}/{2}/{3}", FtpServer,
                FtpPort == null ? 21 : FtpPort,
                string.IsNullOrEmpty(RemoteFolder) ? string.Empty : RemoteFolder,
                FormattedFileName);
            }
        }
    }
}
