namespace SupportMe.DTOs
{
    public class PaginationDTO<T>
    {
        public List<T> Items { get; set; }
        public int TotalRegisters { get; set; }
    }
}
