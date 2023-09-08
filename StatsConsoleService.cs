using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ConsoleTables;


public sealed class StatsConsoleService
{
    private readonly ILogger _logger;

    public StatsConsoleService(
        ILogger<StatsConsoleService> logger,
        IHostApplicationLifetime appLifetime)
    {
        _logger = logger;

        appLifetime.ApplicationStarted.Register(OnStarted);
        appLifetime.ApplicationStopping.Register(OnStopping);
        appLifetime.ApplicationStopped.Register(OnStopped);
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("RunAsync has been called.");
        var sprintService = new SprintService();
        
        var result = await sprintService.getSprints(DateTime.Parse("8/1/2023"), DateTime.Now.AddDays(13));
        var table = new ConsoleTable("Team", "Sprint", "Start Date", "End Date", "planned", "completed", "late", "incomplete","total");
        foreach (TeamIteration sprint in result)
        {
            table.AddRow(sprint.TeamName, sprint.IterationName, sprint.StartDate.ToShortDateString(), sprint.EndDate.ToShortDateString(), sprint.PlannedPoints, sprint.CompletedPoints - sprint.LatePoints, sprint.LatePoints, sprint.IncompletePoints, sprint.CompletedPoints);
        }
        table.Write();

        return;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("1. StartAsync has been called.");
        return;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("4. StopAsync has been called.");

        return Task.CompletedTask;
    }

    private async void OnStarted()
    {
        _logger.LogInformation("2. OnStarted has been called.");


        var sprintService = new SprintService();
        
        var result = await sprintService.getSprints(DateTime.Parse("8/1/2023"), DateTime.Now.AddDays(13));
        var table = new ConsoleTable("Team", "Sprint", "Start Date", "End Date", "planned", "completed", "late", "incomplete","total");
        foreach (TeamIteration sprint in result)
        {
            table.AddRow(sprint.TeamName, sprint.IterationName, sprint.StartDate.ToShortDateString(), sprint.EndDate.ToShortDateString(), sprint.PlannedPoints, sprint.CompletedPoints - sprint.LatePoints, sprint.LatePoints, sprint.IncompletePoints, sprint.CompletedPoints);
        }

        await Task.Run(async ()=>{table.Write();});
        await this.StopAsync(new CancellationToken());
        return; 
    }

    private void OnStopping()
    {
        _logger.LogInformation("3. OnStopping has been called.");
    }

    private void OnStopped()
    {
        _logger.LogInformation("5. OnStopped has been called.");
    }
}