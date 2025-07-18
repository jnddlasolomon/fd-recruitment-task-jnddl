using AutoMapper;
using Todo_App.Application.Common.Mappings;
using Todo_App.Application.Common.Models;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.TodoItems.Queries.GetTodoItemsWithPagination;

public class TodoItemDto
{
    public int Id { get; set; }

    public int ListId { get; set; }

    public string? Title { get; set; }

    public bool Done { get; set; }

    public int Priority { get; set; }

    public string? Note { get; set; }

    public DateTime? Reminder { get; set; }

    // Feature 1: Background color
    public int BackgroundColor { get; set; }

    // Feature 2: Tags
    public List<TagDto> Tags { get; set; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TodoItem, TodoItemDto>()
                .ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority))
                .ForMember(d => d.BackgroundColor, opt => opt.MapFrom(s => s.BackgroundColor.Code))
                .ForMember(d => d.Tags, opt => opt.MapFrom(s => s.TodoItemTags.Select(tt => new TagDto
                {
                    Id = tt.Tag.Id,
                    Name = tt.Tag.Name,
                    Color = tt.Tag.Color,
                    Created = tt.Tag.Created
                }).ToList()));
        }
    }
}
