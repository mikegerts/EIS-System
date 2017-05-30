using System;
using System.IO;
using System.Net;
using EIS.Inventory.Core.Models;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualBasic;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Core.Helpers
{
    public static class FtpWebRequestor
    {
        public static LogEntry CheckFtpConnection(Credential credential)
        {

            var ftpFullPath = string.Format("ftp://{0}:{1}/{2}/", credential.Server,
                        credential.Port,
                        string.IsNullOrEmpty(credential.RemoteFolder) ? string.Empty : credential.RemoteFolder);
            try
            {
                // get the object to communicate with the FTP server
                var request = (FtpWebRequest)WebRequest.Create(new Uri(ftpFullPath));
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(credential.UserName, credential.Password);
                request.KeepAlive = false;
                request.GetResponse();

                return new LogEntry
                {
                    Severity = LogEntrySeverity.Information,
                    Description = "FTP credentials are valid and server is reachable!",
                };
            }
            catch (Exception ex)
            {
                return new LogEntry
                {
                    Severity = LogEntrySeverity.Error,
                    Description = "Error in checking FTP connection! Error Msg: " + EisHelper.GetExceptionMessage(ex)
                };
            }
        }
        public static LogEntry CheckFileFromFtp(Credential credential)
        {
            var parsedFileName = string.Format(credential.FileName, DateTime.Now);
            try
            {
                var isFileExist = false;
                if (credential.FileName.Contains("*"))
                    isFileExist = checkFilePartFromFtp(credential);
                else
                    isFileExist = checkFilenameFromFtp(credential);

                var message = isFileExist ? "and the \'{0}\'file is exist in FTP server!"
                    : "but the \'{0}\'file does NOT exist in FTP server!";

                return new LogEntry
                {
                    Severity = isFileExist ? LogEntrySeverity.Information : LogEntrySeverity.Error,
                    Description = string.Format("FTP credentials are valid {0}", string.Format(message, parsedFileName)),
                };
            }
            catch (Exception ex)
            {
                return new LogEntry
                {
                    Severity = LogEntrySeverity.Error,
                    Description = string.Format("The \'{0}\' file may not exist from FTP server or the credentials are invalid!<br/>Error Msg: {1}", 
                        parsedFileName, EisHelper.GetExceptionMessage(ex))
                };
            }
        }

        private static bool checkFilenameFromFtp(Credential credential)
        {

            var parsedFileName = string.Format(credential.FileName, DateTime.Now);
            var ftpFullPath = string.Format("{0}/{1}", credential.RemoteFtpPath, parsedFileName);

            // get the object to communicate with the FTP server
            var request = (FtpWebRequest)WebRequest.Create(new Uri(ftpFullPath));
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential(credential.UserName, credential.Password);
            request.KeepAlive = false;
            request.GetResponse();

            return true;
        }

        private static bool checkFilePartFromFtp(Credential credential)
        {
            var request = (FtpWebRequest)WebRequest.Create(credential.RemoteFtpPath);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(credential.UserName, credential.Password);

            var response = (FtpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            var reader = new StreamReader(responseStream);
            var line = reader.ReadLine();
            var filenameFound = string.Empty;

            while (!string.IsNullOrEmpty(line))
            {
                if (Operators.LikeString(line, credential.FileName, CompareMethod.Text))
                {
                    filenameFound = line;
                    break;
                }
                line = reader.ReadLine();
            }

            reader.Close();

            return !string.IsNullOrEmpty(filenameFound);
        }
    }
}
