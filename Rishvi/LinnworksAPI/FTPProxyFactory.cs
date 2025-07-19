using LinnMacroCustomer;
using LinnworksMacroHelpers.Classes.Email;
using LinnworksMacroHelpers.Classes.Utility;
using LinnworksMacroHelpers.Classes;
using LinnworksMacroHelpers.Interfaces;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace LinnworksAPI
{
    public class FTPProxyFactory : IProxyFactory
    {
        public ProxiedDeleteFileResponse DeleteFileDropbox(DropboxSettings settings)
        {
            throw new NotImplementedException();
        }

        public ProxiedDeleteFileResponse DeleteFileFTP(FtpSettings settings)
        {
            ProxiedDeleteFileResponse proxiedDeleteFileResponse = new ProxiedDeleteFileResponse();
            try
            {
                FtpWebRequest request = (FtpWebRequest)System.Net.WebRequest.Create($"{settings.Server}:{settings.Port}/{settings.FullPath}");
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(settings.UserName, settings.Password);
                request.GetResponse();
            }
            catch (Exception ex)
            {
                proxiedDeleteFileResponse.Error = ex.Message;
            }

            return proxiedDeleteFileResponse;
        }

        public ProxiedDeleteFileResponse DeleteFileFTPS(FtpsSettings settings)
        {
            throw new NotImplementedException();
        }

        public ProxiedDeleteFileResponse DeleteFileSFTP(SFtpSettings settings)
        {
            throw new NotImplementedException();
        }

        public Stream DownloadDropboxFile(DropboxSettings settings)
        {
            throw new NotImplementedException();
        }

        public Stream DownloadFtpFile(FtpSettings settings)
        {
            try
            {
                // FtpWebRequest request = (FtpWebRequest)System.Net.WebRequest.Create("ftp://94.229.168.78:21/stocklevels/StockDownload_M.csv");
                FtpWebRequest request = (FtpWebRequest)System.Net.WebRequest.Create($"{settings.Server}:{settings.Port}/{settings.FullPath}");
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(settings.UserName, settings.Password);
                request.EnableSsl = false;
                request.UsePassive = true;
                request.ContentLength = 225720576;
                request.Timeout = 300000; // 5 minutes
                request.ReadWriteTimeout = 300000;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return response.GetResponseStream();
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public Stream DownloadFtpsFile(FtpsSettings settings)
        {
            FtpWebRequest request = (FtpWebRequest)System.Net.WebRequest.Create($"{settings.Server}:{settings.Port}/{settings.FullPath}");
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(settings.UserName, settings.Password);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            return response.GetResponseStream();
        }



        public ProxyUpload<DropboxSettings, DropboxUploadResult> GetDropboxUploadProxy(DropboxSettings settings)
        {
            throw new NotImplementedException();
        }

        public ProxyUpload<FtpsSettings, FtpsUploadResult> GetFtpsUploadProxy(FtpsSettings settings)
        {
            throw new NotImplementedException();
        }

        public ProxyUpload<FtpSettings, FtpUploadResult> GetFtpUploadProxy(FtpSettings settings)
        {
            return new FTPProxyUpload(settings);
        }

        public ProxyUpload<SFtpSettings, SftpUploadResult> GetSFtpUploadProxy(SFtpSettings settings)
        {
            throw new NotImplementedException();
        }

        public ProxiedListDirectoryResponse ListDirectoryDropbox(DropboxSettings settings)
        {
            throw new NotImplementedException();
        }

        public ProxiedListDirectoryResponse ListDirectoryFTP(FtpSettings settings)
        {
            ProxiedListDirectoryResponse proxiedListDirectoryResponse = new ProxiedListDirectoryResponse();
            try
            {
                proxiedListDirectoryResponse.FileList = new System.Collections.Generic.List<BaseDirectoryItem>();
                FtpWebRequest request = (FtpWebRequest)System.Net.WebRequest.Create($"{settings.Server}:{settings.Port}/{settings.FullPath}");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(settings.UserName, settings.Password);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        proxiedListDirectoryResponse.FileList.Add(new BaseDirectoryItem() { Path = line });
                    }
                }
            }
            catch (Exception ex)
            {
                proxiedListDirectoryResponse.Error = ex.Message;
            }

            return proxiedListDirectoryResponse;
        }

        public ProxiedListDirectoryResponse ListDirectoryFTPS(FtpsSettings settings)
        {
            throw new NotImplementedException();
        }

        public ProxiedListDirectoryResponse ListDirectorySFTP(SFtpSettings settings)
        {
            throw new NotImplementedException();
        }

        public ProxiedRenameFileResponse RenameFileDropbox(DropboxSettings settings, string newName)
        {
            throw new NotImplementedException();
        }

        public ProxiedRenameFileResponse RenameFileFTP(FtpSettings settings, string newName)
        {
            throw new NotImplementedException();
        }

        public ProxiedRenameFileResponse RenameFileFTPS(FtpsSettings settings, string newName)
        {
            throw new NotImplementedException();
        }

        public ProxiedRenameFileResponse RenameFileSFTP(SFtpSettings settings, string newName)
        {
            throw new NotImplementedException();
        }

        //public ProxiedEmailResponse SendEmail(ProxiedEmailRequest request)
        //{
        //    throw new NotImplementedException();
        //}
        public ProxiedEmailResponse SendEmail(ProxiedEmailRequest request)
        {
            // Validate input request
            if (request == null)
            {
                return new ProxiedEmailResponse
                {
                    IsError = true,
                    Error = "Email request cannot be null."
                };
            }

            if (request.Settings == null)
            {
                return new ProxiedEmailResponse
                {
                    IsError = true,
                    Error = "Email settings are missing."
                };
            }

            if (request.To == null || request.To.Count == 0)
            {
                return new ProxiedEmailResponse
                {
                    IsError = true,
                    Error = "Recipient email addresses are required."
                };
            }

            try
            {
                // Initialize SmtpClient
                using (var smtpClient = new SmtpClient(request.Settings.Host, request.Settings.Port))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(request.Settings.Username, request.Settings.Password);
                    smtpClient.EnableSsl = request.Settings.EnableSsl;

                    // Create the MailMessage
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(request.From.Address, request.From.DisplayName),
                        Subject = request.Subject,
                        Body = request.Body,
                        IsBodyHtml = request.IsBodyHtml
                    };

                    // Add To recipients
                    foreach (var recipient in request.To)
                    {
                        mailMessage.To.Add(new MailAddress(recipient.Address, recipient.DisplayName));
                    }

                    // Add CC recipients if provided
                    if (request.CC != null)
                    {
                        foreach (var ccRecipient in request.CC)
                        {
                            mailMessage.CC.Add(new MailAddress(ccRecipient.Address, ccRecipient.DisplayName));
                        }
                    }

                    // Add Sender if provided
                    if (request.Sender != null)
                    {
                        mailMessage.Sender = new MailAddress(request.Sender.Address, request.Sender.DisplayName);
                    }

                    // Send the email
                    smtpClient.Send(mailMessage);
                }

                // Return success response
                return new ProxiedEmailResponse
                {
                    IsError = false
                };
            }
            catch (Exception ex)
            {
                // Return failure response with the exception message
                return new ProxiedEmailResponse
                {
                    IsError = true,
                    Error = $"Failed to send email: {ex.Message}"
                };
            }
        }

        public ProxiedWebResponse WebRequest(ProxiedWebRequest request)
        {
            request.Validate();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(request.Url);
                webRequest.Method = request.Method;
                webRequest.Timeout = request.Timeout;

                if (!string.IsNullOrEmpty(request.Referrer))
                {
                    webRequest.Referer = request.Referrer;
                }

                foreach (var header in request.Headers)
                {
                    webRequest.Headers[header.Key] = header.Value;
                }

                if (!string.IsNullOrEmpty(request.ContentType))
                {
                    webRequest.ContentType = request.ContentType;
                }

                if (request.RawBody != null && request.RawBody.Length > 0 && (request.Method == "POST" || request.Method == "PUT"))
                {
                    webRequest.ContentLength = request.RawBody.Length;
                    using (Stream dataStream = webRequest.GetRequestStream())
                    {
                        dataStream.Write(request.RawBody, 0, request.RawBody.Length);
                    }
                }

                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    return new ProxiedWebResponse
                    {
                        RawResponse = Encoding.UTF8.GetBytes(responseText),
                        StatusCode = (int)response.StatusCode,
                        Headers = GetHeaders(response.Headers),
                        CharacterSet = response.CharacterSet,
                        ContentType = response.ContentType,
                        ContentEncoding = response.ContentEncoding
                    };
                }
            }
            catch (WebException ex)
            {
                using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response)
                using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                {
                    string errorText = reader.ReadToEnd();
                    return new ProxiedWebResponse
                    {
                        RawResponse = Encoding.UTF8.GetBytes(errorText),
                        StatusCode = (int)(errorResponse?.StatusCode ?? HttpStatusCode.InternalServerError),
                        ErrorMessage = ex.Message
                    };
                }
            }
        }

        private static Dictionary<string, string> GetHeaders(WebHeaderCollection headers)
        {
            var headerDict = new Dictionary<string, string>();
            foreach (string key in headers.AllKeys)
            {
                headerDict[key] = headers[key];
            }
            return headerDict;
        }

        public Stream DownloadSFtpFile(SFtpSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}