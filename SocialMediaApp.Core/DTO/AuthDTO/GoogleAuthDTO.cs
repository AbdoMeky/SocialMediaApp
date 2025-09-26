using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.AuthDTO
{
    public class GoogleAuthDTO
    {
        [Required]
        public required string IdToken { get; set; }
        [Required]
        public required string AccessToken { get; set; }
    }
}
