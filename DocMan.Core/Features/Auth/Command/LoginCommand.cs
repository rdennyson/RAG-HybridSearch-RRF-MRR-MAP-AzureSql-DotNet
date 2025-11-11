using MediatR;
using DocMan.Core.Services;
using DocMan.Model.Dtos.Auth;

namespace DocMan.Core.Features.Auth.Command;

public class LoginCommand : IRequest<LoginResponse?>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse?>
{
    private readonly IAuthenticationService _authService;

    public LoginCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest
        {
            Username = request.Username,
            Password = request.Password
        };

        return await _authService.LoginAsync(loginRequest, cancellationToken);
    }
}

