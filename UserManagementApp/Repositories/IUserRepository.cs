using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagementApp.Models;

namespace UserManagementApp.Repositories
{
    /// <summary>
    /// Интерфейс для работы с пользователями в базе данных
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Получить всех пользователей с их ролями
        /// </summary>
        Task<IEnumerable<User>> GetAllUsersWithRolesAsync();

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        Task<User?> GetUserByIdAsync(int userId);

        /// <summary>
        /// Добавить нового пользователя
        /// </summary>
        Task<int> AddUserAsync(User user);

        /// <summary>
        /// Обновить пользователя
        /// </summary>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Удалить пользователя
        /// </summary>
        Task<bool> DeleteUserAsync(int userId);

        /// <summary>
        /// Проверить существует ли пользователь с таким именем или email
        /// </summary>
        Task<bool> UserExistsAsync(string username, string email);
    }
}
