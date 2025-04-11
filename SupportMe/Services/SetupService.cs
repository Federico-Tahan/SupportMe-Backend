using Microsoft.EntityFrameworkCore;
using SupportMe.Data;
using SupportMe.DTOs.SetupDTOs;

namespace SupportMe.Services
{
    public class SetupService
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public SetupService(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<SetupDTO> GetSetup(string userId) 
        {
            SetupDTO setup = new SetupDTO();
            var hasMercadoPago = await _context.UserMercadoPago.Where(x => x.UserId == userId).AnyAsync();
            setup.HasMercadoPagoConfigured = hasMercadoPago;
            return setup;

        }
    }
}
