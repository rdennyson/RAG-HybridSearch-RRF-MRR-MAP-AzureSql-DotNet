using MediatR;
using DocMan.Core.Features.Documents;
using DocMan.Core.Services;
using DocMan.Core.Services.SparseRetrieval;
using DocMan.Core.Features.Documents.Query;
using DocMan.Core.Features.Documents.Command;
using DocMan.Data.UnitOfWork;

namespace DocMan.API.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/documents")
            .WithTags("Documents")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetDocuments)
            .WithName("GetDocuments")
            .WithSummary("Get all user documents");

        group.MapGet("/{documentId}", GetDocumentById)
            .WithName("GetDocumentById")
            .WithSummary("Get a specific document by ID");

        group.MapPost("/upload", UploadDocument)
            .WithName("UploadDocument")
            .WithSummary("Upload a new document")
            .DisableAntiforgery();

        group.MapDelete("/{documentId}", DeleteDocument)
            .WithName("DeleteDocument")
            .WithSummary("Delete a document");
    }

    private static async Task<IResult> GetDocuments(HttpContext context, IMediator mediator)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        var query = new GetDocumentsQuery { UserId = userId };
        var documents = await mediator.Send(query);

        return Results.Ok(documents);
    }

    private static async Task<IResult> GetDocumentById(Guid documentId, HttpContext context, IMediator mediator)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        var query = new GetDocumentByIdQuery { DocumentId = documentId, UserId = userId };
        var document = await mediator.Send(query);

        return document != null ? Results.Ok(document) : Results.NotFound();
    }

    private static async Task<IResult> UploadDocument(HttpContext context, IFormFile file, IMediator mediator, IDocumentProcessingService processingService)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        if (file == null || file.Length == 0)
            return Results.BadRequest("No file provided");

        try
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var supportedExtensions = new[] { ".pdf", ".docx", ".txt", ".md" };

            if (!supportedExtensions.Contains(extension))
                return Results.BadRequest($"File type {extension} is not supported");

            // Create document record
            var documentService = context.RequestServices.GetRequiredService<IDocumentManagementService>();
            var document = await documentService.CreateDocumentAsync(
                userId,
                null,
                file.FileName,
                file.ContentType,
                extension,
                file.Length
            );

            // Process document
            using (var stream = file.OpenReadStream())
            {
                await processingService.ProcessDocumentAsync(document, stream);
            }

            // Initialize BM25 index for the newly processed document chunks
            var bm25Service = context.RequestServices.GetRequiredService<IBM25Service>();
            var unitOfWork = context.RequestServices.GetRequiredService<IUnitOfWork>();
            var chunks = await unitOfWork.DocumentChunks.GetChunksByDocumentAsync(document.Id);
            if (chunks.Any())
            {
                await bm25Service.IndexDocumentsAsync(chunks);
            }

            return Results.Created($"/api/documents/{document.Id}", new { document.Id, document.FileName });
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error uploading document: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteDocument(Guid documentId, HttpContext context, IMediator mediator)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Results.Unauthorized();

        var command = new DeleteDocumentCommand { DocumentId = documentId, UserId = userId };
        var success = await mediator.Send(command);

        return success ? Results.NoContent() : Results.NotFound();
    }
}

