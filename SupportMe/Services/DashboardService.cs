using Microsoft.EntityFrameworkCore;
using SupportMe.Data;
using SupportMe.DTOs.DashboardDTOs;
using SupportMe.DTOs.MercadoPagoDTOs;
using SupportMe.Helpers;
using static SupportMe.Helpers.DateHelper;

namespace SupportMe.Services
{
    public class DashboardService
    {
        private readonly DataContext _context;

        public DashboardService(DataContext context)
        {
            _context = context;
        }

        public async Task<SummaryResponse> GetSummary(DashboardFilter filter, string userId) 
        {
            SummaryResponse response = new SummaryResponse();
            DateTime? from = null;
            DateTime? to = null;
            DateHelper.SetUTCFromToDate(filter.From, filter.To, out from, out to, "ARG", -180);

            var query =  _context.PaymentDetail.Where(x => x.Campaign.UserId == userId && x.Status == Models.Status.OK);

            if (from.HasValue) 
            {
                query = query.Where(x => x.PaymentDateUTC >= from.Value);
            }
            if (to.HasValue) 
            {
                query = query.Where(x => x.PaymentDateUTC <= to.Value);
            }

            var totalIncome = await query.SumAsync(x => x.Amount);
            var totalDonations = await query.CountAsync();
            var avgIncome = totalIncome > 0 &&  totalDonations > 0? totalIncome / totalDonations : 0;
            var visitQuery = _context.CampaignView.Where(x => x.Campaign.UserId == userId);
            if (from.HasValue)
            {
                visitQuery = visitQuery.Where(x => x.DateUTC >= from.Value);
            }
            if (to.HasValue)
            {
                visitQuery = visitQuery.Where(x => x.DateUTC <= to.Value);
            }

            var visits = await visitQuery.CountAsync();

            response.Visit = visits;
            response.AVGIncome = avgIncome;
            response.Income = totalIncome;
            response.Donations = totalDonations;
            return response;
        }


