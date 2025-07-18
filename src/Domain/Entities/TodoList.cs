using Todo_App.Domain.Common;

namespace Todo_App.Domain.Entities;

public class TodoList : BaseAuditableEntity, ISoftDeletable
{
    public string? Title { get; set; }

    public Colour Colour { get; set; } = Colour.White;

    // Feature 3: Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public IList<TodoItem> Items { get; private set; } = new List<TodoItem>();
}