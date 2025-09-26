using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Helper
{
    public class JWTSettings
    {
        public string SecritKey { get; set; } = string.Empty;
        public string AudienceIP { get; set; } = string.Empty;
        public string IssuerIP { get; set; } = string.Empty;
    }
}
