using Golbet.Entities;
using Golbet.Enums;
using Microsoft.EntityFrameworkCore;

namespace Golbet.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Teams.AnyAsync())
        {
            var existingTeams = await context.Teams.ToListAsync();

            foreach (var team in existingTeams)
            {
                team.CrestUrl = team.Name switch 
                {
                    "Atlético Nacional" => "/Images/Teams/nacional.png",
                    "Independiente Medellín" => "/Images/Teams/medellin.png",
                    "Millonarios" => "/Images/Teams/millonarios.png",
                    "Independiente Santa Fe" => "/Images/Teams/santafe.png",
                    "América de Cali" => "/Images/Teams/america.png",
                    "Deportivo Cali" => "/Images/Teams/deportivocali.png",
                    "Junior de Barranquilla" => "/Images/Teams/junior.png",
                    "Once Caldas" => "/Images/Teams/oncecaldas.png",
                    _ => team.CrestUrl
                };
            }

            await context.SaveChangesAsync();
            return;
        }

        var teams = new List<Team>
        {
            new()
            {
                Name = "Atlético Nacional",
                City = "Medellín",
                CrestUrl = "/Images/Teams/nacional.png"
            },
            new()
            {
                Name = "Independiente Medellín",
                City = "Medellín",
                CrestUrl = "/Images/Teams/medellin.png"
            },
            new()
            {
                Name = "Millonarios",
                City = "Bogotá",
                CrestUrl = "/Images/Teams/millonarios.png"
            },
            new()
            {
                Name = "Independiente Santa Fe",
                City = "Bogotá",
                CrestUrl = "/Images/Teams/santafe.png"
            },
            new()
            {
                Name = "América de Cali",
                City = "Cali",
                CrestUrl = "/Images/Teams/america.png"
            },
            new()
            {
                Name = "Deportivo Cali",
                City = "Cali",
                CrestUrl = "/Images/Teams/deportivocali.png"
            },
            new()
            {
                Name = "Junior de Barranquilla",
                City = "Barranquilla",
                CrestUrl = "/Images/Teams/junior.png"
            },
            new()
            {
                Name = "Once Caldas",
                City = "Manizales",
                CrestUrl = "/Images/Teams/oncecaldas.png"
            }
        };

        context.Teams.AddRange(teams);
        await context.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;

        var matches = new List<Match>
        {
            new()
            {
                HomeTeamId = teams[0].Id,
                AwayTeamId = teams[1].Id,
                Date = today.AddDays(3).AddHours(20),
                Status = MatchStatus.Scheduled,
                HomeOdds = 2.10m,
                DrawOdds = 3.20m,
                AwayOdds = 3.60m
            },
            new()
            {
                HomeTeamId = teams[2].Id,
                AwayTeamId = teams[3].Id,
                Date = today.AddDays(5).AddHours(18),
                Status = MatchStatus.Scheduled,
                HomeOdds = 2.45m,
                DrawOdds = 3.00m,
                AwayOdds = 2.95m
            },
            new()
            {
                HomeTeamId = teams[4].Id,
                AwayTeamId = teams[5].Id,
                Date = today.AddDays(7).AddHours(19),
                Status = MatchStatus.Scheduled,
                HomeOdds = 2.30m,
                DrawOdds = 3.10m,
                AwayOdds = 3.15m
            },
            new()
            {
                HomeTeamId = teams[6].Id,
                AwayTeamId = teams[7].Id,
                Date = today.AddDays(10).AddHours(16),
                Status = MatchStatus.Scheduled,
                HomeOdds = 1.85m,
                DrawOdds = 3.40m,
                AwayOdds = 4.20m
            },
            new()
            {
                HomeTeamId = teams[3].Id,
                AwayTeamId = teams[6].Id,
                Date = DateTime.UtcNow.AddHours(-1),
                Status = MatchStatus.InProgress,
                HomeOdds = 2.60m,
                DrawOdds = 3.05m,
                AwayOdds = 2.80m
            },
            new()
            {
                HomeTeamId = teams[1].Id,
                AwayTeamId = teams[2].Id,
                Date = today.AddDays(-4).AddHours(20),
                Status = MatchStatus.Finished,
                HomeGoals = 2,
                AwayGoals = 1,
                HomeOdds = 2.75m,
                DrawOdds = 3.10m,
                AwayOdds = 2.70m
            }
        };

        context.Matches.AddRange(matches);
        await context.SaveChangesAsync();
    }
}