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

        public bool InsertRefreshToken(RefreshToken refreshToken, string email)
        {
            var sql = "insert into [RefreshToken] (Token, CreatedDate, Expires, Enabled, Email) values (@token, @createddate, @expires, @enabled, @email)";

            var result = connection.Execute(sql, new
            {
                refreshToken.Token,
                refreshToken.CreatedDate,
                refreshToken.Expires,
                refreshToken.Enabled,
                email
            });

            return result > 0;
        }

        public bool DisableUserTokenByEmail(string email)
        {
            var sql = "update [RefreshToken] set [Enabled]=0 where [Email]=@email";
            var result = connection.Execute(sql, new { email });
            return result > 0;
        }

        public bool DisableUserToken(string token)
        {
            var sql = "update [RefreshToken] set [Enabled]=0 where [Token]=@token";
            var result = connection.Execute(sql, new { token });
            return result > 0;
        }

        public bool IsRefreshTokenValid(string token)
        {
            var sql = "select count(1) from RefreshToken where Token=@token and Enabled=1 and Expires>=CAST(GETDATE() AS DATE)";
            var result = connection.ExecuteScalar<int>(sql, new { token });
            return result > 0;
        }

        public UserAccount? FindUserByToken(string token)
        {
            var sql = "select UserAccount.* from RefreshToken inner join UserAccount on RefreshToken.Email=UserAccount.Email where Token=@token";
            return connection.QueryFirstOrDefault<UserAccount>(sql, new { token });
        }
    }
}
