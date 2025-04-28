namespace SupportMe.DTOs.DashboardDTOs
{
    public class GraphResponse<Tkey, TValue>
    {
        public List<GroupGraph<Tkey, TValue>> Items { get; set; }
        public string Group { get; set; }
    }
}
