using System.CommandLine.Parsing;

public class SprintOptionsBinder : BinderBase<SprintOptions>
{
    private const string patEnvVar = "AZURE_DEVOPS_PAT";
    public List<Option> OptionList { get; }
    public readonly Option<int> CountOption = new Option<int>(
            aliases: new[] { "-c", "--count" },
            description: "Number of sprints to include",
            getDefaultValue: () => 3

        )
    { Arity = ArgumentArity.ExactlyOne};
    public readonly Option<string> PATOption = new Option<string>(
            aliases: new[] { "--pat" },
            description: "Azure DevOps Personal Access Token [default: $AZURE_DEVOPS_PAT]"

        )
    { Arity = ArgumentArity.ZeroOrOne};

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

    private void StringValueRequired(OptionResult result)
    {
        var stringValue = result.GetValueOrDefault<string>();
        if (String.IsNullOrWhiteSpace(stringValue) && result.Token.Value != null){
            result.ErrorMessage = $"Required argument missing for option: '{result.Token.Value}'.";
        }
    }
    
    public SprintOptionsBinder()
    {
        PATOption.AddValidator(StringValueRequired);
        InstanceOption.AddValidator(StringValueRequired);
        ProjectOption.AddValidator(StringValueRequired);
        OptionList = new List<Option>() { PATOption, CountOption , InstanceOption, ProjectOption, PlannedDaysOption, LateDaysOption, TeamsOption };
    }

    protected override SprintOptions GetBoundValue(BindingContext bindingContext) =>
        new SprintOptions
        {
            Count = bindingContext.ParseResult.GetValueForOption(CountOption),
            Pat = bindingContext.ParseResult.GetValueForOption(PATOption) ?? Container.GetService<ISystemHelpers>().GetEnvironmentVariable(patEnvVar),
            Instance = bindingContext.ParseResult.GetValueForOption(InstanceOption),
            Project = bindingContext.ParseResult.GetValueForOption(ProjectOption),
            PlannedDays = bindingContext.ParseResult.GetValueForOption(PlannedDaysOption),
            LateDays = bindingContext.ParseResult.GetValueForOption(LateDaysOption),
            Teams = bindingContext.ParseResult.GetValueForOption(TeamsOption)
        };
}