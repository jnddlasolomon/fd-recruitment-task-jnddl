using Todo_App.Domain.Common;

namespace Todo_App.Domain.Entities;

public class TodoItem : BaseAuditableEntity, ISoftDeletable
{
    public int ListId { get; set; }

    public string? Title { get; set; }

    public string? Note { get; set; }

    public PriorityLevel Priority { get; set; }

    public DateTime? Reminder { get; set; }

    // Added new property Feature 1
    public Colour BackgroundColor { get; set; } = Colour.White;

    // Feature 3: Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    private bool _done;
    public bool Done
    {
        get => _done;
        set
        {
            if (value == true && _done == false)
            {
                AddDomainEvent(new TodoItemCompletedEvent(this));
            }

            _done = value;
        }
    }

    public TodoList List { get; set; } = null!;

    // Feature 2: Tags support Many-to-many relationship
    public ICollection<TodoItemTag> TodoItemTags { get; set; } = new List<TodoItemTag>();
}