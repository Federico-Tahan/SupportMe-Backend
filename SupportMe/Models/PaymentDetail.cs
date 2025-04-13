using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SupportMe.Models
{
    [Table("PaymentDetail")]
    public class PaymentDetail
    {
        [Key]
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Last4 { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public string? ChargeId { get; set; }
        public string Funding { get; set; }
        public Status Status { get; set; }
        public string Currency { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDescription { get; set; }
        public decimal Amount { get; set; }
        public decimal NetReceivedAmount { get; set; }
        public decimal? RefundedAmount { get; set; }
        public int Installments { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardHolderEmail { get; set; }
        public DateTime PaymentDateUTC { get; set; }
        public string? UserId { get; set; }
        public int CampaignId { get; set; }
        public enum ErrorCodes
        {
            UNKNOWN,
            CARD_DECLINED,
            AUTHENTICATION_REQUIRED,
            CARD_NOT_SUPPORTED,
            CARD_DISABLED,
            CURRENCY_NOT_SUPPORTED,
            DUPLICATE_TRANSACTION,
            FRAUDULENT,
            ISSUER_NOT_AVAILABLE,
            INVALID_TRANSACTION,
            NOT_ALLOWED_TRANSACTION,
            NOT_ALLOWED_DEFERRED_PAYMENT,
            HOLD_CARD,
            INSUFFICIENT_FUNDS,
            LOST_CARD,
            STOLEN_CARD,
            INCORRECT_CVC,
            INCORRECT_DATA,
            EXPIRED_CARD,
            API_ERROR,
            INVALID_CARD,
            REFUSE,
            INSTALLMENT_UNAVAILABLE,
            INVALID_INSTALLMENT,
            EXCEEDS_CARD_LIMIT,
            HIGH_RISK_TRANSACTION,
            MAX_ATTEMPTS,
            CALL_CARD_ISSUER,
            RETRY,
            THREEDS_CHALLENGE_REJECTED
        }
    }



    public enum Status { OK, ERROR, REFUNDED }
}
