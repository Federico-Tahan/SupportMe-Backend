namespace SupportMe.DTOs.DashboardDTOs
{
    public class BaseGraph<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}
