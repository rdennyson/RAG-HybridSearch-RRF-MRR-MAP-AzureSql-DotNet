using MediatR;
using DocMan.Data.UnitOfWork;
using DocMan.Data.Repositories;

namespace DocMan.Core.Features.Categories.Command;

public class DeleteCategoryCommand : IRequest<bool>
{
    public Guid CategoryId { get; set; }
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null || category.DeletedAt != null)
            return false;

        // Soft delete
        category.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

