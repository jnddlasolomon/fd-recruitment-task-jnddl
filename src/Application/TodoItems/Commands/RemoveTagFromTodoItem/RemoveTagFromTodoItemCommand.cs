using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;

namespace Todo_App.Application.TodoItems.Commands.RemoveTagFromTodoItem;

public record RemoveTagFromTodoItemCommand : IRequest<Unit>
{
    public int TodoItemId { get; init; }
    public int TagId { get; init; }
}

public class RemoveTagFromTodoItemCommandHandler : IRequestHandler<RemoveTagFromTodoItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RemoveTagFromTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemoveTagFromTodoItemCommand request, CancellationToken cancellationToken)
    {
        var todoItemTag = await _context.TodoItemTags
            .FirstOrDefaultAsync(x => x.TodoItemId == request.TodoItemId && x.TagId == request.TagId, cancellationToken);

        if (todoItemTag != null)
        {
            _context.TodoItemTags.Remove(todoItemTag);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}