using MediatR;
using DocMan.Core.Services;
using DocMan.Model.Dtos.Auth;
using DocMan.Model.Entities;

namespace DocMan.Core.Features.Auth.Query;

public class GetUserQuery : IRequest<User?>
{
    public Guid UserId { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User?>
{
    private readonly IAuthenticationService _authService;

    public GetUserQueryHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<User?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return await _authService.GetUserByIdAsync(request.UserId, cancellationToken);
    }
}

