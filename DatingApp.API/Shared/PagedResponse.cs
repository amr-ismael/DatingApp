using System.Collections.Generic;

namespace DatingApp.API.Shared
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public string NextCursor { get; set; }
        public int PageSize { get; set; }

        public PagedResponse(IEnumerable<T> items, string nextCursor, int pageSize)
        {
            Items = items;
            NextCursor = nextCursor;
            PageSize = pageSize;
        }
    }
}
