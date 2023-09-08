using ConsoleTables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Text.Json;
using System.CommandLine.Help;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

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



        var rootCommand = new StatsCommandBuilder().Build();
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

public class StatsCommandBuilder
{
    public RootCommand Build()
    {
        var sprintOptionBinder = new SprintOptionsBinder();
        var fileOptionBinder = new FileOptionsBinder();

        var rootCommand = new RootCommand();

        foreach (var op in sprintOptionBinder.OptionList)
        {
            rootCommand.AddOption(op);
        }
        foreach (var op in fileOptionBinder.OptionList)
        {
            rootCommand.AddOption(op);
        }
        rootCommand.SetHandler(async (statsOptions) =>
        {
            /*await*/
            Console.WriteLine("Stats!");
            Console.WriteLine(JsonSerializer.Serialize(statsOptions));
        },
        sprintOptionBinder);
        return rootCommand;
    }
}

public class FileOptions
{
    public string File { get; set; }
    public bool Overwrite { get; set; }
}

public class FileOptionsBinder : BinderBase<FileOptions>
{
    private static string validatePath(string path){
        if (string.IsNullOrEmpty(path))
        { path = defaultFile; }
        if (path.StartsWith("~"))
        {
            path = Path.Join(Environment.GetEnvironmentVariable("HOME"), path.Replace("~", ""));
        }
        if (Path.EndsInDirectorySeparator(path))
        {
            path = Path.Join(path, "velocities.csv");
        }
        if (Path.GetExtension(path) != ".csv")
        {
            path = Path.ChangeExtension(path, ".csv");
        }
        return path;
    }
    private const string defaultFile = "~/Desktop/velocities.csv";
    public readonly List<Option> OptionList;
    
    
    public readonly Option<string> FileOption = new Option<string>(
            aliases: new[] { "-f", "--file" },
            description: "File to write results to",
            parseArgument: result =>
                {
                    return validatePath(result.Tokens.Single().Value);
                }
        )
    { Arity = ArgumentArity.ZeroOrOne, IsRequired = true };
    
    public readonly Option<bool> OverwriteOption = new Option<bool>(
            aliases: new[] { "-x", "--overwrite" },
            description: "Overwrite file if it exists",
            getDefaultValue: () => false
        )
    { Arity = ArgumentArity.ZeroOrOne };

    

    public FileOptionsBinder()
    {
        OverwriteOption.AddValidator(result => {
            var fileValue = validatePath(result.GetValueForOption(FileOption));
            if (File.Exists(fileValue) && !result.GetValueForOption(OverwriteOption)){
                result.ErrorMessage = $" {fileValue} already exists! Specify --overwrite to overwrite existing file";
            }
        });
        FileOption.SetDefaultValue(validatePath(defaultFile));
        OptionList = new List<Option>() { FileOption, OverwriteOption };
    }
    protected override FileOptions GetBoundValue(BindingContext bindingContext) =>
        new FileOptions
        {
            File = bindingContext.ParseResult.GetValueForOption(FileOption)
        };

}

public class SprintOptions
{
    public int Count { get; set; }
    public string Pat { get; set; }
    public string Instance { get; set; }
    public string Project { get; set; }

    public int PlannedDays { get; set; }

    public int LateDays { get; set; }

    public string[] Teams { get; set; }

}

public class SprintOptionsBinder : BinderBase<SprintOptions>
{
    private const string patEnvVar = "AZURE_DEVOPS_PAT";
    public readonly List<Option> OptionList;
    public readonly Option<int> CountOption = new Option<int>(
            aliases: new[] { "-c", "--count" },
            description: "Number of sprints to include",
            getDefaultValue: () => 3

        )
    { Arity = ArgumentArity.ExactlyOne};
    public readonly Option<string> PATOption = new Option<string>(
            aliases: new[] { "--pat" },
            description: "Azure DevOps Personal Access Token [default: $AZURE_DEVOPS_PAT]",
            parseArgument: result =>
                {
                    if (result.Tokens.Count == 0)
                    {
                        return Environment.GetEnvironmentVariable(patEnvVar);
                    }
                    if (!result.Tokens.Any())
                    {
                        return Environment.GetEnvironmentVariable(patEnvVar);
                    }
                    var pat = result.Tokens.Single().Value;
                    if (string.IsNullOrEmpty(pat))
                    {
                        pat = Environment.GetEnvironmentVariable(patEnvVar);
                    }
                    return pat;
                }

        )
    { Arity = ArgumentArity.ExactlyOne };

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


    public readonly Option<int> PlannedDaysOption = new Option<int>(
            aliases: new[] { "-p", "--planned" },
            description: "Extra days to count from the start of the sprint for planned work",
            getDefaultValue: () => 3
        )
    { Arity = ArgumentArity.ZeroOrOne };

    public readonly Option<int> LateDaysOption = new Option<int>(
            aliases: new[] { "-l", "--late" },
            description: "Extra days to count from the end of the sprint for late work",
            getDefaultValue: () => 2
        )
    { Arity = ArgumentArity.ZeroOrOne };

    public readonly Option<string[]> TeamsOption = new Option<string[]>(
            aliases: new[] { "-t", "--teams", "--include" },
            description: "Teams to include"
        );


    
    public SprintOptionsBinder()
    {
        OptionList = new List<Option>() { PATOption, CountOption , InstanceOption, ProjectOption, PlannedDaysOption, LateDaysOption, TeamsOption };
    }
    protected override SprintOptions GetBoundValue(BindingContext bindingContext) =>
        new SprintOptions
        {
            Count = bindingContext.ParseResult.GetValueForOption(CountOption),
            Pat = bindingContext.ParseResult.GetValueForOption(PATOption) ?? Environment.GetEnvironmentVariable(patEnvVar),
            Instance = bindingContext.ParseResult.GetValueForOption(InstanceOption),
            Project = bindingContext.ParseResult.GetValueForOption(ProjectOption),
            PlannedDays = bindingContext.ParseResult.GetValueForOption(PlannedDaysOption),
            LateDays = bindingContext.ParseResult.GetValueForOption(LateDaysOption),
            Teams = bindingContext.ParseResult.GetValueForOption(TeamsOption)
        };
}