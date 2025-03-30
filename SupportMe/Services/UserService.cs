using AutoMapper;
using SupportMe.Data;
using SupportMe.DTOs.UserDTOs;
using SupportMe.Models;
using SupportMe.Services.Auth;
using SupportMe.Services.Email;

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


        public async Task RegisterUser(RegisterUserDTO request) 
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var user = _mapper.Map<User>(request);
                user.Id = Guid.NewGuid().ToString();
                _context.Add(user);
                await _context.SaveChangesAsync();

                var firebaseRegister = await _firebaseAuthService.CreateUserFireBase(user, request.Password);
                if (!firebaseRegister.Success) 
                {
                    throw new Exception("CANNONT_REGISTER_FIREBASE");
                }
                await transaction.CommitAsync();
                SupportMe.Models.Email email = new SupportMe.Models.Email("Registro de usuario", "Prueba", user.Email);
                EmailFactory.SendEmail(email, _context);
            }
            catch (Exception e) 
            {
                await transaction.RollbackAsync();
                throw e;
            }


        }
    }
}
