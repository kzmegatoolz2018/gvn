using Newtonsoft.Json;

namespace UserManagementApp.Models
{
    /// <summary>
    /// Модель для экспорта/импорта пользователей в формате JSON
    /// </summary>
    public class UserExportModel
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Username")]
        public required string Username { get; set; }

        [JsonProperty("Email")]
        public required string Email { get; set; }

        [JsonProperty("Role")]
        public required string Role { get; set; }
    }
}
