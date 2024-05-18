using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Models.Dtos;

namespace SuperAdmin.Service.Helpers
{
    public static class PaginationHelper
    {
        public static async Task<PaginationResult<T>> PaginateRecords<T>(IQueryable<T> records, int page, int pageSize)
            where T : class
        {
            int recordsToSkip = (page - 1) * pageSize;
            int totalRecords = await records.CountAsync();

            double totalPages = totalRecords / (double)pageSize;
            int pageCount = (int)Math.Ceiling(totalPages);

            var paginatedRecords = await records.Skip(recordsToSkip).Take(pageSize).ToArrayAsync();
            return new PaginationResult<T>(paginatedRecords, totalRecords, page, pageSize, pageCount);
        }
    }
}
