using System;
using System.ComponentModel.DataAnnotations;

namespace ScreenWorking.Server.API.Data.Entities
{
    public class UserEntity
    {
        [Key]
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Editor";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
