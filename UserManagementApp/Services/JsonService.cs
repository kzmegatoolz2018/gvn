using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UserManagementApp.Models;

namespace UserManagementApp.Services
{
    /// <summary>
    /// Интерфейс для сервиса экспорта/импорта JSON
    /// </summary>
    public interface IJsonService
    {
        /// <summary>
        /// Экспорт пользователей в JSON файл
        /// </summary>
        Task ExportUsersToJsonAsync(IEnumerable<User> users, string filePath);

        /// <summary>
        /// Импорт пользователей из JSON файла
        /// </summary>
        Task<IEnumerable<UserExportModel>> ImportUsersFromJsonAsync(string filePath);
    }

    /// <summary>
    /// Сервис для работы с экспортом/импортом пользователей в формате JSON
    /// </summary>
    public class JsonService : IJsonService
    {
        public async Task ExportUsersToJsonAsync(IEnumerable<User> users, string filePath)
        {
            try
            {
                var exportData = users.Select(u => new UserExportModel
                {
                    Id = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                }).ToList();

                var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при экспорте данных в JSON: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<UserExportModel>> ImportUsersFromJsonAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Файл не найден: {filePath}");
                }

                var json = await File.ReadAllTextAsync(filePath);
                var users = JsonConvert.DeserializeObject<List<UserExportModel>>(json);
                return users ?? new List<UserExportModel>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Ошибка при чтении JSON файла: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при импорте данных из JSON: {ex.Message}", ex);
            }
        }
    }
}
