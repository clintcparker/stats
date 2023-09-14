global using ConsoleTables;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using System.CommandLine;
global using System.CommandLine.Binding;
global using System.Text.Json;
global using System.CommandLine.Help;
global using System.CommandLine.Builder;
global using System.CommandLine.Parsing;
global using stats;

namespace stats;

public class Program
{
    // async main for console app
    public static async Task<int> Main(string[] args)
    {
        //var executor = DIContainer.BuildEntryPoint<Executor>();
        var executor = Container.GetService<Executor>();
        return await executor.ExecuteAsync(args);
    }
}