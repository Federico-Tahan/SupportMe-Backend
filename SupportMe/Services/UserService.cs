using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SupportMe.Data;
using SupportMe.DTOs.UserDTOs;
using SupportMe.Models;
using SupportMe.Services.Auth;
using SupportMe.Services.Email;
using SupportMe.Services.Email.Views;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SupportMe.Services
{
    public class UserService
    {
        private readonly DataContext _context;
        private readonly FirebaseAuthService _firebaseAuthService;
        private readonly IMapper _mapper;
        public UserService(DataContext context, FirebaseAuthService firebaseAuthService, IMapper mapper)
        {
            _context = context;
            _firebaseAuthService = firebaseAuthService;
            _mapper = mapper;
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
                SupportMe.Models.Email email = new SupportMe.Models.Email("Registro de usuario", "~Services/Email/Views/RegisterUser.cshtml", user.Email, register);
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


        private string DecodeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token.Replace("Bearer ", "")) as JwtSecurityToken;
            var userIdClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Sid)?.Value;
            return userIdClaim;
        }
    }
}
