using MediatR;
using DocMan.Core.Features.Categories;
using DocMan.Model.Dtos.Categories;
using DocMan.Core.Features.Categories.Query;
using DocMan.Core.Features.Categories.Command;

namespace DocMan.API.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetCategories)
            .WithName("GetCategories")
            .WithSummary("Get all user categories");

        group.MapGet("/{categoryId}", GetCategoryById)
            .WithName("GetCategoryById")
            .WithSummary("Get a specific category by ID");

        group.MapPost("/", CreateCategory)
            .WithName("CreateCategory")
            .WithSummary("Create a new category");

        group.MapPut("/{categoryId}", UpdateCategory)
            .WithName("UpdateCategory")
            .WithSummary("Update a category");

        group.MapDelete("/{categoryId}", DeleteCategory)
            .WithName("DeleteCategory")
            .WithSummary("Delete a category");
    }

    private static async Task<IResult> GetCategories(IMediator mediator)
    {
        var query = new GetCategoriesQuery();
        var categories = await mediator.Send(query);

        return Results.Ok(categories);
    }

    private static async Task<IResult> GetCategoryById(Guid categoryId, IMediator mediator)
    {
        var query = new GetCategoryByIdQuery { CategoryId = categoryId };
        var category = await mediator.Send(query);

        return category != null ? Results.Ok(category) : Results.NotFound();
    }

    private static async Task<IResult> CreateCategory(CreateCategoryRequest request, IMediator mediator)
    {
        var command = new CreateCategoryCommand
        {
            Name = request.Name,
            Description = request.Description,
            Color = request.Color
        };

        var result = await mediator.Send(command);
        return Results.Created($"/api/categories/{result.Id}", result);
    }

    private static async Task<IResult> UpdateCategory(Guid categoryId, UpdateCategoryRequest request, IMediator mediator)
    {
        var command = new UpdateCategoryCommand
        {
            CategoryId = categoryId,
            Name = request.Name,
            Description = request.Description,
            Color = request.Color
        };

        var result = await mediator.Send(command);
        return result ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> DeleteCategory(Guid categoryId, IMediator mediator)
    {
        var command = new DeleteCategoryCommand { CategoryId = categoryId };
        var success = await mediator.Send(command);

        return success ? Results.NoContent() : Results.NotFound();
    }
}

