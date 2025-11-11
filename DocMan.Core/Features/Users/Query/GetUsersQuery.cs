using MediatR;
using DocMan.Model.Dtos.Users;
using DocMan.Data.UnitOfWork;
using DocMan.Data.Repositories;

namespace DocMan.Core.Features.Users.Query;

public class GetUsersQuery : IRequest<List<UserDto>>
{
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.FindAsync(
            u => u.DeletedAt == null,
            cancellationToken
        );

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Theme = u.Theme,
            CreatedAt = u.CreatedAt
        }).ToList();
    }
}

