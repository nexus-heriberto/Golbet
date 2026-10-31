using Golbet.Data;
using Golbet.DTOs;
using Golbet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Golbet.Controllers;

[Authorize(Roles = DbSeeder.ApostadorRole)]
public class BetsController : Controller
{
    private readonly IBetService _betService;
    private readonly UserManager<Entities.AppUser> _userManager;

    public BetsController(
        IBetService betService,
        UserManager<Entities.AppUser> userManager)
    {
        _betService = betService;
        _userManager = userManager;
    }

    // POST: /Bets/Place
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Place(BetFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] =
                "Los datos de la apuesta no son válidos.";

            return RedirectToAction(
                "Detail",
                "Matches",
                new { id = dto.MatchId });
        }

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        try
        {
            await _betService.PlaceBetAsync(userId, dto);

            TempData["Success"] =
                "¡Tu apuesta fue registrada correctamente!";

            return RedirectToAction(
                "Detail",
                "Matches",
                new { id = dto.MatchId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;

            return RedirectToAction(
                "Detail",
                "Matches",
                new { id = dto.MatchId });
        }
    }

    // GET: /Bets/MyBets
    [HttpGet]
    public async Task<IActionResult> MyBets()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var bets = await _betService.GetMyBetsAsync(userId);

        return View(bets);
    }
}
