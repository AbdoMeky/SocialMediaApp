using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.User
{
    public class UpdateProfilePictureUrlDTO
    {
        public IFormFile? Image { get; set; }
    }
}
