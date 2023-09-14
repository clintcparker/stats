namespace stats; 
public class StatsCommandBuilder : ICommandBuilder
{
    private readonly IStatsCommandHandler _statsCommandHandler;
    private readonly SprintOptionsBinder _sprintOptionsBinder;
    public StatsCommandBuilder(IStatsCommandHandler statsCommandHandler, SprintOptionsBinder sprintOptionsBinder)
    {
        _statsCommandHandler = statsCommandHandler;
        _sprintOptionsBinder = sprintOptionsBinder;
    }
    public RootCommand Build()
    {
        var fileOptionBinder = new FileOptionsBinder();

        var rootCommand = new RootCommand();

        foreach (var op in _sprintOptionsBinder.OptionList)
        {
            rootCommand.AddOption(op);
        }
        foreach (var op in fileOptionBinder.OptionList)
        {
            rootCommand.AddOption(op);
        }
        rootCommand.SetHandler<SprintOptions,FileOptions>(_statsCommandHandler.Handler,
        _sprintOptionsBinder,fileOptionBinder);
        return rootCommand;
    
    
    }



}
