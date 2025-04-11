using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SupportMe.Data;
using SupportMe.DTOs;
using SupportMe.DTOs.MercadoPagoDTOs;
using SupportMe.Helpers;
using SupportMe.Models;

namespace SupportMe.Services
{
    public class MercadoPagoService
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public MercadoPagoService(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public async Task<BaseValidation> DeleteToken(string userId)
        {
            BaseValidation response = new BaseValidation();

            try
            {
                await _context.UserMercadoPago.Where(x => x.UserId == userId).ExecuteDeleteAsync();
                response.Status = ValidationStatusCode.SUCCESS;
            }
            catch 
            {
            }
            return response;
        }
        public async Task<BaseValidation> ConnectOAuthAccount(string code, string userId)
        {
            BaseValidation response = new BaseValidation();

            //response.Status = ValidationStatusCode.SUCCESS;
            //return response;

            MercadopagoSetup mercadoPagoSetup = await _context.MercadopagoSetup.FirstOrDefaultAsync();
            string mercadoPagoApi = $"{_configuration.GetValue<string>("MERCADO_PAGO_API")}/oauth/token";
            Dictionary<string, string> headers = new Dictionary<string, string>();

            GenerateOAuthToken body = new GenerateOAuthToken
            {
                client_id = mercadoPagoSetup.ClientId,
                client_secret = mercadoPagoSetup.ClientSecret,
                code = code,
                grant_type = "authorization_code",
                redirect_uri = mercadoPagoSetup.CallBackUrl,
                test_token = mercadoPagoSetup.TestMode
            };


            var apiResponse = await HttpClientHelper.PostAsync(mercadoPagoApi, body, headers: headers);

            if (apiResponse is null)
            {
                return null;
            }
            var responseDeserialize = JsonConvert.DeserializeObject<OAuthMercadoPago>(apiResponse);
            UserMercadoPago userMercadoPago = new UserMercadoPago();
            userMercadoPago.public_key = responseDeserialize.public_key;
            userMercadoPago.refresh_token = responseDeserialize.refresh_token;
            userMercadoPago.MPUserId = responseDeserialize.MPUserId;
            userMercadoPago.Token = responseDeserialize.access_token;
            userMercadoPago.UserId = responseDeserialize.userId;
            userMercadoPago.CreatedDateUTC = DateTime.UtcNow;
            userMercadoPago.live_mode = responseDeserialize.live_mode;
            userMercadoPago.ExpirationSeconds = responseDeserialize.expires_in;

            _context.Add(userMercadoPago);
            await _context.SaveChangesAsync();

            response.Status = ValidationStatusCode.SUCCESS;
            return response;
        }
    }
}
