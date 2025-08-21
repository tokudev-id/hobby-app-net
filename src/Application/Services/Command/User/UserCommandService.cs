using AutoMapper;
using HobbyApp.Application.DTOs;
using HobbyApp.Application.Services.Command.User;
using HobbyApp.Domain.Entities;
using HobbyApp.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HobbyApp.Application.Services.Command.User
{
    public class UserCommandService : IUserCommandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<Domain.Entities.User> _passwordHasher;

        public UserCommandService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordHasher = new PasswordHasher<Domain.Entities.User>();
        }

        public async Task<int> CreateAsync(CreateUserCommandDto dto)
        {
            // Check for existing username or email
            var existingUser = await _unitOfWork.Users.GetByUsernameAsync(dto.Username) ??
                              await _unitOfWork.Users.GetByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Username or email already exists");
            }

            // Validate hobbies for duplicates (case-sensitive)
            ValidateHobbiesForDuplicates(dto.Hobbies);

            // Map DTO to entity
            var user = _mapper.Map<Domain.Entities.User>(dto);

            // Clear any hobbies that might have been mapped to prevent duplicates
            user.Hobbies.Clear();

            // Hash password
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            // Create user
            var createdUser = await _unitOfWork.Users.CreateAsync(user);

            // Create hobbies for the user
            if (dto.Hobbies?.Any() == true)
            {
                foreach (var hobbyDto in dto.Hobbies)
                {
                    var hobby = _mapper.Map<Hobby>(hobbyDto);
                    hobby.UserId = createdUser.Id;
                    await _unitOfWork.Hobbies.CreateAsync(hobby);
                }
            }

            // Assign roles from DTO or default 'User' role
            var roleIds = dto.RoleIds?.Any() == true ? dto.RoleIds : new List<int>();
            
            // If no roles specified, assign default 'User' role
            if (!roleIds.Any())
            {
                var userRole = await _unitOfWork.Roles.GetByNameAsync("User");
                if (userRole != null)
                {
                    roleIds.Add(userRole.Id);
                }
            }

            // Assign all specified roles
            foreach (var roleId in roleIds)
            {
                var userRoleAssignment = new Domain.Entities.UserRole
                {
                    UserId = createdUser.Id,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = 1 // System assignment
                };
                await _unitOfWork.UserRoles.AddAsync(userRoleAssignment);
            }
            
            await _unitOfWork.SaveChangesAsync();

            return createdUser.Id;
        }

        public async Task UpdateAsync(UpdateUserCommandDto dto)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(dto.Id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Check if username or email is taken by another user
            var userWithSameUsername = await _unitOfWork.Users.GetByUsernameAsync(dto.Username);
            if (userWithSameUsername != null && userWithSameUsername.Id != dto.Id)
            {
                throw new InvalidOperationException("Username already exists");
            }

            var userWithSameEmail = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (userWithSameEmail != null && userWithSameEmail.Id != dto.Id)
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Validate hobbies for duplicates (case-sensitive)
            ValidateHobbiesForDuplicates(dto.Hobbies);

            // Update user properties
            _mapper.Map(dto, existingUser);
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Update hobbies
            await UpdateUserHobbiesAsync(existingUser, dto.Hobbies);

            await _unitOfWork.Users.UpdateAsync(existingUser);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            await _unitOfWork.Users.DeleteAsync(id);
        }

        private async Task UpdateUserHobbiesAsync(Domain.Entities.User user, List<HobbyItemDto> hobbyDtos)
        {
            // Remove existing hobbies
            var existingHobbies = await _unitOfWork.Hobbies.GetByUserIdAsync(user.Id);
            
            foreach (var hobby in existingHobbies)
            {
                await _unitOfWork.Hobbies.DeleteAsync(hobby.Id);
            }

            // Add new hobbies
            foreach (var hobbyDto in hobbyDtos)
            {
                var hobby = _mapper.Map<Hobby>(hobbyDto);
                hobby.UserId = user.Id;
                await _unitOfWork.Hobbies.CreateAsync(hobby);
            }
        }

        private void ValidateHobbiesForDuplicates(List<HobbyItemDto>? hobbies)
        {
            if (hobbies == null || !hobbies.Any())
                return;

            // Check for duplicate hobby names (case-sensitive)
            var hobbyNames = hobbies.Select(h => h.Name?.Trim()).Where(name => !string.IsNullOrEmpty(name)).ToList();
            var duplicateNames = hobbyNames.GroupBy(name => name, StringComparer.Ordinal)
                                          .Where(g => g.Count() > 1)
                                          .Select(g => g.Key)
                                          .ToList();

            if (duplicateNames.Any())
            {
                throw new InvalidOperationException($"Duplicate hobbies found: {string.Join(", ", duplicateNames)}. Each hobby name must be unique (case-sensitive).");
            }

            // Check for empty or whitespace-only hobby names
            var invalidHobbies = hobbies.Where(h => string.IsNullOrWhiteSpace(h.Name)).ToList();
            if (invalidHobbies.Any())
            {
                throw new InvalidOperationException("Hobby names cannot be empty or contain only whitespace.");
            }
        }
    }
}