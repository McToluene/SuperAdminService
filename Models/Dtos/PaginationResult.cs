using Configurations.Utility;

namespace SuperAdmin.Service.Models.Dtos
{
    public class PaginationResult<T> : Page<T>
    {
        public PaginationResult(T[] items, long totalSize, long pageNumber, long pageSize, long totalPages) 
            : base(items, totalSize, pageNumber, pageSize)
        {
            TotalPages = totalPages;
        }

        public long TotalPages { get; set; }
    }
}
