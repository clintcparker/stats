namespace stats; 
public class StatsCommandBuilder : ICommandBuilder
{
    private readonly ISprintServiceFactory _sprintServiceFactory;
    public StatsCommandBuilder(ISprintServiceFactory sprintServiceFactory)
    {
        _sprintServiceFactory = sprintServiceFactory;
    }
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
            var sprintService = _sprintServiceFactory.createSprintService(statsOptions);
            Console.WriteLine("Stats!");
            Console.WriteLine(JsonSerializer.Serialize(statsOptions));
        },
        sprintOptionBinder);
        return rootCommand;
    
    
    }
}