        public async Task<CampaignStatistic> GetCampaignStatistics(DashboardFilter filter, string userId)
        {
            CampaignStatistic response = new CampaignStatistic();
            DateTime? from = null;
            DateTime? to = null;
            DateHelper.SetUTCFromToDate(filter.From, filter.To, out from, out to, "ARG", -180);

            var query = _context.PaymentDetail.Where(x => x.Campaign.UserId == userId && x.CampaignId == filter.CampaignId && x.Status == Models.Status.OK);
            var campaign = await _context.Campaigns.Where(x => x.Id == filter.CampaignId && x.UserId == userId).FirstOrDefaultAsync();

            if (from.HasValue)
            {
                query = query.Where(x => x.PaymentDateUTC >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(x => x.PaymentDateUTC <= to.Value);
            }
            var totalIncome = await query.SumAsync(x => x.Amount);
            var totalDonations = await query.CountAsync();

            response.Income = totalIncome;
            response.Goal = campaign.GoalAmount;
            response.Id = campaign.Id;
            response.Name = campaign.Name;
            response.GoalDate = campaign.GoalDate;
            response.RemaigninDays = campaign.GoalDate.HasValue
                ? (int?)(campaign.GoalDate.Value.Date - DateTime.UtcNow.Date).TotalDays
                : null;
            response.Donations = totalDonations;
            return response;
        }

        public async Task<GraphResponse<string, decimal>> GetDonationsIncome(DashboardFilter filter, string userId)
        {

            var filterType = ObtainGroupType(filter.From, filter.To);
            var query = _context.PaymentDetail.Include(x => x.Campaign).Where(x => x.Campaign.UserId == userId && x.CampaignId == filter.CampaignId && x.Status == Models.Status.OK)
                                              .Select(x => new 
                                              {
                                                 PaymentDate = x.PaymentDateUTC.AddMinutes(-180),
                                                 Amount = x.Amount,
                                              });
            if (filter.From.HasValue)
            {
                query = query.Where(x => x.PaymentDate >= filter.From.Value);
            }
            if (filter.To.HasValue)
            {
                query = query.Where(x => x.PaymentDate <= filter.To.Value.AddDays(1).AddTicks(-1));
            }
            List<GroupGraph<DateTime, decimal>> items = new List<GroupGraph<DateTime, decimal>>();

            switch (filterType)
            {
                case DatePart.YEAR:
                    items = await query.GroupBy(o => new { o.PaymentDate.Year })
                                      .Select(g => new GroupGraph<DateTime, decimal>
                                      {
                                          Key = new DateTime(g.Key.Year, 01, 01),
                                          Value = g.Sum(x => x.Amount),
                                      }).ToListAsync();
                    break;
                case DatePart.MONTH:
                    items = await query.GroupBy(o => new { o.PaymentDate.Year, o.PaymentDate.Month })
                                      .Select(g => new GroupGraph<DateTime, decimal>
                                      {
                                          Key = new DateTime(g.Key.Year, g.Key.Month, 01),
                                          Value = g.Sum(x => x.Amount),
                                      }).ToListAsync();
                    break;
                case DatePart.DAY:
                    items = await query.GroupBy(o => new { o.PaymentDate.Year, o.PaymentDate.Month, o.PaymentDate.Day })
                                      .Select(g => new GroupGraph<DateTime, decimal>
                                      {
                                          Key = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                                          Value = g.Sum(x => x.Amount),
                                      }).ToListAsync();
                    break;
                case DatePart.HOUR:
                    items = await query.GroupBy(o => new { o.PaymentDate.Year, o.PaymentDate.Month, o.PaymentDate.Day, o.PaymentDate.Hour })
                                      .Select(g => new GroupGraph<DateTime, decimal>
                                      {
                                          Key = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                                          Value = g.Sum(x => x.Amount),
                                      }).ToListAsync();
                    break;
            }

            var missingDays = DateHelper.MissingDays(filter.From, filter.To, items.Select(s => s.Key).ToList(), filterType);

            var graphMissing = missingDays.Select(p => new GroupGraph<DateTime, decimal>
            {
                Key = p,
                Value = 0

            }).ToList();
            items.AddRange(graphMissing);


            List<GroupGraph<string, decimal>> itemsSUMMARY = new List<GroupGraph<string, decimal>>();
            itemsSUMMARY = items.OrderBy(p => p.Key).Select(p => new GroupGraph<string, decimal>
            {
                Key = filterType == DatePart.YEAR ? p.Key.Year.ToString() :
                        filterType == DatePart.MONTH ? p.Key.Month.ToString("d2") + "/" + p.Key.Year.ToString() :
                        filterType == DatePart.DAY ? p.Key.Day.ToString("d2") + "/" + p.Key.Month.ToString() + "/" + p.Key.Year.ToString("d2") :
                        p.Key.Hour.ToString() + ":00",
                Value = p.Value

            }).ToList();

            GraphResponse<string, decimal> response = new GraphResponse<string, decimal> { Group = filterType.ToString(), Items = itemsSUMMARY };
            return response;
        }


        public async Task<GraphResponse<string, int>> GetVisitsGraph(DashboardFilter filter, string userId)
        {

            var filterType = ObtainGroupType(filter.From, filter.To);
            var query = _context.CampaignView.Include(x => x.Campaign).Where(x => x.Campaign.UserId == userId && x.CampaignId == filter.CampaignId)
                                              .Select(x => new
                                              {
                                                  ViewDate = x.DateUTC.AddMinutes(-180),
                                                  View = 1
                                              });
            if (filter.From.HasValue)
            {
                query = query.Where(x => x.ViewDate >= filter.From.Value);
            }
            if (filter.To.HasValue)
            {
                query = query.Where(x => x.ViewDate <= filter.To.Value.AddDays(1).AddTicks(-1));
            }
            List<GroupGraph<DateTime, int>> items = new List<GroupGraph<DateTime, int>>();

            switch (filterType)
            {
                case DatePart.YEAR:
                    items = await query.GroupBy(o => new { o.ViewDate.Year })
                                      .Select(g => new GroupGraph<DateTime, int>
                                      {
                                          Key = new DateTime(g.Key.Year, 01, 01),
                                          Value = g.Sum(x => x.View),
                                      }).ToListAsync();
                    break;
                case DatePart.MONTH:
                    items = await query.GroupBy(o => new { o.ViewDate.Year, o.ViewDate.Month })
                                      .Select(g => new GroupGraph<DateTime, int>
                                      {
                                          Key = new DateTime(g.Key.Year, g.Key.Month, 01),
                                          Value = g.Sum(x => x.View),
                                      }).ToListAsync();
                    break;
                case DatePart.DAY:
                    items = await query.GroupBy(o => new { o.ViewDate.Year, o.ViewDate.Month, o.ViewDate.Day })
                                      .Select(g => new GroupGraph<DateTime, int>
                                      {
                                          Key = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                                          Value = g.Sum(x => x.View),
                                      }).ToListAsync();
                    break;
                case DatePart.HOUR:
                    items = await query.GroupBy(o => new { o.ViewDate.Year, o.ViewDate.Month, o.ViewDate.Day, o.ViewDate.Hour })
                                      .Select(g => new GroupGraph<DateTime, int>
                                      {
                                          Key = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                                          Value = g.Sum(x => x.View),
                                      }).ToListAsync();
                    break;
            }

            var missingDays = DateHelper.MissingDays(filter.From, filter.To, items.Select(s => s.Key).ToList(), filterType);

            var graphMissing = missingDays.Select(p => new GroupGraph<DateTime, int>
            {
                Key = p,
                Value = 0

            }).ToList();
            items.AddRange(graphMissing);


            List<GroupGraph<string, int>> itemsSUMMARY = new List<GroupGraph<string, int>>();
            itemsSUMMARY = items.OrderBy(p => p.Key).Select(p => new GroupGraph<string, int>
            {
                Key = filterType == DatePart.YEAR ? p.Key.Year.ToString() :
                        filterType == DatePart.MONTH ? p.Key.Month.ToString("d2") + "/" + p.Key.Year.ToString() :
                        filterType == DatePart.DAY ? p.Key.Day.ToString("d2") + "/" + p.Key.Month.ToString() + "/" + p.Key.Year.ToString("d2") :
                        p.Key.Hour.ToString() + ":00",
                Value = p.Value

            }).ToList();

            GraphResponse<string, int> response = new GraphResponse<string, int> { Group = filterType.ToString(), Items = itemsSUMMARY };

            return response;
        }



        private DatePart ObtainGroupType(DateTime? From, DateTime? To)
        {
            if (!From.HasValue)
            {
                return DatePart.YEAR;
            }
            var diffInDays = !To.HasValue ? (DateTime.UtcNow.Date - From.Value.Date).Days : (To.Value.Date - From.Value.Date).Days;

            DatePart filterType;
            filterType = diffInDays == 0 ? DatePart.HOUR :
                         diffInDays <= 31 ? DatePart.DAY :
                         diffInDays <= 365 ? DatePart.MONTH :
                         DatePart.YEAR;
            return filterType;
        }
    }
}
