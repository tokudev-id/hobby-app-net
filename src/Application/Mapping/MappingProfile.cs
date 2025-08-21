using AutoMapper;
using HobbyApp.Application.DTOs;
using HobbyApp.Domain.Entities;

namespace HobbyApp.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.Hobbies, opt => opt.MapFrom(src => src.Hobbies));

        CreateMap<User, UserListItemDto>()
            .ForMember(dest => dest.HobbyCount, opt => opt.MapFrom(src => src.Hobbies.Count))
            .ForMember(dest => dest.Hobbies, opt => opt.MapFrom(src => src.Hobbies))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role)));

        CreateMap<CreateUserCommandDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Hobbies, opt => opt.Ignore());

        CreateMap<UpdateUserCommandDto, User>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Hobbies, opt => opt.Ignore());

        // Hobby mappings
        CreateMap<Hobby, HobbyItemDto>();
        CreateMap<HobbyItemDto, Hobby>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // Role mappings
        CreateMap<Role, RoleDto>();
    }
}

