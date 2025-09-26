using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Services.Extensions
{
    public static class ModelStateExtensions
    {
        public static string ExtractErrors(this ModelStateDictionary modelState)
        {
            var errorMessages = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            var allErrors = string.Join(" | ", errorMessages);
            return allErrors;
        }
    }
}
