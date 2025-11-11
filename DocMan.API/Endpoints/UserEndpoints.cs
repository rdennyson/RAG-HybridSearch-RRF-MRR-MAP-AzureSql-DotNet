using MediatR;
using DocMan.Core.Features.Users;
using DocMan.Model.Dtos.Users;
using DocMan.Core.Features.Users.Command;
using DocMan.Core.Features.Users.Query;

namespace DocMan.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get all users");

        group.MapGet("/{userId}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get a specific user by ID");

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user")
            .AllowAnonymous();

        group.MapPut("/{userId}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Update a user");

        group.MapDelete("/{userId}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Delete a user");
    }

    private static async Task<IResult> GetUsers(IMediator mediator)
    {
        var query = new GetUsersQuery();
        var users = await mediator.Send(query);

        return Results.Ok(users);
    }

    private static async Task<IResult> GetUserById(Guid userId, IMediator mediator)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        var user = await mediator.Send(query);

        return user != null ? Results.Ok(user) : Results.NotFound();
    }

    private static async Task<IResult> CreateUser(CreateUserRequest request, IMediator mediator)
    {
        var command = new CreateUserCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password
        };

        var result = await mediator.Send(command);
        if (result == null)
            return Results.BadRequest("User already exists");

        return Results.Created($"/api/users/{result.Id}", result);
    }

    private static async Task<IResult> UpdateUser(Guid userId, UpdateUserRequest request, HttpContext context, IMediator mediator)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            return Results.Unauthorized();

        // Users can only update their own profile
        if (currentUserId != userId)
            return Results.Forbid();

        var command = new UpdateUserCommand
        {
            UserId = userId,
            Email = request.Email
        };

        var result = await mediator.Send(command);
        return result ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> DeleteUser(Guid userId, HttpContext context, IMediator mediator)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            return Results.Unauthorized();

        // Users can only delete their own account
        if (currentUserId != userId)
            return Results.Forbid();

        var command = new DeleteUserCommand { UserId = userId };
        var success = await mediator.Send(command);

        return success ? Results.NoContent() : Results.NotFound();
    }
}

