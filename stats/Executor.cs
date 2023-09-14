
namespace stats;

public class Executor
{
    private readonly ICommandBuilder _commandBuilder;

    public Executor(ICommandBuilder commandBuilder)
    {
        _commandBuilder = commandBuilder;
    }

    public int Execute(string[] args)
    {
        return ExecuteAsync(args).Result;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        var rootCommand = _commandBuilder.Build();
        return await rootCommand.InvokeAsync(args);
    }
}