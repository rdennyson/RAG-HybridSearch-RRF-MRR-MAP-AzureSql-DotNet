using MediatR;
using DocMan.Core.Services;
using DocMan.Model.Dtos.Documents;

namespace DocMan.Core.Features.Documents.Query;

public class GetDocumentByIdQuery : IRequest<DocumentDto?>
{
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
}

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto?>
{
    private readonly IDocumentManagementService _documentService;

    public GetDocumentByIdQueryHandler(IDocumentManagementService documentService)
    {
        _documentService = documentService;
    }

    public async Task<DocumentDto?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _documentService.GetDocumentAsync(request.DocumentId, request.UserId, cancellationToken);
    }
}

