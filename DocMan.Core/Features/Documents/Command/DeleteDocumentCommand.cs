using MediatR;
using DocMan.Core.Services;

namespace DocMan.Core.Features.Documents.Command;

public class DeleteDocumentCommand : IRequest<bool>
{
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
}

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IDocumentManagementService _documentService;

    public DeleteDocumentCommandHandler(IDocumentManagementService documentService)
    {
        _documentService = documentService;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        return await _documentService.DeleteDocumentAsync(request.DocumentId, request.UserId, cancellationToken);
    }
}

