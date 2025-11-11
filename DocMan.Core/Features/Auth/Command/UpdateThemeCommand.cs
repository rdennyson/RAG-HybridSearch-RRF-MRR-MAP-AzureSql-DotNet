using MediatR;
using DocMan.Core.Services;

namespace DocMan.Core.Features.Auth.Command;

public class UpdateThemeCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public string Theme { get; set; } = string.Empty;
}

public class UpdateThemeCommandHandler : IRequestHandler<UpdateThemeCommand, bool>
{
    private readonly IAuthenticationService _authService;

    public UpdateThemeCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        return await _authService.UpdateThemeAsync(request.UserId, request.Theme, cancellationToken);
    }
}

