using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.TodoLists.Commands.SoftDeleteTodoList;

public record SoftDeleteTodoListCommand : IRequest<Unit>
{
    public int Id { get; init; }
}

public class SoftDeleteTodoListCommandHandler : IRequestHandler<SoftDeleteTodoListCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public SoftDeleteTodoListCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(SoftDeleteTodoListCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoLists
            .IgnoreQueryFilters() // Include soft-deleted items to find the entity
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(TodoList), request.Id);
        }

        // Soft delete the list
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        // Soft delete all items in the list
        foreach (var item in entity.Items.Where(i => !i.IsDeleted))
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}