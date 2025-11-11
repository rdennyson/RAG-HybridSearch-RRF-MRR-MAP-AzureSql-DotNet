using MediatR;
using DocMan.Core.Services;
using DocMan.Model.Dtos.Auth;

namespace DocMan.Core.Features.Auth.Command;

public class RegisterCommand : IRequest<LoginResponse?>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, LoginResponse?>
{
    private readonly IAuthenticationService _authService;

    public RegisterCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponse?> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterRequest
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName
        };

        return await _authService.RegisterAsync(registerRequest, cancellationToken);
    }
}

