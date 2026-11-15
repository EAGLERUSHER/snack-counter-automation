using Microsoft.AspNetCore.Mvc;
using SnackCounter.Api.Models;
using SnackCounter.Api.Services;

namespace SnackCounter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly SnackCounterApplication _app;

    public ChatController(SnackCounterApplication app) => _app = app;

    [HttpGet]
    public ActionResult<IEnumerable<ChatMessageDto>> List()
    {
        return _app.Chat.Messages
            .Select(m => new ChatMessageDto(m.Target, m.Text, m.At))
            .ToList();
    }
}