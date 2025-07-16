using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.DTOs;
using Rishvi.Modules.ErrorLogs.Models;
namespace Rishvi.Modules.ErrorLogs.Services
{
    public interface IErrorLogService
    {
        bool Create(ErrorLog dto);
        void CreateBySP(ErrorLog dto);
    }
    public class ErrorLogService : IErrorLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ErrorLog> _errorLogRepository;
        private readonly ConnectionStrings _connectionStrings;
        public ErrorLogService(IUnitOfWork unitOfWork,
            IRepository<ErrorLog> errorLogRepository,
            IOptions<ConnectionStrings> connectionStrings)
        {
            _unitOfWork = unitOfWork;
            _errorLogRepository = errorLogRepository;
            _connectionStrings = connectionStrings.Value;
        }
        public void CreateBySP(ErrorLog dto)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionStrings.Connection))
                {
                    string query = @"Insert Into ErrorLog (ErrorLogID,Name,Description,CreatedAt,UpdatedAt) 
                    Values (@ErrorLogId,@Name,@Description,@CreatedAt,@UpdatedAt)";
                    SqlCommand cmd1 = new SqlCommand(query, conn);
                    cmd1.CommandType = CommandType.Text;
                    cmd1.Parameters.AddWithValue("@ErrorLogId", Guid.NewGuid());
                    cmd1.Parameters.AddWithValue("@Name", dto.Name);
                    cmd1.Parameters.AddWithValue("@Description", dto.Description);
                    cmd1.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    cmd1.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                    conn.Open();
                    cmd1.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool Create(ErrorLog dto)
        {
            try
            {
                dto.ErrorLogID = Guid.NewGuid();
                dto.CreatedAt = DateTime.Now;
                dto.UpdatedAt = DateTime.Now;
                _errorLogRepository.Add(dto);
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
