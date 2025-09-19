
using AutoMapper;
using nauth_asp.Models;

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
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.TwoFactorAuthEntries, opt => opt.MapFrom(src => src._2FAEntries));

            CreateMap<DB_User, UserBasicDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

            CreateMap<CreateUserDTO, DB_User>()
                .ForMember(dest => dest.passwordHash, opt => opt.Ignore());


            // Session mappings
            CreateMap<DB_Session, SessionDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.userId.ToString()))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.serviceId.ToString()));
            CreateMap<DB_Session, SessionBasicDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.userId.ToString()))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.serviceId.ToString()));

            // Service mappings
            CreateMap<DB_Service, ServiceDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.userId.ToString()));
            CreateMap<DB_Service, ServiceBasicDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.userId.ToString()));

            // Permission mappings
            CreateMap<DB_Permission, PermissionDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId.ToString()));
            CreateMap<DB_Permission, PermissionBasicDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId.ToString()));

            CreateMap<CreatePermissionDTO, DB_Permission>()
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => long.Parse(src.ServiceId)));

            // UserPermission mappings
            CreateMap<DB_UserPermission, UserPermissionDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.permissionId.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.userId.ToString()));

            CreateMap<CreateUserPermissionDTO, DB_UserPermission>()
                .ForMember(dest => dest.permissionId, opt => opt.MapFrom(src => long.Parse(src.PermissionId)))
                .ForMember(dest => dest.userId, opt => opt.MapFrom(src => long.Parse(src.UserId)));

            // FullSessionDTO mappings
            CreateMap<DB_Session, FullSessionDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.userId.ToString()))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.serviceId.ToString()));

            // TwoFactorAuth mappings
            CreateMap<DB_2FAEntry, _2FAEntryDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

            // EmailAction mappings
            CreateMap<DB_EmailAction, EmailActionDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.userId.ToString()));

            CreateMap<DB_EmailAction, DecodedEmailActionDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

            //EmailTemplate mappings
            CreateMap<DB_EmailTemplate, EmailTemplateDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        }
    }
}
