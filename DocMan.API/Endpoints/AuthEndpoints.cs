using MediatR;
using DocMan.Core.Features.Auth;
using DocMan.Model.Dtos.Auth;
using DocMan.Core.Features.Auth.Query;
using DocMan.Core.Features.Auth.Command;

namespace DocMan.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login with username and password")
            .AllowAnonymous();

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user")
            .AllowAnonymous();

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user information")
            .RequireAuthorization();

        group.MapPut("/theme", UpdateTheme)
            .WithName("UpdateTheme")
            .WithSummary("Update user theme preference")
            .RequireAuthorization();
    }

    private static async Task<IResult> Login(LoginRequest request, IMediator mediator)
    {
        var command = new LoginCommand { Username = request.Username, Password = request.Password };
        var result = await mediator.Send(command);

        if (result == null)
            return Results.Unauthorized();

        return Results.Ok(result);
    }

    private static async Task<IResult> Register(RegisterRequest request, IMediator mediator)
    {
        var command = new RegisterCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName
        };

        var result = await mediator.Send(command);

        if (result == null)
            return Results.BadRequest("User already exists");

        return Results.Created($"/api/auth/me", result);
    }

    private static async Task<IResult> GetCurrentUser(HttpContext context, IMediator mediator)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        var query = new GetUserQuery { UserId = userId };
        var user = await mediator.Send(query);

        if (user == null)
            return Results.NotFound();

        return Results.Ok(user);
    }

    private static async Task<IResult> UpdateTheme(UpdateThemeRequest request, HttpContext context, IMediator mediator)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        var command = new UpdateThemeCommand { UserId = userId, Theme = request.Theme };
        var success = await mediator.Send(command);

        return success ? Results.Ok() : Results.NotFound();
    }
}

