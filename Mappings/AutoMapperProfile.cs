
using AutoMapper;
using nauth_asp.Models;
using nauth_asp.Models.DTOs;
using nauth_asp.Models.EmailAction;
using nauth_asp.Models.Permissions;
using nauth_asp.Models.Service;
using nauth_asp.Models.Session;
using nauth_asp.Models.TwoFactorAuth;

namespace nauth_asp.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Configure global conventions
            SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
            DestinationMemberNamingConvention = new PascalCaseNamingConvention();

            // User mappings
            CreateMap<DB_User, UserDTO>()
                .ForMember(dest => dest.TwoFactorAuthEntries, opt => opt.MapFrom(src => src._2FAEntries));

            CreateMap<DB_User, UserBasicDTO>();

            CreateMap<CreateUserDTO, DB_User>()
                .ForMember(dest => dest.isEmailVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.passwordHash, opt => opt.Ignore())
                .ForMember(dest => dest.passwordSalt, opt => opt.Ignore());

            CreateMap<UpdateUserDTO, DB_User>()
                .ForMember(dest => dest.passwordHash, opt => opt.Ignore())
                .ForMember(dest => dest.passwordSalt, opt => opt.Ignore());

            // Session mappings
            CreateMap<DB_Session, SessionDTO>();

            // Service mappings
            CreateMap<DB_Service, ServiceDTO>();
            CreateMap<DB_Service, ServiceBasicDTO>();

            CreateMap<CreateServiceDTO, DB_Service>();

            CreateMap<UpdateServiceDTO, DB_Service>();

            // Permission mappings
            CreateMap<DB_Permission, PermissionDTO>();
            CreateMap<DB_Permission, PermissionBasicDTO>();

            CreateMap<CreatePermissionDTO, DB_Permission>();

            CreateMap<UpdatePermissionDTO, DB_Permission>();

            // UserPermission mappings
            CreateMap<DB_UserPermission, UserPermissionDTO>();

            CreateMap<CreateUserPermissionDTO, DB_UserPermission>();

            // UserService mappings
            CreateMap<DB_UserService, UserServiceDTO>();

            CreateMap<CreateUserServiceDTO, DB_UserService>();

            // TwoFactorAuth mappings
            CreateMap<DB_2FAEntry, TwoFactorAuthDTO>();

            // EmailAction mappings
            CreateMap<DB_EmailAction, EmailActionDTO>();
        }
    }
}
