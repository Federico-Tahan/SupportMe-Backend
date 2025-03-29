using SupportMe.Models.Enums;

namespace SupportMe.DTOs
{
    public class BaseFilter
    {
        public int? Limit { get; set; }
        public int? Skip { get; set; }
        public SORTBY? SortBy { get; set; }
        public string? ColumnFilter { get; set; }
    }
}
