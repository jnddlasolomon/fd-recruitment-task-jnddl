using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo_App.Domain.Entities;
public class TodoItemTag : BaseAuditableEntity
{
    public int TodoItemId { get; set; }
    public TodoItem TodoItem { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
