using SupportMe.DTOs.PaymentDTOs;

namespace SupportMe.DTOs.MercadoPagoDTOs
{
    public class ProcessMercadoPagoPayment
    {
        public ProcessMercadoPagoPayment(Payer payer, int installments, CardRequest card, decimal transaction_Amount, string description)
        {
            var item = new MPItems(transaction_Amount, description);
            this.payer = payer;
            this.description = description;
            this.installments = installments;
            this.payment_method_id = card.Brand;
            this.token = card.Token;
            this.transaction_amount = (float)transaction_Amount;
            this.additional_info = new Additional_Info
            {
                payer = new AditionalPayer { first_name = card.GetFirstName(), last_name = card.GetLastName() },
                items = new List<MPItems>() { item }

            };
            this.external_reference = Guid.NewGuid().ToString();
        }

        public Payer payer { get; set; }
        public int installments { get; set; }
        public string description { get; set; }
        public string payment_method_id { get; set; }
        public string token { get; set; }
        public string external_reference { get; set; }
        public float transaction_amount { get; set; }
        public bool binary_mode { get; set; } = false;
        public string three_d_secure_mode { get; set; } = "not_supported";
        public decimal application_fee { get; set; }
        public Additional_Info additional_info { get; set; }
    }


    public class Payer
    {
        public string email { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
        public IdentificationRequest identification { get; set; }
    }
    public class IdentificationRequest
    {
        public string type { get; set; }
        public string number { get; set; }
    }
    public class MP_Phone
    {
        public int Area_code { get; set; }
        public string Number { get; set; }
    }

    public class MP_Address
    {
        public string Zip_code { get; set; }
        public string Street_name { get; set; }
        public int Street_number { get; set; }
    }
}
