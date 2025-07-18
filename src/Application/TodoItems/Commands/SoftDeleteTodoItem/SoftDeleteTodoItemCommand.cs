using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.TodoItems.Commands.SoftDeleteTodoItem;

public record SoftDeleteTodoItemCommand : IRequest<Unit>
{
    public int Id { get; init; }
}

public class SoftDeleteTodoItemCommandHandler : IRequestHandler<SoftDeleteTodoItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public SoftDeleteTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(SoftDeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoItems
            .IgnoreQueryFilters() // Include soft-deleted items to find the entity
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(TodoItem), request.Id);
        }

        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}