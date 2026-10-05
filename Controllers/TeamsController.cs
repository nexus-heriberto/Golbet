using Golbet.DTOs;
using Golbet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Golbet.Controllers;

public class TeamsController : Controller
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    // GET: /Teams
    public async Task<IActionResult> Index()
    {
        var teams = await _teamService.GetAllAsync();

        return View(teams);
    }

    // GET: /Teams/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new TeamFormDto());
    }

    // POST: /Teams/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeamFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        await _teamService.CreateAsync(dto);

        TempData["Success"] = "Equipo creado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Teams/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _teamService.GetForEditAsync(id);

        if (dto is null)
        {
            return NotFound();
        }

        return View(dto);
    }

    // POST: /Teams/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TeamFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _teamService.UpdateAsync(dto);

            TempData["Success"] = "Equipo actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // POST: /Teams/Deactivate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _teamService.DeactivateAsync(id);

        TempData["Success"] = "Equipo desactivado correctamente.";

        return RedirectToAction(nameof(Index));
    }
}


