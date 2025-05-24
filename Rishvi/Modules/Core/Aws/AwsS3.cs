using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Rishvi.Modules.Core.Helpers;
using Rishvi_Vault;

namespace Rishvi.Modules.Core.Aws
{
    public class AwsS3
    {
        public static string AwsBuketName = AWSParameter.GetConnectionString(AppSettings.AwsBuketName);
        public static string AwsAccessKey = AWSParameter.GetConnectionString(AppSettings.AwsAccessKey);
        public static string AwsSecretKey = AWSParameter.GetConnectionString(AppSettings.AwsSecretKey);

        public static async Task<bool> DeleteImageToAws(string awsFolderName, string fileKey)
        {
            try
            {
                IAmazonS3 client = AwsAuthentication();
                DeleteObjectRequest request = new DeleteObjectRequest()
                {
                    BucketName = AwsBuketName,
                    Key = awsFolderName + "/" + fileKey
                };
                DeleteObjectResponse response = await client.DeleteObjectAsync(request);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static IAmazonS3 AwsAuthentication()
        {
            AWSCredentials awsCredentials = new BasicAWSCredentials(AwsAccessKey, AwsSecretKey);
            IAmazonS3 client = new AmazonS3Client(awsCredentials, Amazon.RegionEndpoint.EUWest2);
            return client;
        }

        public static bool UploadFileToS3(string awsFolderName, System.IO.Stream fileInputStream, string fileKey)
        {
            try
            {
                IAmazonS3 client = AwsAuthentication();
                TransferUtility utility = new TransferUtility(client);

                TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();
                request.BucketName = AwsBuketName;
                request.Key = awsFolderName + "/" + fileKey;
                request.InputStream = fileInputStream;
                request.CannedACL = S3CannedACL.PublicReadWrite;
                request.StorageClass = S3StorageClass.Standard;

                utility.Upload(request);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static MemoryStream GetS3ImageForInputStream(string awsFolderName, string fileKey)
        {
            using (IAmazonS3 client = AwsAuthentication())
            {
                MemoryStream file = new MemoryStream();
                try
                {
                    Task<GetObjectResponse> objectResponse = client.GetObjectAsync(new GetObjectRequest()
                    {
                        BucketName = AwsBuketName,
                        Key = awsFolderName + "/" + fileKey
                    });

                    if (objectResponse.Result.ResponseStream != null)
                    {
                        long transferred = 0L;
                        BufferedStream stream2 = new BufferedStream(objectResponse.Result.ResponseStream);
                        byte[] buffer = new byte[0x2000];
                        int count = 0;
                        while ((count = stream2.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            file.Write(buffer, 0, count);
                        }
                    }
                    return file;
                }
                catch (AmazonS3Exception ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;
                }
            }
            return null;
        }
        public static async Task<List<string>> ListFilesInS3Folder(string awsFolderName)
        {
            List<string> fileList = new List<string>();

            using (IAmazonS3 client = AwsAuthentication()) // Assuming AwsAuthentication is properly implemented
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = AwsBuketName,
                    Prefix = awsFolderName + "/",  // Adding the folder prefix
                    Delimiter = "/"  // This will give us the "folder-like" structure
                };

                try
                {
                    ListObjectsV2Response response = await client.ListObjectsV2Async(request);
                    if (response != null && response.S3Objects != null)
                    {
                        // Loop through all the objects in the folder
                        foreach (var s3Object in response.S3Objects)
                        {
                            // Add each object's key (filename) to the list
                            fileList.Add(s3Object.Key);
                        }
                    }
                }
                catch (AmazonS3Exception ex)
                {
                    // Handle specific S3 exception
                    Console.WriteLine($"S3 Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Handle general exception
                    Console.WriteLine($"General Error: {ex.Message}");
                }
            }
            return fileList;
        }


        public static async Task<bool> S3FileIsExists(string awsFolderName, string fileKey)
        {
            using (IAmazonS3 client = AwsAuthentication())
            {
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = AwsBuketName;
                request.Key = awsFolderName + "/" + fileKey;
                try
                {
                    GetObjectResponse response = await client.GetObjectAsync(request);
                    if (response != null)
                    {
                        if (response.ResponseStream != null)
                        {
                            return true;
                        }
                    }
                }
                catch (Amazon.S3.AmazonS3Exception ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return false;
                    //status wasn't not found, so throw the exception
                    //throw;
                }
            }
            return false;
        }

        public static string GetS3File(string awsFolderName, string fileKey)
        {
            var jsonString = "";
            using (IAmazonS3 client = AwsAuthentication())
            {
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = AwsBuketName;
                request.Key = awsFolderName + "/" + fileKey;
                try
                {
                    Task<GetObjectResponse> response = client.GetObjectAsync(request);
                    using (Stream responseStream = response.Result.ResponseStream)
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        jsonString = reader.ReadToEnd();
                    }
                    return jsonString;
                }
                catch (Amazon.S3.AmazonS3Exception ex)
                {
                    //if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    //    return false;
                    //status wasn't not found, so throw the exception
                    //throw;
                }
            }
            return string.Empty;
        }

        public static byte[] GetByteS3File(string awsFolderName, string fileKey)
        {
            byte[] jsonString = null;
            using (IAmazonS3 client = AwsAuthentication())
            {
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = AwsBuketName;
                request.Key = awsFolderName + "/" + fileKey;
                try
                {
                    Task<GetObjectResponse> response = client.GetObjectAsync(request);
                    using (Stream responseStream = response.Result.ResponseStream)
                    {
                        jsonString = ReadFully(responseStream);
                    }
                    return jsonString;
                }
                catch (Amazon.S3.AmazonS3Exception ex)
                {
                    //if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    //    return false;
                    //status wasn't not found, so throw the exception
                    //throw;
                }
            }
            return jsonString;
        }
        public static bool CopyFileFormFolder(string awsFolderName, string sourceKey, string destinationKey)
        {
            try
            {
                using (IAmazonS3 client = AwsAuthentication())
                {
                    CopyObjectRequest request = new CopyObjectRequest();
                    request.SourceBucket = AwsBuketName;
                    request.SourceKey = awsFolderName + "/" + sourceKey;
                    request.DestinationBucket = AwsBuketName;
                    request.DestinationKey = awsFolderName + "/" + destinationKey;
                    request.CannedACL = S3CannedACL.PublicReadWrite;
                    request.StorageClass = S3StorageClass.Standard;

                    Task<CopyObjectResponse> response = client.CopyObjectAsync(request);

                    if (response.Result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
            }
            catch (Amazon.S3.AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}