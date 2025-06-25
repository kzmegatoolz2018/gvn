using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagementApp.Models;

namespace UserManagementApp.Repositories
{
    /// <summary>
    /// Интерфейс для работы с ролями в базе данных
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Получить все роли
        /// </summary>
        Task<IEnumerable<Role>> GetAllRolesAsync();

        /// <summary>
        /// Получить роль по ID
        /// </summary>
        Task<Role?> GetRoleByIdAsync(int roleId);

        /// <summary>
        /// Получить роль по названию
        /// </summary>
        Task<Role?> GetRoleByNameAsync(string name);
    }
}
