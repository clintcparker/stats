using ConsoleTables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Text.Json;

public class Program
{
    // async main for console app
    public static async Task<int> Main(string[] args)
    {
        //     HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        //     builder.Services.AddSingleton<StatsConsoleService>();
        //     var provider = builder.Services.BuildServiceProvider();
        //     //using IHost host = builder.Build();

        //     var service = provider.GetService<StatsConsoleService>();
        //     await service.RunAsync();
        //     //await host.RunAsync();
        //tftools velocities -f ~/Desktop/velocities.csv -x  -c 19  -p 3 -l 2  --pat $AZURE_DEVOPS_PAT --domain DOMAIN --project PROJECT --include "TEAM1,TEAM2"


        var optionBinder = new StatsOptionBinder();

        var rootCommand = new RootCommand();

        foreach (var op in optionBinder.OptionList)
        {
            rootCommand.AddOption(op);
        }
        rootCommand.SetHandler(async (statsOptions) =>
        {
            /*await*/
            Console.WriteLine("Stats!");
            Console.WriteLine(JsonSerializer.Serialize(statsOptions));
        },
        optionBinder);
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
    }
}

public class StatsOptions
{
    public int Count { get; set; }
    public string Pat { get; set; }
    public string Instance { get; set; }
    public string Project { get; set; }
    public string File { get; set; }

    public int PlannedDays { get; set; }

    public int LateDays { get; set; }

    public string[] Teams { get; set; }

    public bool Overwrite { get; set; }
}

public class StatsOptionBinder : BinderBase<StatsOptions>
{
    private static string validatePath(string path){
        if (string.IsNullOrEmpty(path))
                    { path = defaultFile;}
                    if (path.StartsWith("~"))
                    {
                        path = Path.Join(Environment.GetEnvironmentVariable("HOME"), path.Replace("~", ""));
                    }
                    if (Path.EndsInDirectorySeparator(path))
                    {
                        path = Path.Join(path, "velocities.csv");
                    }
                    if (Path.GetExtension(path)!=".csv")
                    {
                        path = Path.ChangeExtension(path, ".csv");
                    }
                    return path;
    }
    private const string defaultFile = "~/Desktop/velocities.csv";
    public readonly List<Option> OptionList;
    public readonly Option<int> CountOption = new Option<int>(
            aliases: new[] { "-c", "--count" },
            description: "Number of sprints to include",
            getDefaultValue: () => 3

        )
    { Arity = ArgumentArity.ZeroOrOne };
    public readonly Option<string> PATOption = new Option<string>(
            aliases: new[] { "--pat" },
            description: "Azure DevOps Personal Access Token",
            getDefaultValue: () => Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT")

        )
    { Arity = ArgumentArity.ZeroOrOne,IsRequired = true };
    public readonly Option<string> InstanceOption = new Option<string>(
            aliases: new[] { "--instance", "--domain" },
            description: "Azure DevOps Domain",
            parseArgument: result =>
                {
                    return result.Tokens.Single().Value.Replace("https://dev.azure.com/", "");
                }

        )
    { Arity = ArgumentArity.ExactlyOne, IsRequired = true };
    public readonly Option<string> ProjectOption = new Option<string>(
            aliases: new[] { "--project" },
            description: "Azure DevOps Project"

        )
    { Arity = ArgumentArity.ExactlyOne, IsRequired = true };//TODO support multiple projects
    public readonly Option<string> FileOption = new Option<string>(
            aliases: new[] { "-f", "--file" },
            description: "File to write results to",
            parseArgument: result =>
                {
                    
                    return validatePath(result.Tokens.Single().Value);
                }
        )
    { Arity = ArgumentArity.ZeroOrOne, IsRequired = true };

    public readonly Option<int> PlannedDaysOption = new Option<int>(
            aliases: new[] { "-p", "--planned" },
            description: "Number of days to plan for",
            getDefaultValue: () => 3
        )
    { Arity = ArgumentArity.ZeroOrOne };

    public readonly Option<int> LateDaysOption = new Option<int>(
            aliases: new[] { "-l", "--late" },
            description: "Number of days to consider late",
            getDefaultValue: () => 2
        )
    { Arity = ArgumentArity.ZeroOrOne };

    public readonly Option<string[]> TeamsOption = new Option<string[]>(
            aliases: new[] { "-t", "--teams", "--include" },
            description: "Teams to include"
        );
    
    public StatsOptionBinder()
    {
        FileOption.SetDefaultValue(validatePath(defaultFile));
        OptionList = new List<Option>() { CountOption, PATOption, InstanceOption, ProjectOption, FileOption, PlannedDaysOption, LateDaysOption, TeamsOption };
    }
    protected override StatsOptions GetBoundValue(BindingContext bindingContext) =>
        new StatsOptions
        {
            Count = bindingContext.ParseResult.GetValueForOption(CountOption),
            Pat = bindingContext.ParseResult.GetValueForOption(PATOption),
            Instance = bindingContext.ParseResult.GetValueForOption(InstanceOption),
            Project = bindingContext.ParseResult.GetValueForOption(ProjectOption),
            File = bindingContext.ParseResult.GetValueForOption(FileOption),
            PlannedDays = bindingContext.ParseResult.GetValueForOption(PlannedDaysOption),
            LateDays = bindingContext.ParseResult.GetValueForOption(LateDaysOption),
            Teams = bindingContext.ParseResult.GetValueForOption(TeamsOption)
        };
}