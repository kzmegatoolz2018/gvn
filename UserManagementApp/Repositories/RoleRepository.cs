using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using UserManagementApp.Models;

namespace UserManagementApp.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с ролями в PostgreSQL
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly string _connectionString;

        public RoleRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            var roles = new List<Role>();
            const string query = "SELECT role_id, name, permissions FROM roles ORDER BY name";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    roles.Add(new Role
                    {
                        RoleId = reader.GetInt32(reader.GetOrdinal("role_id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        Permissions = reader.IsDBNull(reader.GetOrdinal("permissions")) ? "" : reader.GetString(reader.GetOrdinal("permissions"))
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении ролей: {ex.Message}", ex);
            }

            return roles;
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            const string query = "SELECT role_id, name, permissions FROM roles WHERE role_id = @roleId";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@roleId", roleId);
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new Role
                    {
                        RoleId = reader.GetInt32(reader.GetOrdinal("role_id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        Permissions = reader.IsDBNull(reader.GetOrdinal("permissions")) ? "" : reader.GetString(reader.GetOrdinal("permissions"))
                    };
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении роли: {ex.Message}", ex);
            }

            return null;
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            const string query = "SELECT role_id, name, permissions FROM roles WHERE name = @name";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", name);
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new Role
                    {
                        RoleId = reader.GetInt32(reader.GetOrdinal("role_id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        Permissions = reader.IsDBNull(reader.GetOrdinal("permissions")) ? "" : reader.GetString(reader.GetOrdinal("permissions"))
                    };
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении роли по имени: {ex.Message}", ex);
            }

            return null;
        }
    }
}
