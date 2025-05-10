

namespace TBL.Core.Converter
{
    public class Pagination<T>
    {
        public List<T> Items { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPreviuos { get; set; }

        public Pagination(List<T> items, int count, int currentPage)
        {
            Items = items;
            TotalPages = (int)Math.Ceiling((double)count / 8);
            HasNext = currentPage < TotalPages;
            HasPreviuos = currentPage > 1;
            CurrentPage = currentPage;
        }

        public static Pagination<T> GetPage(IQueryable<T> query, int pageNumer, int pageSize)
        {
            var count = query.Count();
            var items = query.ToList().Skip((pageNumer - 1) * pageSize).Take(pageSize).ToList();
            return new Pagination<T>(items, count, pageNumer);

        }
    }
}
