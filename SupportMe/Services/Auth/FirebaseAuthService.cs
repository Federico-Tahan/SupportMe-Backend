using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using SupportMe.Data;
using SupportMe.DTOs.FirebaseDTOs;
using SupportMe.Helpers;
using SupportMe.Models;

namespace SupportMe.Services.Auth
{
    public class FirebaseAuthService
    {
        private readonly FirebaseHandler _firebaseHandler;
        private readonly DataContext _context;
        private IConfiguration _configuration;
        public FirebaseAuthService(FirebaseHandler firebaseHandler, DataContext context, IConfiguration configuration)
        {
            _firebaseHandler = firebaseHandler;
            _context = context;
            _configuration = configuration;
        }

        public async Task<RegisterFirebase> CreateUserFireBase(User user, string Password)
        {
            try
            {
                var firebaseTenantId = _configuration.GetValue<string>("TenantId");
                var fireBaseUser = await _firebaseHandler.Auth.CreateUserAsync(new UserRecordArgs
                {
                    Disabled = false,
                    Email = user.Email,
                    Password = Password,
                    DisplayName = $"{user.Name} {user.LastName}",
                    EmailVerified = true,
                });

                user.AuthExternalId = fireBaseUser.Uid;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                RegisterFirebase response = new RegisterFirebase { Id = fireBaseUser.Uid, Success = true };

                return response;
            }
            catch (Exception e)
            {
                //LogHelper.LogError($"FIREBASE RESPONSE : {e.Message} ");

                return new RegisterFirebase { Id = null, Success = false };
            }
        }
        public async Task<FirebaseToken> VerifyToken(string firebaseToken)
        {
            var decodedToken = await _firebaseHandler.Auth.VerifyIdTokenAsync(firebaseToken);
            if (decodedToken == null)
            {
                throw new Exception("not possible to decode");
            }
            return decodedToken;
        }

        public async Task<UserRecord> ChangePassword(User user, string newPassword)
        {
            try
            {
                var fireBaseUser = await _firebaseHandler.Auth
                                        .UpdateUserAsync(new UserRecordArgs
                                        {
                                            Email = user.Email,
                                            Password = newPassword,
                                            DisplayName = $"{user.Name} {user.LastName}",
                                            EmailVerified = true,
                                            Uid = user.AuthExternalId
                                        });
                return fireBaseUser;
            }
            catch
            {
                return null;

            }
        }
    }
}
