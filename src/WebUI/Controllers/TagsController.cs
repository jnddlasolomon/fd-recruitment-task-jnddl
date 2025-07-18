using Microsoft.AspNetCore.Mvc;
using Todo_App.Application.Common.Models;
using Todo_App.Application.Tags.Commands.CreateTag;
using Todo_App.Application.Tags.Commands.DeleteTag;
using Todo_App.Application.Tags.Queries.GetTags;

namespace Todo_App.WebUI.Controllers;

public class TagsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TagDto>>> GetTags()
    {
        return await Mediator.Send(new GetTagsQuery());
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTagCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteTagCommand(id));
        return NoContent();
    }
}