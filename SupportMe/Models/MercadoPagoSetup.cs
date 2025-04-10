using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SupportMe.Models
{
    [Table("MercadopagoSetup")]
    public class MercadopagoSetup
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Email of mercadopago account
        /// </summary>
        public string AccountEmail { get; set; }

        public string AccessToken { get; set; }

        public string PublicKey { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string Currency { get; set; }

        public string? DisbursementInitiative { get; set; }

        /// <summary>
        /// Owner of the wallet with all the operations related to this mercadopago setup
        /// </summary>
        public string? OwnerUserId { get; set; }

        /// <summary>
        /// Only for sandbox mode accounts, customer email will be replaced by this email, so payment is approved in mercadopago sandbox
        /// </summary>
        public string? TestEmailAccount { get; set; }

        /// <summary>
        /// Possible values: "not_supported", "optional", "mandatory"
        /// https://www.mercadopago.com.mx/developers/es/docs/checkout-api/how-tos/integrate-3ds
        /// </summary>
        public string? ThreeDsMode { get; set; }
        public string? IntegrationId { get; set; }
        public string? CallBackUrl { get; set; }
        public bool TestMode { get; set; }
    }
}
