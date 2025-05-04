using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SupportMe.Data;
using SupportMe.DTOs.PaymentDTOs;
using SupportMe.Helpers;
using SupportMe.Models;
using SupportMe.Services.Email;

namespace SupportMe.Services
{
    public class NotificationService
    {
        public readonly DataContext _context;
        public NotificationService(DataContext context)
        {
            _context = context;
        }


        public async Task SendDonationToOwner(string chargeId) 
        {
            try
            {
                var result = await _context.PaymentDetail.Include(x => x.Campaign).Where(x => x.ChargeId == chargeId)
                           .Select(x => new
                           {
                               Amount = x.Amount,
                               CampaignName = x.Campaign.Name,
                               DonatorName = x.CardHolderName,
                               DonatorEmail = x.CardHolderEmail,
                               Brand = x.Brand,
                               Last4 = x.Last4,
                               Date = DateHelper.GetDateInZoneTime(x.PaymentDateUTC, "ARG", -180),
                               Status = x.Status.ToString(),
                               Comment = _context.PaymentComments.Where(c => c.PaymentId == x.Id).Select(c => c.Comment).FirstOrDefault(),
                               CommissionMP = x.MPCommission,
                               CommissionSupportMe = x.SupportmeCommission,
                               NetReceived = x.NetReceivedAmount,
                               OwnerEmail = x.Campaign.User.Email,
                           })
                           .FirstOrDefaultAsync();

                var body = new PaymentDetailDTO
                {
                    Amount = result.Amount,
                    CampaignName = result.CampaignName,
                    DonatorName = result.DonatorName,
                    Brand = result.Brand,
                    Last4 = result.Last4,
                    Date = result.Date,
                    Status = result.Status.ToString(),
                    Comment = result.Comment,
                    CommissionMP = result.CommissionMP,
                    CommissionSupportMe = result.CommissionSupportMe,
                    NetReceived = result.NetReceived,
                };

                SupportMe.Models.Email email1 = new SupportMe.Models.Email("Has recibido una donación", "~/Services/Email/Views/DonationToOwner.cshtml", result.OwnerEmail, body);
                EmailFactory.SendEmail(email1, _context);

                SupportMe.Models.Email email2 = new SupportMe.Models.Email("Has realizado una donacion", "~/Services/Email/Views/DonationToDonator.cshtml", result.DonatorEmail, body);
                EmailFactory.SendEmail(email2, _context);
            }
            catch 
            {
                return;
            }
            
        }
    }
}
