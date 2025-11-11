using MediatR;
using DocMan.Core.Services;
using DocMan.Model.Dtos.Documents;

namespace DocMan.Core.Features.Documents.Query;

public class GetDocumentsQuery : IRequest<IEnumerable<DocumentDto>>
{
    public Guid UserId { get; set; }
}

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, IEnumerable<DocumentDto>>
{
    private readonly IDocumentManagementService _documentService;

    public GetDocumentsQueryHandler(IDocumentManagementService documentService)
    {
        _documentService = documentService;
    }

    public async Task<IEnumerable<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        return await _documentService.GetUserDocumentsAsync(request.UserId, cancellationToken);
    }
}

