namespace stats;
public class StatsCommandHandler : IStatsCommandHandler
{
    private readonly ISprintServiceFactory _sprintServiceFactory;
    public StatsCommandHandler(ISprintServiceFactory sprintServiceFactory)
    {
        _sprintServiceFactory = sprintServiceFactory;
    }
    public async void Handler(SprintOptions statsOptions, FileOptions fileOptions)
    {
            /*await*/
            var sprintService = _sprintServiceFactory.createSprintService(statsOptions);
                // var sprintService = new SprintService();
        var result = await sprintService.getSprints(statsOptions.Count);
        // var table = new ConsoleTable("Team", "Sprint", "Start Date", "End Date", "planned", "completed", "late", "incomplete","total");
        // foreach (TeamIteration sprint in result)
        // {
        //     table.AddRow(sprint.TeamName, sprint.IterationName, sprint.StartDate.ToShortDateString(), sprint.EndDate.ToShortDateString(), sprint.PlannedPoints, sprint.CompletedPoints - sprint.LatePoints, sprint.LatePoints, sprint.IncompletePoints, sprint.CompletedPoints);
        // }
        // table.Write();
    }
}

public interface IStatsCommandHandler
{
    void Handler(SprintOptions statsOptions, FileOptions fileOptions);
}