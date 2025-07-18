using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.Common.Models;

namespace Todo_App.Application.Tags.Queries.GetTags;

public record GetTagsQuery : IRequest<List<TagDto>>;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tags
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
                Created = t.Created
            })
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }
}