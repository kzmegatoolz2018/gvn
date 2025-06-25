using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using UserManagementApp.Models;

namespace UserManagementApp.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с пользователями в PostgreSQL
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<IEnumerable<User>> GetAllUsersWithRolesAsync()
        {
            var users = new List<User>();
            const string query = @"
                SELECT u.user_id, u.username, u.email, u.password_hash, u.role_id, 
                       u.created_at, u.updated_at, u.last_login, r.name as role
                FROM users u
                JOIN roles r ON u.role_id = r.role_id
                ORDER BY u.user_id";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        UserId = reader.GetInt32("user_id"),
                        Username = reader.GetString("username"),
                        Email = reader.GetString("email"),
                        PasswordHash = reader.GetString("password_hash"),
                        RoleId = reader.GetInt32("role_id"),
                        Role = reader.GetString("role"),
                        CreatedAt = reader.GetDateTime("created_at"),
                        UpdatedAt = reader.GetDateTime("updated_at"),
                        LastLogin = reader.IsDBNull("last_login") ? null : reader.GetDateTime("last_login")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении пользователей: {ex.Message}", ex);
            }

            return users;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            const string query = @"
                SELECT u.user_id, u.username, u.email, u.password_hash, u.role_id, 
                       u.created_at, u.updated_at, u.last_login, r.name as role
                FROM users u
                JOIN roles r ON u.role_id = r.role_id
                WHERE u.user_id = @userId";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        UserId = reader.GetInt32("user_id"),
                        Username = reader.GetString("username"),
                        Email = reader.GetString("email"),
                        PasswordHash = reader.GetString("password_hash"),
                        RoleId = reader.GetInt32("role_id"),
                        Role = reader.GetString("role"),
                        CreatedAt = reader.GetDateTime("created_at"),
                        UpdatedAt = reader.GetDateTime("updated_at"),
                        LastLogin = reader.IsDBNull("last_login") ? null : reader.GetDateTime("last_login")
                    };
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении пользователя: {ex.Message}", ex);
            }

            return null;
        }

        public async Task<int> AddUserAsync(User user)
        {
            const string query = @"
                INSERT INTO users (username, email, password_hash, role_id, created_at, updated_at)
                VALUES (@username, @email, @passwordHash, @roleId, @createdAt, @updatedAt)
                RETURNING user_id";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash ?? "");
                command.Parameters.AddWithValue("@roleId", user.RoleId);
                command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при добавлении пользователя: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            const string query = @"
                UPDATE users 
                SET username = @username, email = @email, password_hash = @passwordHash, 
                    role_id = @roleId, updated_at = @updatedAt
                WHERE user_id = @userId";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                
                command.Parameters.AddWithValue("@userId", user.UserId);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash ?? "");
                command.Parameters.AddWithValue("@roleId", user.RoleId);
                command.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при обновлении пользователя: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            const string query = "DELETE FROM users WHERE user_id = @userId";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при удалении пользователя: {ex.Message}", ex);
            }
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            const string query = "SELECT COUNT(*) FROM users WHERE username = @username OR email = @email";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@email", email);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при проверке существования пользователя: {ex.Message}", ex);
            }
        }
    }
}
