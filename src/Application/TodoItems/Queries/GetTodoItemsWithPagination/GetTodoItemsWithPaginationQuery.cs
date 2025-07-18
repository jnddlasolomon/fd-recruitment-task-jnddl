using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.Common.Mappings;
using Todo_App.Application.Common.Models;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.TodoItems.Queries.GetTodoItemsWithPagination;

public record GetTodoItemsWithPaginationQuery : IRequest<PaginatedList<TodoItemDto>>
{
    public int ListId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    // Feature 2: Filtering by tags
    public List<int> TagIds { get; init; } = new();

    // Feature 2: Text search
    public string? SearchTerm { get; init; }
}

public class GetTodoItemsWithPaginationQueryHandler : IRequestHandler<GetTodoItemsWithPaginationQuery, PaginatedList<TodoItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTodoItemsWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TodoItemDto>> Handle(GetTodoItemsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        IQueryable<TodoItem> query = _context.TodoItems
            .Where(x => x.ListId == request.ListId)
            .Include(x => x.TodoItemTags)
                .ThenInclude(tt => tt.Tag);

        // Feature 2: Filter by tags
        if (request.TagIds.Any())
        {
            query = query.Where(x => x.TodoItemTags.Any(tt => request.TagIds.Contains(tt.TagId)));
        }

        // Feature 2: Text search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower().Trim();
            query = query.Where(x =>
                (x.Title != null && x.Title.ToLower().Contains(searchTerm)) ||
                (x.Note != null && x.Note.ToLower().Contains(searchTerm)) ||
                x.TodoItemTags.Any(tt => tt.Tag.Name.ToLower().Contains(searchTerm)));
        }

        return await query
            .OrderBy(x => x.Title)
            .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}