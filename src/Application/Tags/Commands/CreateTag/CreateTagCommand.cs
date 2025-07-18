using MediatR;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.Tags.Commands.CreateTag;

public record CreateTagCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = "#6b7280";
}

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateTagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var entity = new Tag
        {
            Name = request.Name.Trim(),
            Color = request.Color
        };

        _context.Tags.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}