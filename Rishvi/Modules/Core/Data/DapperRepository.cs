using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using static Dapper.SqlMapper;

namespace Rishvi.Modules.Core.Data
{
    public interface IDapperRepository
    {
        T QueryFirst<T>(string Query, object parameters = null, SqlTransaction transaction = null);

        T QuerySingle<T>(string query, object parameters = null, SqlTransaction transaction = null);

        List<TEntity> GetAll<TEntity>(string Query, object parameters = null, SqlTransaction transaction = null);

        object QueryMultiple<T>(string Query, object parameters = null, SqlTransaction transaction = null);

        void Execute(string Query, object parameters, SqlTransaction transaction = null);
    }

    public class DapperRepository : IDapperRepository
    {
        public static string ConnectionString { get; set; }

        public T QueryFirst<T>(string Query, object parameters = null, SqlTransaction transaction = null)
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            return connection.QueryFirst<T>(Query, parameters, transaction);
        }

        public T QuerySingle<T>(string query, object parameters = null, SqlTransaction transaction = null)
        {
            try
            {
                if (transaction != null)
                {
                    return transaction.Connection.QuerySingle<T>(query, parameters, transaction);
                }

                using SqlConnection connection = new SqlConnection(ConnectionString);
                return connection.QuerySingle<T>(query, parameters, transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TEntity> GetAll<TEntity>(string Query, object parameters = null, SqlTransaction transaction = null)
        {
            using var connection = new SqlConnection(ConnectionString);
            var data = connection.Query<TEntity>(Query, parameters);
            return data.ToList();
        }

        public object QueryMultiple<T>(string Query, object parameters = null, SqlTransaction transaction = null)
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            using var multi = connection.QueryMultiple(Query, parameters, commandType: System.Data.CommandType.StoredProcedure);
            var data = multi.Read<T>().ToList();
            var total = multi.Read<int>().First();
            return new { data, total };
        }

        public void Execute(string Query, object parameters, SqlTransaction transaction = null)
        {
            if (transaction == null)
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Execute(Query, parameters, transaction);
            }
            else
            {
                transaction.Connection.Execute(Query, parameters, transaction);
            }
        }
    }
}