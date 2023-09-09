
namespace stats;

public class Executor
{
    private readonly ICommandBuilder _commandBuilder;

    public Executor(
        ICommandBuilder commandBuilder
        )
    {
        _commandBuilder = commandBuilder;
    }

    public int Execute(string[] args)
    {
        throw new NotImplementedException();
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        var rootCommand = _commandBuilder.Build();
        //new StatsOptionBinder(countOption, patOption, instanceOption, projectOption, fileOption));

        // var sprintService = new SprintService();
        // var result = await sprintService.getSprints(DateTime.Parse("8/1/2023"), DateTime.Now.AddDays(13));
        // var table = new ConsoleTable("Team", "Sprint", "Start Date", "End Date", "planned", "completed", "late", "incomplete","total");
        // foreach (TeamIteration sprint in result)
        // {
        //     table.AddRow(sprint.TeamName, sprint.IterationName, sprint.StartDate.ToShortDateString(), sprint.EndDate.ToShortDateString(), sprint.PlannedPoints, sprint.CompletedPoints - sprint.LatePoints, sprint.LatePoints, sprint.IncompletePoints, sprint.CompletedPoints);
        // }
        // table.Write();
        return await rootCommand.InvokeAsync(args);
        // _test.DoSomething();
        // return 0;
    }
}