using MediatR;
using DocMan.Data.UnitOfWork;
using DocMan.Model.Dtos.Categories;

namespace DocMan.Core.Features.Categories.Query;

public class GetCategoryByIdQuery : IRequest<CategoryDto?>
{
    public Guid CategoryId { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(
            c => c.Id == request.CategoryId && c.DeletedAt == null,
            cancellationToken
        );

        if (category == null)
            return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt
        };
    }
}

