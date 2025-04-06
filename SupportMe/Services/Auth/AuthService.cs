using Microsoft.EntityFrameworkCore;
using SupportMe.Data;
using SupportMe.DTOs.UserDTOs;
using SupportMe.Helpers;

namespace SupportMe.Services.Auth
{
    public class AuthService
    {
        private readonly DataContext _context;
        private FirebaseAuthService _firebaseAuthService { get; set; }
        private JwtConfig _jwtConfig { get; set; }
        public IConfiguration _configuration { get; set; }
        public AuthService(DataContext context, FirebaseAuthService firebaseAuthService, JwtConfig jwtConfig, IConfiguration configuration)
        {
            _context = context;
            _firebaseAuthService = firebaseAuthService;
            _jwtConfig = jwtConfig;
            _configuration = configuration;
        }

        public async Task<LoginDTO> CreateJwtFromFirebaseJwt(string firebaseJWT) 
        {
            var firebaseToken = await _firebaseAuthService.VerifyToken(firebaseJWT);
            var ExpirationMinutesToken = _configuration.GetValue<int>("JWT__ExpirationMinutes");
            var user = await _context.Users.Where(x => x.AuthExternalId == firebaseToken.Uid).FirstOrDefaultAsync();
            var jwt = JwtManager.GenerateToken(_jwtConfig, user, ExpirationMinutesToken);
            var token = new LoginDTO();
            token.Email = user.Email;
            token.FirstName = user.Name;
            token.LastName = user.Name;
            token.ProfilePic = user.ProfilePic;
            token.Token = jwt;
            return token;
        }

        public async Task<bool> IsAvailableEmail(string email)
        {
            var user = await _context.Users.AnyAsync(x => x.Email == email);
            return !user;
        }
    }
}
