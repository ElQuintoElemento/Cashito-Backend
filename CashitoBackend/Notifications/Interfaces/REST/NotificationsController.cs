using CashitoBackend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using CashitoBackend.Notifications.Domain.Model.Queries;
using CashitoBackend.Notifications.Domain.Services;
using CashitoBackend.Notifications.Interfaces.REST.Resources;
using CashitoBackend.Notifications.Interfaces.REST.Transform;
using CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CashitoBackend.Notifications.Interfaces.REST;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationQueryService _queryService;
    private readonly INotificationCommandService _commandService;

    public NotificationsController(
        INotificationQueryService queryService,
        INotificationCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();
        var notifications = await _queryService.Handle(new GetNotificationsByUserIdQuery(userId));
        var response = notifications.Select(NotificationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(response);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.GetUserId();
        var count = await _queryService.Handle(new GetUnreadCountQuery(userId));
        return Ok(new UnreadCountResource(count));
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = User.GetUserId();
        var result = await _commandService.MarkAsRead(id, userId);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.GetUserId();
        await _commandService.MarkAllAsRead(userId);
        return NoContent();
    }
}
