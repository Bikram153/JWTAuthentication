using Dapper;
using Microsoft.Data.SqlClient;
using WebAPI.Models;

namespace WebAPI.Infrastructure
{
    public class DataAccess
    {
        private SqlConnection connection;
        public DataAccess(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            connection = new SqlConnection(connectionString);
            connection.Open();
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        public bool RegisterUser(string email, string password, string role)
        {
            var accountCount = connection.ExecuteScalar<int>(
                "select count(1) from [UserAccount] where [Email]=@email", new { email = email }
                );

            if (accountCount > 0) return false;

            var sql = "insert into [UserAccount] (Email,Password, Role) values (@email, @password, @role)";
            var result = connection.Execute(sql, new { email = email, password = password, role = role });

            return result > 0;
        }

        public UserAccount? FindUserByEmail(string email)
        {
            var sql = "select * from [USERACCOUNT] where [Email]=@email";
            return connection.QueryFirstOrDefault<UserAccount>(sql, new { email = email });
        }
    }
}
