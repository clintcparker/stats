
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using stats;
global using Moq;
global using System.Text.Json;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;


namespace stats.Tests;

public class ProgramTests
{

    [SetUp]
    public void Setup()
    {
        Container.Flush();
    }
    
    [Test]
    public async Task ExitCodeIsZero()
    {
        var result = await stats.Program.Main(new string[] { "--help" });
        Assert.AreEqual(0, result);
    }
}