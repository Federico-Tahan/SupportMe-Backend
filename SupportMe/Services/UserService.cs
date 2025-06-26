using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using Core.BusinessLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SupportMe.Data;
using SupportMe.DTOs.UserDTOs;
using SupportMe.Helpers;
using SupportMe.Models;
using SupportMe.Services.Auth;
using SupportMe.Services.Email;
using SupportMe.Services.Email.Views;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Web;

namespace SupportMe.Services
{
    public class UserService
    {
        private readonly DataContext _context;
        private readonly FirebaseAuthService _firebaseAuthService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly NotificationService _notificationService;
        public UserService(DataContext context, FirebaseAuthService firebaseAuthService, IMapper mapper, IConfiguration configuration, NotificationService notificationService)
        {
            _context = context;
            _firebaseAuthService = firebaseAuthService;
            _mapper = mapper;
            _configuration = configuration;
            _notificationService = notificationService;
        }



        public async Task SendEmail() 
        {
            var user = await _context.Users.Where(x => x.Email == "fedetahan8@gmail.com").FirstOrDefaultAsync();
            RegisterModel register = new RegisterModel();
            register.User = user;
            SupportMe.Models.Email email = new SupportMe.Models.Email("Registro de usuario", "~/Services/Email/Views/RegisterUser.cshtml", user.Email, register);
            EmailFactory.SendEmail(email, _context);
        }
        public async Task<bool> RegisterUser(RegisterUserDTO request) 
        {
            var success = false;
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var user = _mapper.Map<User>(request);
                user.Name = request.FirstName;
                user.Id = Guid.NewGuid().ToString();
                user.CreatedDateUTC = DateTime.UtcNow;
                _context.Add(user);
                await _context.SaveChangesAsync();

                var firebaseRegister = await _firebaseAuthService.CreateUserFireBase(user, request.Password);
                if (!firebaseRegister.Success) 
                {
                    throw new Exception("CANNONT_REGISTER_FIREBASE");
                }
            
                await transaction.CommitAsync();
                RegisterModel register = new RegisterModel();
                register.User = user;
                SupportMe.Models.Email email = new SupportMe.Models.Email("Registro de usuario", "~/Services/Email/Views/RegisterUser.cshtml", user.Email, register);
                EmailFactory.SendEmail(email, _context);
                success = true;
            }
            catch (Exception e) 
            {
                await transaction.RollbackAsync();            
            }
            return success;
        }

        public async Task<User> GetUserById(string id)
        {
            var user = await _context.Users.FindAsync(id);
            return user;
        }

        public async Task<User> GetUserByToken(string token) 
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var userId = this.DecodeToken(token);

            if (string.IsNullOrEmpty(userId))
                return null;

            var user = await GetUserById(userId);
            return user;
        }

        public async Task<bool> ChangePassword(string token, string password)
        {

            var decrypted = ForgotPasswordToken.Decrypt(token, _configuration);
            if (decrypted is null || decrypted.HasExpired()) 
            {
                return false;
            }

            var user = await _context.Users.Where(x => x.Email == decrypted.Email).FirstOrDefaultAsync();

            await _firebaseAuthService.ChangePassword(user, password);

            return true;
        }


        public async Task<RecoveryPasswordUserData> GetRecoveryDataUser(string token)
        {

            var decrypted = ForgotPasswordToken.Decrypt(token, _configuration);
            if (decrypted is null || decrypted.HasExpired())
            {
                throw new Exception("INVALID_TOKEN");
            }

            var user = await _context.Users.Where(x => x.Email == decrypted.Email)
                .Select(x => new RecoveryPasswordUserData 
                {
                    Email = x.Email,
                    FirstName = x.Name,
                    LastName = x.LastName
                }).FirstOrDefaultAsync();

            return user;
        }

        public async Task<SimpleUserInfo> GetProfile(string userId)
        {
            var user = await _context.Users.Where(x => x.Id == userId)
                .Select(x => new SimpleUserInfo 
                {
                    DateOfBirth = x.DateOfBirth,
                    Email = x.Email,
                    LastName = x.LastName,
                    Name = x.Name,
                    ProfilePic = x.ProfilePic,
                    DonationsCount = _context.PaymentDetail.Where(c => c.UserId == x.Id && c.Status == Status.OK).Count(),
                    TotalDonated = _context.PaymentDetail.Where(c => c.UserId == x.Id && c.Status == Status.OK).Sum(x => x.Amount),
                    CreatedDate = DateHelper.GetDateInZoneTime(x.CreatedDateUTC, "ARG", -180)
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task ForgotPassword(string email)
        {

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return;

            var token = new ForgotPasswordToken(email, TimeSpan.FromHours(1));
            string encrypted = HttpUtility.UrlEncode(token.Encrypt(_configuration));
            _notificationService.ForgotPassword($"{user?.Name} {user?.LastName}", email, encrypted);
        }
        private string DecodeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token.Replace("Bearer ", "")) as JwtSecurityToken;
            var userIdClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Sid)?.Value;
            return userIdClaim;
        }
    }
}
