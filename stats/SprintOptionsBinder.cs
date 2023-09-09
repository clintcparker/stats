namespace stats;

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
                    if (string.IsNullOrWhiteSpace(pat))
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