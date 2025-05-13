using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SupportMe.Data;
using SupportMe.DTOs.CampaignDTOs;
using SupportMe.DTOs.ForgotPassword;
using SupportMe.DTOs.PaymentDTOs;
using SupportMe.Helpers;
using SupportMe.Models;
using SupportMe.Services.Email;

namespace SupportMe.Services
{
    public class NotificationService
    {
        public readonly DataContext _context;
        private readonly IConfiguration _configuration;
        public NotificationService(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

        public async void ForgotPassword(string userName, string email, string token)
        {
            try
            {
               
                var body = new ForgotPassword
                {
                   Url = $"{_configuration.GetValue<string>("SUPPORTME_PAGE")}/forgot?token={token}",
                   UserName = userName,

                };

                SupportMe.Models.Email emailb = new SupportMe.Models.Email("Recupera tu contraseña", "~/Services/Email/Views/ForgotPassword.cshtml", email, body);
                EmailFactory.SendEmail(emailb, _context);

            }
            catch
            {
                return;
            }

        }

        public async Task SendGoalDonationNotification(int campaignId)
        {
            try
            {
                var campaign = await _context.Campaigns
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(x => x.Id == campaignId);

                if (campaign == null || !campaign.GoalAmount.HasValue)
                {
                    return;
                }

                var goalAmount = campaign.GoalAmount.Value;

                // Obtiene total recaudado
                var amounts = await _context.PaymentDetail
                    .Where(x => x.CampaignId == campaignId && x.Status == Status.OK)
                    .GroupBy(x => x.CampaignId)
                    .Select(x => new
                    {
                        Donations = x.Count(),
                        Income = x.Sum(x => x.Amount),
                    })
                    .FirstOrDefaultAsync();

                if (amounts == null)
                {
                    return; // No hay donaciones
                }

                decimal percentageReached = (amounts.Income / goalAmount) * 100;

                // Busca última notificación enviada para la campaña
                var lastNotification = await _context.CampaignNotification
                    .Where(n => n.CampaignId == campaignId)
                    .OrderByDescending(n => n.DateUtc)
                    .FirstOrDefaultAsync();

                string nextNotification = null;

                if (percentageReached >= 100 && (lastNotification == null || lastNotification.NotificationType != "100"))
                {
                    nextNotification = "100";
                }
                else if (percentageReached >= 75 && (lastNotification == null || lastNotification.NotificationType != "75"))
                {
                    nextNotification = "75";
                }
                else if (percentageReached >= 50 && (lastNotification == null || lastNotification.NotificationType != "50"))
                {
                    nextNotification = "50";
                }

                if (nextNotification != null)
                {
                    // Guarda registro en CampaignNotification
                    var newNotification = new CampaignNotification
                    {
                        CampaignId = campaignId,
                        DateUtc = DateTime.UtcNow,
                        NotificationType = nextNotification
                    };
                    _context.CampaignNotification.Add(newNotification);
                    await _context.SaveChangesAsync();

                    // Prepara y envía email
                    var emailDto = new EmailNotificationDTO
                    {
                        GoalAmount = goalAmount,
                        Income = amounts.Income,
                        CampaignName = campaign.Name,
                        OwnerName = $"{campaign.User.Name} {campaign.User.LastName}",
                        Donations = amounts.Donations
                    };

                    var email = new SupportMe.Models.Email(
                        "¡Has alcanzado una nueva meta!",
                        "~/Services/Email/Views/GoalDonations.cshtml",
                        campaign.User.Email,
                        emailDto
                    );

                    EmailFactory.SendEmail(email, _context);
                }
            }
            catch (Exception ex)
            {
                // Aquí puedes loguear el error si lo deseas
                return;
            }

        }
    }
}
