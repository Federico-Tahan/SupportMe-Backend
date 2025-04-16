namespace SupportMe.DTOs.PaymentDTOs
{
    public class PaymentLiveFeed
    {
        public PaginationDTO<PaymentDetailRead> Items { get; set; }
        public int TotalRegisters { get; set; }
        public int TotalOk { get; set; }
        public int TotalError { get; set; }
        public int TotalRefunded { get; set; }
    }
}
