using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.TodoItems.Commands.AddTagToTodoItem;

public record AddTagToTodoItemCommand : IRequest<Unit>
{
    public int TodoItemId { get; init; }
    public int TagId { get; init; }
}

public class AddTagToTodoItemCommandHandler : IRequestHandler<AddTagToTodoItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public AddTagToTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AddTagToTodoItemCommand request, CancellationToken cancellationToken)
    {
        // Verify TodoItem exists - Only check for ID to avoid NULL field issues
        var todoItemExists = await _context.TodoItems
            .AnyAsync(x => x.Id == request.TodoItemId, cancellationToken);

        if (!todoItemExists)
        {
            throw new NotFoundException(nameof(TodoItem), request.TodoItemId);
        }

        // Verify Tag exists - Only check for ID to avoid NULL field issues  
        var tagExists = await _context.Tags
            .AnyAsync(x => x.Id == request.TagId, cancellationToken);

        if (!tagExists)
        {
            throw new NotFoundException(nameof(Tag), request.TagId);
        }

        // Check if relationship already exists
        var existingRelation = await _context.TodoItemTags
            .FirstOrDefaultAsync(x => x.TodoItemId == request.TodoItemId && x.TagId == request.TagId, cancellationToken);

        if (existingRelation != null)
        {
            // Relationship already exists, don't add duplicate
            return Unit.Value;
        }

        // Create new relationship
        var todoItemTag = new TodoItemTag
        {
            TodoItemId = request.TodoItemId,
            TagId = request.TagId
        };

        _context.TodoItemTags.Add(todoItemTag);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}