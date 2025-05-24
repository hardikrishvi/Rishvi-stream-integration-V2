namespace Rishvi.Modules.RishviFrameWorkDotNetCore.AppSettings
{
    public interface IAppSettings
    {
        // Environment: Local, Dev, Production
        string Environment { get; set; }

        string SiteCode { get; set; }
        // Site Name
        string SiteName { get; set; }

        // Enter assets path include virtual directory or full domain
        string AssetPath { get; set; }

        //File Upload
        string TempUploads { get; set; }
        string ResumeFileUpload { get; set; }
        string FileUploadSize { get; set; }

        // Google ReCaptcha
        bool RecaptchaEnable { get; set; }
        string RecaptchaPublicKey { get; set; }
        string RecaptchaSecretKey { get; set; }

        // AWS S3
        string AwsRegion { get; set; }
        string AwsAccessKey { get; set; }
        string AwsSecretKey { get; set; }
        string AwsBucket { get; set; }
        string AwsRoot { get; set; }
        string AwsBaseUrl { get; set; }

        //HiqPdf Key
        string HiqPdfKey { get; set; }
        string GAcode { get; set; }
        string SentryDsn { get; set; }

        //Admin Uer name/email Key
        string AdminUserName { get; set; }
        string AdminUserEmail { get; set; }

        string CalendlyURL { get; set; }
    }
}
