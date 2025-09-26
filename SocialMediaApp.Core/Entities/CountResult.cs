using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Entities
{
    [Keyless]
    public class CountResult
    {
        public int Count { get; set; }

    }
}
