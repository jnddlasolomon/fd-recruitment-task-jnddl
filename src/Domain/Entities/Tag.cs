using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo_App.Domain.Entities;

public class Tag : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = "#6b7280"; // Default gray color

    // Navigation property for many-to-many relationship
    public ICollection<TodoItemTag> TodoItemTags { get; set; } = new List<TodoItemTag>();
}
