using Golbet.DTOs;
using Golbet.Enums;
using Golbet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Golbet.Controllers;

public class MatchesController : Controller
{
    private readonly IMatchService _matchService;
    private readonly ITeamService _teamService;

    public MatchesController(
        IMatchService matchService,
        ITeamService teamService)
    {
        _matchService = matchService;
        _teamService = teamService;
    }

    // GET: /Matches
    // La cartelera permite buscar equipos y filtrar por estado.
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        MatchStatus? status,
        string? search)
    {
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search;

        var board = await _matchService.GetBoardAsync(status, search);

        return View(board);
    }

    // GET: /Matches/Detail/5
    [AllowAnonymous]
    public async Task<IActionResult> Detail(int id)
    {
        var match = await _matchService.GetDetailAsync(id);

        if (match is null)
            return NotFound();

        return View(match);
    }

    // GET: /Matches/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await LoadTeamsAsync();

        return View(new MatchFormDto());
    }

    // POST: /Matches/Create
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MatchFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadTeamsAsync();
            return View(dto);
        }

        try
        {
            await _matchService.CreateAsync(dto);

            TempData["Success"] = "Partido creado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await LoadTeamsAsync();

            return View(dto);
        }
    }

    // GET: /Matches/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _matchService.GetForEditAsync(id);

        if (dto is null)
            return NotFound();

        await LoadTeamsAsync();

        return View(dto);
    }

    // POST: /Matches/Edit
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MatchFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadTeamsAsync();
            return View(dto);
        }

        try
        {
            await _matchService.UpdateAsync(dto);

            TempData["Success"] = "Partido actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await LoadTeamsAsync();

            return View(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // POST: /Matches/Deactivate
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _matchService.DeactivateAsync(id);

        TempData["Success"] = "Partido desactivado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadTeamsAsync()
    {
        var teams = await _teamService.GetAllAsync();

        ViewBag.Teams = teams
            .Where(team => team.IsActive)
            .OrderBy(team => team.Name)
            .ToList();
    }
}



