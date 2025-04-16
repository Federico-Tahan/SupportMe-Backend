namespace SupportMe.DTOs.MercadoPagoDTOs
{
    public class ProcessMercadoPagoPaymentResponse
    {
        public string id { get; set; }
        public DateTime date_created { get; set; }
        public object date_approved { get; set; }
        public DateTime date_last_updated { get; set; }
        public object date_of_expiration { get; set; }
        public object money_release_date { get; set; }
        public string money_release_status { get; set; }
        public string operation_type { get; set; }
        public string issuer_id { get; set; }
        public string payment_method_id { get; set; }
        public string payment_type_id { get; set; }
        public Payment_Method payment_method { get; set; }
        public string status { get; set; }
        public string status_detail { get; set; }
        public string currency_id { get; set; }
        public object description { get; set; }
        public bool live_mode { get; set; }
        public object sponsor_id { get; set; }
        public string authorization_code { get; set; }
        public object money_release_schema { get; set; }
        public decimal taxes_amount { get; set; }
        public object counter_currency { get; set; }
        public object brand_id { get; set; }
        public decimal shipping_amount { get; set; }
        public string build_version { get; set; }
        public object pos_id { get; set; }
        public object store_id { get; set; }
        public object integrator_id { get; set; }
        public object platform_id { get; set; }
        public object corporation_id { get; set; }
        public MPPayer payer { get; set; }
        public int collector_id { get; set; }
        public object marketplace_owner { get; set; }
        public Metadata metadata { get; set; }
        public Additional_Info additional_info { get; set; }
        public object external_reference { get; set; }
        public decimal transaction_amount { get; set; }
        public decimal transaction_amount_refunded { get; set; }
        public decimal coupon_amount { get; set; }
        public object differential_pricing_id { get; set; }
        public object financing_group { get; set; }
        public object deduction_schema { get; set; }
        public int installments { get; set; }
        public Transaction_Details transaction_details { get; set; }
        public object[] fee_details { get; set; }
        public Charges_Details[] charges_details { get; set; }
        public bool captured { get; set; }
        public bool binary_mode { get; set; }
        public object call_for_authorize_id { get; set; }
        public string statement_descriptor { get; set; }
        public Card card { get; set; }
        public object notification_url { get; set; }
        public object[] refunds { get; set; }
        public string processing_mode { get; set; }
        public object merchant_account_id { get; set; }
        public object merchant_number { get; set; }
        public object[] acquirer_reconciliation { get; set; }
        public Point_Of_Interaction point_of_interaction { get; set; }
        public object accounts_info { get; set; }
        public object release_info { get; set; }
        public object tags { get; set; }
    }

    public class Payment_Method
    {
        public string id { get; set; }
        public string type { get; set; }
        public string issuer_id { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public Routing_Data routing_data { get; set; }
    }

    public class Routing_Data
    {
        public string merchant_account_id { get; set; }
    }

    public class MPPayer
    {
        public Identification identification { get; set; }
        public object entity_type { get; set; }
        public Phone phone { get; set; }
        public object last_name { get; set; }
        public string id { get; set; }
        public object type { get; set; }
        public object first_name { get; set; }
        public object email { get; set; }
    }

    public class Identification
    {
        public object number { get; set; }
        public object type { get; set; }
    }

    public class Phone
    {
        public object number { get; set; }
        public object extension { get; set; }
        public object area_code { get; set; }
    }

    public class Metadata
    {
    }

    public class Additional_Info
    {
        public AditionalPayer payer { get; set; }
        public List<MPItems> items { get; set; }
    }
    public class MPItems
    {
        public MPItems(decimal amount, string title)
        {
            this.unit_price = amount;
            this.title = title;
            this.category_id = "Others";
            this.id = 0;
            this.event_date = DateTime.Now;
            this.quantity = 1;
        }

        public string category_id { get; set; }
        public int id { get; set; }
        public DateTime event_date { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
        public string title { get; set; }


    }
    public class AditionalPayer
    {
        public string last_name { get; set; }
        public string first_name { get; set; }
    }

    public class Transaction_Details
    {
        public object payment_method_reference_id { get; set; }
        public object acquirer_reference { get; set; }
        public decimal net_received_amount { get; set; }
        public decimal total_paid_amount { get; set; }
        public decimal overpaid_amount { get; set; }
        public object external_resource_url { get; set; }
        public decimal installment_amount { get; set; }
        public object financial_institution { get; set; }
        public object payable_deferral_period { get; set; }
    }

    public class Card
    {
        public object id { get; set; }
        public object first_six_digits { get; set; }
        public string last_four_digits { get; set; }
        public object expiration_month { get; set; }
        public object expiration_year { get; set; }
        public object date_created { get; set; }
        public object date_last_updated { get; set; }
        public object country { get; set; }
        public object[] tags { get; set; }
        public Cardholder cardholder { get; set; }
    }

    public class Cardholder
    {
        public Identification1 identification { get; set; }
        public object name { get; set; }
    }

    public class Identification1
    {
        public object number { get; set; }
        public object type { get; set; }
    }

    public class Point_Of_Interaction
    {
        public string type { get; set; }
        public Business_Info business_info { get; set; }
    }

    public class Business_Info
    {
        public string unit { get; set; }
        public string sub_unit { get; set; }
        public string branch { get; set; }
    }

    public class Charges_Details
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public Accounts accounts { get; set; }
        public int client_id { get; set; }
        public DateTime date_created { get; set; }
        public DateTime last_updated { get; set; }
        public Amounts amounts { get; set; }
        public Metadata1 metadata { get; set; }
        public object reserve_id { get; set; }
        public object[] refund_charges { get; set; }
    }

    public class Accounts
    {
        public string from { get; set; }
        public string to { get; set; }
    }

    public class Amounts
    {
        public decimal original { get; set; }
        public decimal refunded { get; set; }
    }

    public class Metadata1
    {
        public string source { get; set; }
    }

    public enum MP_STATUS { approved, rejected, pending, in_process }
    public enum MP_STATUS_DETAIL { accredited }
}
