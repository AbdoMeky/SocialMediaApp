using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.AuthDTO
{
    public class AuthResultDTO
    {
        public bool IsAuthenticated { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        [JsonIgnore]
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpired { get; set; }
    }
}
