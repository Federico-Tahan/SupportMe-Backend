namespace SupportMe.DTOs.PaymentDTOs
{
    public class CardRequest
    {
        public string Token { get; set; }
        public string CardHolderName { get; set; }
        public string CardHolderEmail { get; set; }
        public string Brand { get; set; }
        public string Last4 { get; set; }
        public int ExpiryYear { get; set; }
        public int ExpiryMonth { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string GetFirstName()
        {
            if (string.IsNullOrWhiteSpace(CardHolderName))
                return string.Empty;

            var nameParts = CardHolderName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return nameParts.Length > 0 ? nameParts[0] : string.Empty;
        }

        public string GetLastName()
        {
            if (string.IsNullOrWhiteSpace(CardHolderName))
                return string.Empty;

            var nameParts = CardHolderName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : string.Empty;
        }
    }
}
