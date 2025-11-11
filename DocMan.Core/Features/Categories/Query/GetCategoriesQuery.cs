using MediatR;
using DocMan.Model.Dtos.Categories;
using DocMan.Data.UnitOfWork;
using DocMan.Data.Repositories;

namespace DocMan.Core.Features.Categories.Query;

public class GetCategoriesQuery : IRequest<List<CategoryDto>>
{
}

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.FindAsync(
            c => c.DeletedAt == null,
            cancellationToken
        );

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Color = c.Color,
            CreatedAt = c.CreatedAt
        }).ToList();
    }
}

