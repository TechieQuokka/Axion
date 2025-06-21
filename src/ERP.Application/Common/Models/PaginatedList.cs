using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Common.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageNumber { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items), "Items cannot be null.");
            }

            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Items = items;
        }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source cannot be null.");
            }

            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
