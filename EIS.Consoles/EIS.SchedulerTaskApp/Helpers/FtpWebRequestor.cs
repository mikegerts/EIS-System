using System;
using System.IO;
using System.Net;
using System.Text;
using EIS.SchedulerTaskApp.Repositories;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualBasic;
using EIS.Inventory.Shared.Models;
using System.Collections.Generic;

namespace EIS.SchedulerTaskApp.Helpers
{
    public class FtpWebRequestor
    {
        private readonly string _server;
        private readonly string _user;
        private readonly string _password;
        private readonly int _port;
        private readonly string _remoteFolder;

        public FtpWebRequestor(string server, string user, string password, int? port, string remoteFolder)
        {
            _server = server;
            _user = user;
            _password = password;
            _port = port ?? 21;
            _remoteFolder = remoteFolder ?? string.Empty;
        }

        public void WriteFtpFile(string fileFullPath)
        {
#if DEBUG
            var isReturn = true;
            if (isReturn)
                return;
#endif
            var ftpFullPath = string.Format("ftp://{0}:{1}/{2}/{3}", _server,
                        _port,
                        string.IsNullOrEmpty(_remoteFolder) ? string.Empty : _remoteFolder,
                        Path.GetFileName(fileFullPath));
            try
            {
                // read the file and transport it
                using (var stream = new StreamReader(fileFullPath))
                {
                    var buffer = Encoding.Default.GetBytes(stream.ReadToEnd());

                    // get the object to communicate with the FTP server
                    var request = (FtpWebRequest)WebRequest.Create(new Uri(ftpFullPath));
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(_user, _password);

                    var requestStream = request.GetRequestStream();
                    requestStream.Write(buffer, 0, buffer.Length);

                    // close the streams
                    requestStream.Close();
                }

                Logger.LogInfo(LogEntryType.FtpWebRequestor, "FTP file transfer completed -> " + ftpFullPath);
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.FtpWebRequestor, string.Format("Error in transfering file to {0} <br/>Error message: {1}", ftpFullPath, ex.Message), ex.StackTrace);
            }
        }

        public List<string> GetFileList(string fileExtension,string fileName)
        {
            var ftpFullPath = string.Format("ftp://{0}:{1}/{2}", _server,
                          _port,
                          string.IsNullOrEmpty(_remoteFolder) ? string.Empty : _remoteFolder);

            List<string> directories = new List<string>();

            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFullPath);
                ftpRequest.Credentials = new NetworkCredential(_user, _password);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());

                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        if (line.Contains(fileExtension))
                            directories.Add(line);
                    }
                    else if (line.Contains("."))
                    {
                        directories.Add(line);
                    }

                    line = streamReader.ReadLine();
                }

                streamReader.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.FtpWebRequestor, string.Format("Error in transfering file to {0} <br/>Error message: {1}", ftpFullPath, ex.Message), ex.StackTrace);
            }

            
            if (!string.IsNullOrEmpty(fileName))
            {
                List<string> tempDirectories = new List<string>();

                tempDirectories = directories;
                directories = new List<string>();

                foreach (var filePath in tempDirectories)
                {
                    string[] filePathArray = filePath.Split('/');
                    if (filePathArray[filePathArray.Length - 1].Contains(fileName))
                    {
                        directories.Add(filePath);
                    }

                }
            }

            return directories;
        }

        public string DownloadFtpFile(string ftpFullPath, string formattedFileName)
        {
            try
            {
                // get the object to communicate with the FTP server
                var request = (FtpWebRequest)WebRequest.Create(new Uri(ftpFullPath));
                request.Credentials = new NetworkCredential(_user, _password);

                var response = (FtpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();

                // opean a file stream to write the downloaded file.
                var supplierDirectory = ConfigurationManager.AppSettings["SupplierFilesRoot"].ToString();
                var localFilePath = string.Format("{0}/{1}", supplierDirectory, formattedFileName);
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
                Logger.LogInfo(LogEntryType.FtpWebRequestor, "FTP file download complete -> " + ftpFullPath);

                return localFilePath;
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.FtpWebRequestor, string.Format("Error in downloading file for {0} <br/>Error message: {1}", ftpFullPath, ex.Message), ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Find the part file name into FTP server
        /// </summary>
        /// <param name="partFileName">The part of the file name of the file</param>
        /// <returns>Returns the file full path from FTP server</returns>
        public string FindFileFromFtp(string partFileName)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(RemoteFtpPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_user, _password);

                var response = (FtpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                var line = reader.ReadLine();
                var filenameFound = string.Empty;

                while (!string.IsNullOrEmpty(line))
                {
                    if (Operators.LikeString(line, partFileName, CompareMethod.Text))
                    {
                        filenameFound = line;
                        break;
                    }

                    line = reader.ReadLine();
                }
                reader.Close();

                if (string.IsNullOrEmpty(filenameFound))
                    Logger.LogWarning(LogEntryType.FtpWebRequestor, string.Format("The file with part name \'{0}\' was not found from FTP server -> {1}", partFileName, RemoteFtpPath));

                return string.Format("{0}/{1}", RemoteFtpPath, filenameFound);
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.FtpWebRequestor, string.Format("Error in retrieving files from FTP server. <br/>Error message: {1}", RemoteFtpPath, ex.Message), ex.StackTrace);
                return null;
            }

        }

        public StreamReader GetFileStreamReader(string fileName)
        {
            var ftpFullPath = string.Format("ftp://{0}:{1}/{2}", _server,_port,fileName);
            StreamReader reader = null;
            try
            {
                // get the object to communicate with the FTP server
                var request = (FtpWebRequest)WebRequest.Create(new Uri(ftpFullPath));
                request.Credentials = new NetworkCredential(_user, _password);

                var response = (FtpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.FtpWebRequestor, string.Format("Error in retrieving files from FTP server. <br/>Error message: {1}", RemoteFtpPath, ex.Message), ex.StackTrace);
                return null;
            }
            return reader;
        }

        public async Task<bool> DeleteFileFromFtpAsync(string fileFullPath)
        {
#if DEBUG
            return true;
#endif
            try
            {
                // get the object to communicate with the FTP server
                var request = (FtpWebRequest)WebRequest.Create(new Uri(fileFullPath));
                request.Credentials = new NetworkCredential(_user, _password);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                var response = (FtpWebResponse)await request.GetResponseAsync();
                Logger.LogInfo(LogEntryType.FtpWebRequestor, string.Format("{0} - delete status: {1}", response.StatusDescription, fileFullPath));
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.FtpWebRequestor, string.Format("Error in deleting file from {0} <br/>Error message: {1}", fileFullPath, ex.Message), ex.StackTrace);
                return false;
            }
        }

        private string RemoteFtpPath
        {
            get
            {
                return string.Format("ftp://{0}:{1}/{2}", _server, _port, _remoteFolder);
            }
        }
    }
}