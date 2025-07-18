using Microsoft.AspNetCore.Mvc;
using Todo_App.Application.Common.Models;
using Todo_App.Application.TodoItems.Commands.AddTagToTodoItem;
using Todo_App.Application.TodoItems.Commands.CreateTodoItem;
using Todo_App.Application.TodoItems.Commands.DeleteTodoItem;
using Todo_App.Application.TodoItems.Commands.RemoveTagFromTodoItem;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItem;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItemDetail;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItemTags;
using Todo_App.Application.TodoItems.Queries.GetTodoItemsWithPagination;

namespace Todo_App.WebUI.Controllers;

public class TodoItemsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<TodoItemDto>>> GetTodoItemsWithPagination([FromQuery] GetTodoItemsWithPaginationQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTodoItemCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTodoItemCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpPut("[action]")]
    public async Task<ActionResult> UpdateItemDetails(int id, UpdateTodoItemDetailCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteTodoItemCommand(id));

        return NoContent();
    }

    // Feature 2: Tag Management for TodoItems
    [HttpPost("{id}/tags/{tagId}")]
    public async Task<IActionResult> AddTag(int id, int tagId)
    {
        await Mediator.Send(new AddTagToTodoItemCommand { TodoItemId = id, TagId = tagId });
        return NoContent();
    }

    [HttpDelete("{id}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTag(int id, int tagId)
    {
        await Mediator.Send(new RemoveTagFromTodoItemCommand { TodoItemId = id, TagId = tagId });
        return NoContent();
    }

    [HttpPut("{id}/tags")]
    public async Task<IActionResult> UpdateTags(int id, [FromBody] List<int> tagIds)
    {
        await Mediator.Send(new UpdateTodoItemTagsCommand { TodoItemId = id, TagIds = tagIds });
        return NoContent();
    }
}
