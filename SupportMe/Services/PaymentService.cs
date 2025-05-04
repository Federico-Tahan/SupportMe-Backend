using SupportMe.DTOs.PaymentDTOs;
using SupportMe.DTOs;
using Newtonsoft.Json;
using SupportMe.Helpers;
using Microsoft.EntityFrameworkCore;
using SupportMe.Models;
using System.Net.NetworkInformation;
using SupportMe.DTOs.MercadoPagoDTOs;
using SupportMe.Data;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using SupportMe.DTOs.DonationDTOs;

namespace SupportMe.Services
{
    public class PaymentService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public PaymentService(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<BaseValidation> Pay(PaymentInformation paymentInformation, int id, string? userId)
        {
            BaseValidation response = new BaseValidation();
            var campaign = await _context.Campaigns.Where(x => x.Id == id).FirstOrDefaultAsync();

            try
            {
                var payment = await this.Payment(paymentInformation, campaign);

                if (payment != null)
                {
                    response = await ProcessPaymentDetail(payment, paymentInformation, campaign.UserId, campaign.Id, userId);
                }

            }
            catch(Exception ex) 
            {
                response.Response = ex.Message;
            }

            return response;
        }

        private async Task<ProcessMercadoPagoPaymentResponse> Payment(PaymentInformation paymentInformation, Campaign campaign)
        {
            var mpAccount = await _context.UserMercadoPago.Where(x => x.UserId == campaign.UserId).FirstOrDefaultAsync();

            string mercadoPagoApi = $"{_configuration.GetValue<string>("MERCADO_PAGO_API")}/v1/payments";

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", $"Bearer {mpAccount.Token}");
            headers.Add("X-meli-session-id", paymentInformation.DeviceId);
            headers.Add("X-Idempotency-Key", paymentInformation.Idempotency);

            Payer payer = new Payer();
            payer.email = paymentInformation.Card.CardHolderEmail;
            payer.first_name = paymentInformation.Card.GetFirstName();
            payer.last_name = paymentInformation.Card.GetLastName();
            payer.identification = new IdentificationRequest
            {
                type = paymentInformation.Card.DocumentType,
                number = paymentInformation.Card.DocumentNumber,
            };
            ProcessMercadoPagoPayment processMercadoPagoPayment = new ProcessMercadoPagoPayment
                                                                        (
                                                                            payer,
                                                                            paymentInformation.Installments,
                                                                            paymentInformation.Card,
                                                                            paymentInformation.Amount,
                                                                            campaign.Name
                                                                        );
            processMercadoPagoPayment.application_fee = (decimal)(paymentInformation.Amount * 0.05m);
            try
            {
                var apiResponse = await HttpClientHelper.PostAsync(mercadoPagoApi, processMercadoPagoPayment, headers: headers);
                if (apiResponse is null)
                {
                    return null;
                }
                var response = JsonConvert.DeserializeObject<ProcessMercadoPagoPaymentResponse>(apiResponse);
                return response;
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
           
        }
        private async Task<BaseValidation> ProcessPaymentDetail(ProcessMercadoPagoPaymentResponse payment, PaymentInformation paymentInfo, string campaignCreatorUserId, int campaignId, string? userId)
        {
            PaymentDetail paymentDetail = new PaymentDetail();
            paymentDetail.Installments = payment.installments;
            paymentDetail.Currency = payment.currency_id.ToUpper();
            paymentDetail.PaymentDateUTC = DateTime.UtcNow;
            paymentDetail.CardHolderEmail = paymentInfo.Card.CardHolderEmail;
            paymentDetail.CardHolderName = paymentInfo.Card.CardHolderName;
            paymentDetail.ChargeId = payment.id;
            paymentDetail.UserId = userId;
            paymentDetail.CampaignId = campaignId;
            if (payment.status != MP_STATUS.rejected.ToString() && payment.status != MP_STATUS.approved.ToString() && payment.status != MP_STATUS.pending.ToString() && payment.status != MP_STATUS.in_process.ToString())
            {
                throw new Exception("STATUS UNKNOWED");
            }
            if ((payment.status == MP_STATUS.pending.ToString() || payment.status.ToString() == MP_STATUS.in_process.ToString()) && payment.status_detail != null && payment.status_detail != "pending_challenge")
            {
                var paymentUpdated = await getPaymentNotPending(paymentDetail.ChargeId, campaignCreatorUserId);
                if (paymentUpdated != null)
                {
                    payment = paymentUpdated;
                }
            }

            if (payment.status == MP_STATUS.approved.ToString() && payment.status_detail == MP_STATUS_DETAIL.accredited.ToString())
            {
                paymentDetail.Status = Status.OK;
                paymentDetail.Brand = payment.payment_method_id != null ? payment.payment_method_id.ToUpper() : string.Empty;
                paymentDetail.ExpirationMonth = Convert.ToInt32(payment.card.expiration_month);
                paymentDetail.ExpirationYear = Convert.ToInt32(payment.card.expiration_year);
                paymentDetail.Last4 = payment.card.last_four_digits;
                paymentDetail.Funding = payment.payment_type_id.ToString().ToUpper().Replace("_", " ");
                paymentDetail.Amount = payment.transaction_amount;
                paymentDetail.TotalPaidAmount = payment.transaction_details.total_paid_amount;
                paymentDetail.NetReceivedAmount = payment.transaction_details.net_received_amount;
                paymentDetail.SupportmeCommission = !payment.fee_details.IsNullOrEmpty() ? payment.fee_details.Where(x => x.type == "application_fee").Select(x => x.amount).FirstOrDefault(0) : 0;
                paymentDetail.MPCommission = !payment.fee_details.IsNullOrEmpty() ? payment.fee_details.Where(x => x.type == "mercadopago_fee").Select(x => x.amount).FirstOrDefault(0) : 0;

            }
            else if (payment.status == MP_STATUS.rejected.ToString())
            {
                paymentDetail.Status = Status.ERROR;
                paymentDetail.Brand = payment.payment_method_id != null ? payment.payment_method_id.ToUpper() : string.Empty;
                paymentDetail.ExpirationMonth = Convert.ToInt32(payment.card.expiration_month);
                paymentDetail.ExpirationYear = Convert.ToInt32(payment.card.expiration_year);
                paymentDetail.Last4 = payment.card.last_four_digits;
                paymentDetail.Funding = payment.payment_type_id.ToString().ToUpper().Replace("_", " ");
                setRejectedCodes(paymentDetail, payment.status_detail.ToString());
            }
            _context.Add(paymentDetail);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(paymentInfo.Description)) 
            {
                PaymentComments comment = new PaymentComments();
                comment.Comment = paymentInfo.Description;
                comment.PaymentId = paymentDetail.Id;
                _context.Add(comment);
                await _context.SaveChangesAsync();
            }

            BaseValidation response = new BaseValidation(paymentDetail);

            return response;
        }

        public async Task<ProcessMercadoPagoPaymentResponse> getPaymentNotPending(string paymentId, string userId)
        {
            int retries = 5;
            int timeout = 2000;
            ProcessMercadoPagoPaymentResponse result = null;
            bool keepRetrying = false;
            do
            {
                keepRetrying = false;
                result = await this.GetPayment(paymentId, userId);
                if (result != null && result.status.ToString() == MP_STATUS.pending.ToString() || result.status == MP_STATUS.in_process.ToString())
                {
                    retries--;
                    await Task.Delay(timeout);
                    timeout = (int)(1.5 * timeout);
                    keepRetrying = retries > 0;
                }
            } while (keepRetrying);

            return result;
        }

        public async Task<ProcessMercadoPagoPaymentResponse> GetPayment(string id, string userId)
        {
            var mpAccount = await _context.UserMercadoPago.Where(x => x.UserId == userId).FirstOrDefaultAsync();

            string mercadoPagoApi = $"{_configuration.GetValue<string>("MERCADO_PAGO_API")}/v1/payments/{id}";
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", $"Bearer {mpAccount.Token}");

            var apiResponse = await HttpClientHelper.GetAsync(mercadoPagoApi, headers: headers);
            if (apiResponse is null)
            {
                return null;
            }
            var response = JsonConvert.DeserializeObject<ProcessMercadoPagoPaymentResponse>(apiResponse);
            return response;
        }

        private static void setRejectedCodes(PaymentDetail paymentDetail, string statusDetail)
        {
            paymentDetail.ErrorDescription = statusDetail;

            switch (statusDetail)
            {
                case "cc_rejected_bad_filled_card_number":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.INVALID_CARD.ToString();
                    break;

                case "cc_rejected_bad_filled_date":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.EXPIRED_CARD.ToString();
                    break;

                case "cc_rejected_bad_filled_other":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.INCORRECT_DATA.ToString();
                    break;

                case "cc_rejected_bad_filled_security_code":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.INCORRECT_CVC.ToString();
                    break;

                case "cc_rejected_blacklist":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.CARD_DECLINED.ToString();
                    break;

                case "cc_rejected_call_for_authorize":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.CALL_CARD_ISSUER.ToString();
                    break;

                case "cc_rejected_card_disabled":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.CARD_DISABLED.ToString();
                    break;

                case "cc_rejected_card_error":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.CARD_DECLINED.ToString();
                    break;

                case "cc_rejected_duplicated_payment":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.DUPLICATE_TRANSACTION.ToString();
                    break;

                case "cc_rejected_high_risk":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.HIGH_RISK_TRANSACTION.ToString();
                    break;

                case "cc_rejected_insufficient_amount":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.INSUFFICIENT_FUNDS.ToString();
                    break;

                case "cc_rejected_invalid_installments":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.INVALID_INSTALLMENT.ToString();
                    break;

                case "cc_rejected_max_attempts":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.MAX_ATTEMPTS.ToString();
                    break;

                case "cc_rejected_other_reason":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.CARD_DECLINED.ToString();
                    break;

                case "cc_rejected_3ds_challenge":
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.THREEDS_CHALLENGE_REJECTED.ToString();
                    break;

                default:
                    paymentDetail.ErrorCode = PaymentDetail.ErrorCodes.CARD_DECLINED.ToString();
                    break;
            }
        }

        public async Task<PaymentLiveFeed> GetPayments(string userId, PaymentFilter filter) 
        {

            DateTime? from = filter.From.HasValue ? DateHelper.GetUTCDateFromLocalDate(filter.From.Value, "ARS", -180) : null;
            DateTime? to = filter.To.HasValue ? DateHelper.GetUTCDateFromLocalDate(filter.To.Value.AddDays(1).AddSeconds(-1), "ARS", -180) : null;

            var query = _context.PaymentDetail.Include(x => x.Campaign).Where(x => x.Campaign.UserId == userId);

            if (from.HasValue)
            {
                query = query.Where(x => x.PaymentDateUTC >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(x => x.PaymentDateUTC <= to.Value);
            }

            if (!filter.Brand.IsNullOrEmpty()) 
            {
                if (filter.Brand.Any(x => x.Equals("Mastercard", StringComparison.OrdinalIgnoreCase)))
                {
                    filter.Brand.Add("MASTER");
                }


                query = query.Where(x => filter.Brand.Contains(x.Brand));
            }

            if (!filter.CampaignId.IsNullOrEmpty())
            {
                query = query.Where(x => filter.CampaignId.Contains(x.CampaignId));
            }

            var statusCounts = await query
                    .GroupBy(p => p.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);

            var okRegisters = statusCounts.GetValueOrDefault(Status.OK, 0);
            var errorRegisters = statusCounts.GetValueOrDefault(Status.ERROR, 0);
            var refundRegisters = statusCounts.GetValueOrDefault(Status.REFUNDED, 0);
            var total = okRegisters + errorRegisters + refundRegisters;

            if (!filter.Status.IsNullOrEmpty())
            {
                query = query.Where(x => filter.Status.Contains(x.Status));
            }

            query = SortingHelper.ApplyMultipleSortingAndPagination(query, filter, true);
            PaginationDTO<PaymentDetailRead> pag = new PaginationDTO<PaymentDetailRead>();


            pag.Items = await query.Select(x => new PaymentDetailRead 
            {
                Campaign = new DTOs.CampaignDTOs.SimpleCampaignRead 
                {
                    Id = x.Campaign.Id,
                    LogoUrl = x.Campaign.MainImage,
                    Name = x.Campaign.Name,
                },
                Amount = x.Amount,
                ChargeId = x.ChargeId,
                Brand = x.Brand,
                CustomerName = x.CardHolderName,
                Last4 = x.Last4,
                PaymentDate = DateHelper.GetDateInZoneTime(x.PaymentDateUTC, "arg", -180),
                Status = x.Status,
            }).ToListAsync();

            PaymentLiveFeed response = new PaymentLiveFeed();
            response.Items = pag;
            response.TotalRegisters = total;
            response.TotalOk = okRegisters;
            response.TotalError = errorRegisters;
            response.TotalRefunded = refundRegisters;

            return response;
        }
        public async Task<SimpleDonation> GetPayments(string chargeId)
        {
            var response = await _context.PaymentDetail.Include(x => x.Campaign).Where(x => x.ChargeId == chargeId)
                .Select(x => new SimpleDonation { Amount = x.Amount, CampaignName = x.Campaign.Name, DonatorName = x.CardHolderName }).FirstOrDefaultAsync();
            return response;
        }
        public async Task<PaymentDetailDTO> GetPaymentDetail(string chargeId, string userId)
        {
            var response = await _context.PaymentDetail.Include(x => x.Campaign).Where(x => x.ChargeId == chargeId && x.Campaign.UserId == userId)
                .Select(x => new PaymentDetailDTO 
                {
                    Amount = x.Amount, 
                    CampaignName = x.Campaign.Name, 
                    DonatorName = x.CardHolderName,
                    Brand = x.Brand,
                    Last4 = x.Last4,
                    Date = DateHelper.GetDateInZoneTime(x.PaymentDateUTC, "ARG", -180),
                    Status = x.Status.ToString(),
                    Comment = _context.PaymentComments.Where(c => c.PaymentId == x.Id).Select(c => c.Comment).FirstOrDefault(),
                    CommissionMP = x.MPCommission,
                    CommissionSupportMe = x.SupportmeCommission,
                    NetReceived = x.NetReceivedAmount
                })
                .FirstOrDefaultAsync();
            return response;
        }
    }
}
