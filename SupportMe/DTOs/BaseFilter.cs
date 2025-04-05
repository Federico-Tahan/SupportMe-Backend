using SupportMe.Models.Enums;
using System.Text.Json.Serialization;

namespace SupportMe.DTOs
{
    public class BaseFilter
    {
        public int? Limit { get; set; }
        public int? Skip { get; set; }
        public List<SortingDTO>? Sorting { get; set; }
        public string? TextFilter { get; set; }
    }

    public class SortingDTO
    {
        public SORTBY SortBy { get; set; }
        public string Field { get; set; }
    }
}
