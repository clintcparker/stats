namespace stats;

public class FileOptionsBinder : BinderBase<FileOptions>
{
    private static string validatePath(string path){
        if (string.IsNullOrEmpty(path))
        { path = defaultFile; }
        if (path.StartsWith("~"))
        {
            path = Path.Join(Container.GetService<ISystemHelpers>().GetEnvironmentVariable("HOME"), path.Replace("~", ""));
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
            if (Container.GetService<ISystemHelpers>().Exists(fileValue) && !result.GetValueForOption(OverwriteOption)){
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