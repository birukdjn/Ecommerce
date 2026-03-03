namespace Domain.Common
{
    public class PagedList<T>(IEnumerable<T> items, int count)
    {
        public List<T> Items { get; set; } = [.. items];
        public int TotalCount { get; set; } = count;
    }
}