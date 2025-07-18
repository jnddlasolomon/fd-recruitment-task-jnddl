using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.TodoItems.Commands.UpdateTodoItemTags;

public record UpdateTodoItemTagsCommand : IRequest<Unit>
{
    public int TodoItemId { get; init; }
    public List<int> TagIds { get; init; } = new();
}

public class UpdateTodoItemTagsCommandHandler : IRequestHandler<UpdateTodoItemTagsCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateTodoItemTagsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Unit> Handle(UpdateTodoItemTagsCommand request, CancellationToken cancellationToken)
    {
        // Verify TodoItem exists
        var todoItem = await _context.TodoItems
            .FirstOrDefaultAsync(x => x.Id == request.TodoItemId, cancellationToken);

        if (todoItem == null)
        {
            throw new NotFoundException(nameof(TodoItem), request.TodoItemId);
        }

        // Remove all existing tag relationships for this item
        var existingRelations = await _context.TodoItemTags
            .Where(x => x.TodoItemId == request.TodoItemId)
            .ToListAsync(cancellationToken);

        _context.TodoItemTags.RemoveRange(existingRelations);

        // Add new tag relationships
        if (request.TagIds.Any())
        {
            // Verify all tags exist
            var existingTagIds = await _context.Tags
                .Where(t => request.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            var invalidTagIds = request.TagIds.Except(existingTagIds).ToList();
            if (invalidTagIds.Any())
            {
                throw new NotFoundException($"Tags not found: {string.Join(", ", invalidTagIds)}");
            }

            // Create new relationships
            var newRelations = request.TagIds.Select(tagId => new TodoItemTag
            {
                TodoItemId = request.TodoItemId,
                TagId = tagId
            }).ToList();

            _context.TodoItemTags.AddRange(newRelations);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}