using System.Data;
using System.Data.SqlClient;

namespace Rishvi.Modules.Core.Helpers
{
    public static class SqlHelper
    {
        //This is my connection string i have assigned the database file address path
        //public static string MyConnection2 = "Server=DESKTOP-PI703NO;Database=HyperTest;Trusted_Connection=True;";

        //This is static connection string. I will make it dynamic(RC)
        //public static string MyConnection2 = "Server=rds-master-admin-panel.c0s0ifu7tzo1.eu-west-2.rds.amazonaws.com;Database=ebay_stream;uid=mdbAdmin;pwd=gYk9bKztXFhANPuMYp8E;Trusted_Connection=false;Integrated Security=false;MultipleActiveResultSets=True;Connect Timeout=200; Pooling=true; Max Pool Size=200;TrustServerCertificate=True";

        private static IConfiguration _configuration;

        static SqlHelper()
        {
            // Initialize the configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Ensure to set the base path for the configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public static string GetConnectionString(string connectionName)
        {
            return _configuration.GetConnectionString(connectionName);
        }

        public static int ErrorLogInsert(string name, string description)
        {
            string query = "Insert into Errorlog(ErrorLogID,Name,Description,CreatedAt,UpdatedAt)Values(newId(),'"+name+"','"+description+"',GetDate(),GetDate())";
            SqlConnection con = new SqlConnection(GetConnectionString("SqlHelperConnection"));
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.CommandType = CommandType.Text;
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return 0;
        }

        public static int SystemLogInsert(string moduleName, string requestHeader, string requestJson, string responseJson,string status, string message, bool isError,string UserID)
        {
            message = message == null ? "" : message;
            string query = "Insert into Systemlog(SystemLogID, UserId,ModuleName,RequestHeader,RequestJson,ResponseJson,Status,Message,IsError,CreatedAt,UpdatedAt)Values(newId(),null,'" + moduleName + "','" + requestHeader + "','" + requestJson + "','" + responseJson + "','" + status + "','" + message.Replace("'"," ")+ "',"+ (isError ? '1':'0') + ",GetDate(),GetDate())";
            SqlConnection con = new SqlConnection(GetConnectionString("SqlHelperConnection"));
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.CommandType = CommandType.Text;
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return 0;
        }
    }
}
