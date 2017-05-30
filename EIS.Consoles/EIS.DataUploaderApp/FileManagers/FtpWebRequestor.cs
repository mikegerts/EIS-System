using System;
using System.IO;
using System.Net;
using EIS.DataUploaderApp.Models;
using EIS.DataUploaderApp.Repositories;
using System.Threading.Tasks;

namespace EIS.DataUploaderApp.FileManagers
{
    public class FtpWebRequestor
    {
        private readonly FileSetting _fileSetting;

        public FtpWebRequestor(FileSetting fileSetting)
        {
            _fileSetting = fileSetting;
        }

        public string DownloadFtpFile()
        {
            try
            {
                // get the object to communicate with the FTP server
                var request = (FtpWebRequest)WebRequest.Create(new Uri(_fileSetting.FtpFileFullPath));
                request.Credentials = new NetworkCredential(_fileSetting.FtpUser, _fileSetting.FtpPassword);

                var response = (FtpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();

                // opean a file stream to write the downloaded file.
                var localFilePath = string.Format("{0}/{1}", _fileSetting.FilePath, _fileSetting.FormattedFileName);
                var localFileStream = new FileStream(localFilePath, FileMode.Create);

                // buffer fo rthe downloaded data
                var buffer = new byte[2048];
                var bytesRead = responseStream.Read(buffer, 0, 2048);

                // download the file by writing the buffered data until the transfer is complete
                while (bytesRead > 0)
                {
                    localFileStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, 2048);
                }

                // close the streams
                localFileStream.Close();
                response.Close();
                Logger.LogInfo(this.GetType().Name, "FTP file download complete -> " + _fileSetting.FtpFileFullPath);

                return localFilePath;
            }
            catch (Exception ex)
            {
                Logger.LogError(this.GetType().Name, string.Format("Error in downloading file for {0} <br/>Error message: {1}", _fileSetting.FtpFileFullPath, ex.Message), ex.StackTrace);
                return null;
            }
        }

        public async Task<bool> DeleteFileFromFtpAsync()
        {
            if (!_fileSetting.IsDeleteFile)
                return true;

            try
            {
                // get the object to communicate with the FTP server
                var request = (FtpWebRequest)WebRequest.Create(new Uri(_fileSetting.FtpFileFullPath));
                request.Credentials = new NetworkCredential(_fileSetting.FtpUser, _fileSetting.FtpPassword);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                var response = (FtpWebResponse) await request.GetResponseAsync();
                Logger.LogInfo(this.GetType().Name, string.Format("{0} - delete status: {1}", response.StatusDescription, _fileSetting.FtpFileFullPath));
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(this.GetType().Name, string.Format("Error in deleting file from {0} <br/>Error message: {1}", _fileSetting.FtpFileFullPath, ex.Message), ex.StackTrace);
                return false;
            }
        }
    }
}
