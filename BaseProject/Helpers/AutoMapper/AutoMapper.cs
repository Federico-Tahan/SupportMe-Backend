using AutoMapper;
using SupportMe.DTOs.UserDTOs;
using SupportMe.Models;

namespace SupportMe.Helpers.AutoMapper
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<RegisterUserDTO, User>().ReverseMap();
        }
    }
}
