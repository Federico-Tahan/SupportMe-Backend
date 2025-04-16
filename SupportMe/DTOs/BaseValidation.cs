using SupportMe.Models;
using System.Text.Json.Serialization;

namespace SupportMe.DTOs
{
    public class BaseValidation
    {
        public ValidationError Error { get; set; }
        public ValidationStatusCode Status { get; set; }
        public dynamic Response { get; set; }
        public BaseValidation()
        {
            this.Status = ValidationStatusCode.PENDING;
            this.Error = ValidationError.NONE;
        }

        public BaseValidation(PaymentDetail paymentDetail)
        {
            this.Status = ValidationStatusCode.PENDING;
            if (paymentDetail.Status == Models.Status.OK)
            {
                Status = ValidationStatusCode.SUCCESS;
            }
            else if (paymentDetail.Status == Models.Status.ERROR)
            {
                Status = ValidationStatusCode.ERROR;
            }

            Response = paymentDetail;
        }

        public bool Success => this.Status == ValidationStatusCode.SUCCESS;
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ValidationStatusCode { SUCCESS, ERROR, PENDING }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ValidationError
    {
        NONE = 0,
    }
}
