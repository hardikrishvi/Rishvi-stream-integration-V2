using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExportFeedType
    {
        FTP,
        SFTP,
        BUCKET,
        DROPBOX,
        HTTP,
        FTPS,
    }
}