using MediatR;
using DocMan.Core.Features.Search;
using DocMan.Model.Dtos.Search;
using DocMan.Core.Features.Search.Query;

namespace DocMan.API.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/search")
            .WithTags("Search")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/query", SearchDocuments)
            .WithName("SearchDocuments")
            .WithSummary("Search documents using vector similarity");
    }

    private static async Task<IResult> SearchDocuments(SearchRequest request, HttpContext context, IMediator mediator)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        if (string.IsNullOrEmpty(request.Query))
            return Results.BadRequest("Query cannot be empty");

        try
        {
            var query = new SearchDocumentsQuery
            {
                UserId = userId,
                Query = request.Query,
                TopK = request.TopK,
                IncludeMetrics = request.IncludeMetrics,
                SearchMode = request.SearchMode,
                UseReranking = request.UseReranking
            };

            var result = await mediator.Send(query);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Search failed: {ex.Message}");
        }
    }
}

