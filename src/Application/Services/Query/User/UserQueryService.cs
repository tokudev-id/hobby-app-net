using AutoMapper;
using HobbyApp.Application.DTOs;
using HobbyApp.Application.Services.Query.User;
using HobbyApp.Infrastructure.Repositories.Interfaces;

namespace HobbyApp.Application.Services.Query.User;

public class UserQueryService : IUserQueryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserQueryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDetailDto?> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return user != null ? _mapper.Map<UserDetailDto>(user) : null;
    }

    public async Task<PaginatedResult<UserListItemDto>> GetPagedAsync(int page, int size, string? search = null)
    {
        var users = await _unitOfWork.Users.GetPagedAsync(page, size, search);
        var totalCount = await _unitOfWork.Users.GetTotalCountAsync(search);

        return new PaginatedResult<UserListItemDto>
        {
            Items = _mapper.Map<List<UserListItemDto>>(users),
            TotalCount = totalCount,
            Page = page,
            Size = size
        };
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.Users.ExistsAsync(id);
    }
}

