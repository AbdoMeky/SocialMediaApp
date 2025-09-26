using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Utilities
{
    public class PaginationHelper
    {
        public static async Task<(int page, int pageSize)> NormalizePaginationAsync<T>(IQueryable<T> query, int page, int pageSize, int defaultPageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = pageSize == 0 ? defaultPageSize : pageSize;

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            page = Math.Min(page, totalPages == 0 ? 1 : totalPages);

            return (page, pageSize);
        }
        public static async Task<(int page, int pageSize)> NormalizePaginationWithCountAsync(int count, int page, int pageSize, int defaultPageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = pageSize == 0 ? defaultPageSize : pageSize;

            int totalPages = (int)Math.Ceiling((decimal)count / pageSize);
            if (page > totalPages)
            {
                return (page - totalPages, -1);
            }
            page = Math.Min(page, totalPages == 0 ? 1 : totalPages);

            return (page, pageSize);
        }
    }
}
