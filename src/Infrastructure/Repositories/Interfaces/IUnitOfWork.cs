namespace HobbyApp.Infrastructure.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IHobbyRepository Hobbies { get; }
    IRoleRepository Roles { get; }
    IUserRoleRepository UserRoles { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

