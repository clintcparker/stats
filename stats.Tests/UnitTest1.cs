using stats;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Text.Json;
using System.CommandLine.Help;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Moq;

namespace stats.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }

    
    [Test]
    public void Test2()
    {
        //Assert.Pass();
        var command = new stats.StatsCommandBuilder(null).Build();

        var result = command.Parse();
        Assert.AreEqual(3, result.Errors.Count);
    }

    [Test]
    public void StripAzureDomain()
    {
        var command = new stats.StatsCommandBuilder(null).Build();

        var result = command.Parse("--instance https://dev.azure.com/MyDomain");
        Assert.AreEqual("MyDomain", result.GetValueForOption(command.Options.First(x=>x.Name == "instance")));
    }

    [Test]
    public void EmptyPATGetsEnvVar()
    {
        var command = new stats.StatsCommandBuilder(null).Build();
        var envVar = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        var result = command.Parse("--pat \" \"");
        Assert.AreEqual(envVar, result.GetValueForOption(command.Options.First(x=>x.Name == "pat")));
    }

    [Test]
    public void MissingPATGetsEnvVar()
    {
        var command = new stats.StatsCommandBuilder(null).Build();
        var envVar = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        var result = command.Parse("");
        Assert.AreEqual(envVar, result.GetValueForOption(command.Options.First(x=>x.Name == "pat")));
    }

    [Test]
    public void BlankPATGetsError()
    {
        var command = new stats.StatsCommandBuilder(null).Build();
        var envVar = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        var result = command.Parse("--pat");
        Assert.AreEqual("Required argument missing for option: '--pat'.", result.Errors[2].Message);
    }


    [Test]
    public async Task ExitCodeIsZero()
    {
        var result = await stats.Program.Main(new string[] { "--help" });
        Assert.AreEqual(0, result);
    }

    [Test]
    public async Task OptionsGetBuiltRight()
    {

        var callArgs = new List<SprintOptions>();
        var ssf = new Mock<ISprintServiceFactory>(){ CallBase = true };
        var ss = new Mock<SprintService>(){ CallBase = true };
        ssf.Setup(x=>x.createSprintService(It.IsAny<SprintOptions>())).Callback<SprintOptions>((options)=>callArgs.Add(options)).Returns(ss.Object);
        var statsBuilder = new stats.StatsCommandBuilder(ssf.Object);
        var command = statsBuilder.Build();
        var result =  command.Invoke("--instance https://dev.azure.com/mydomainx --project myproj -x");
        Assert.AreEqual(0, result);
        Assert.AreEqual(1, callArgs.Count);
        Assert.AreEqual("mydomainx", callArgs[0].Instance);
    }

    [Test]
    public async Task HandlerIsntInvokedWithBadCommand()
    {
        var callArgs = new List<SprintOptions>();
        var ssf = new Mock<ISprintServiceFactory>(){ CallBase = true };
        var ss = new Mock<SprintService>(){ CallBase = true };
        ssf.Verify(x=>x.createSprintService(It.IsAny<SprintOptions>()), Times.Never);
        ssf.Setup(x=>x.createSprintService(It.IsAny<SprintOptions>())).Callback<SprintOptions>((options)=>callArgs.Add(options)).Returns(ss.Object);
        var statsBuilder = new stats.StatsCommandBuilder(ssf.Object);
        var command = statsBuilder.Build();
        var result =  command.Invoke("--instance https://dev.azure.com/mydomain --project myproj ");
        Assert.AreEqual(0, callArgs.Count);
    }
}