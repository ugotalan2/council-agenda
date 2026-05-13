using CouncilAgendaApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace CouncilAgendaApi.Controllers;

public class BaseController : ControllerBase
{
    protected readonly AppDbContext _db;

    public BaseController(AppDbContext db)
    {
        _db = db;
    }

    protected string GetClerkUserId() =>
        User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? string.Empty;
}