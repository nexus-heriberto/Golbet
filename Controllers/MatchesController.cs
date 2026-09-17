using Golbet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Golbet.Controllers;

public class MatchesController : Controller
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    public async Task<IActionResult> Index()
    {
        var board = await _matchService.GetBoardAsync();

        return View(board);
    }
}