
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;


namespace stats.Tests;

public class CommandBuilderTests
{

    private void MockReplace<T> (Mock<T> mock) where T : class
    {
        Container.Replace(mock.Object);
    }
    
    [SetUp]
    public void Setup()
    {
        Container.Flush();
    }
    
    [Test]
    public void Test2()
    {
        //var sp = services.BuildServiceProvider();
        //Assert.Pass();
        var command = Container.GetService<ICommandBuilder>().Build();

        var result = command.Parse();
        Assert.That(result.Errors.Count, Is.EqualTo(2));
    }

    
    [Test]
    public void ContainerTest1()
    {
        var mockCommand = new Mock<ICommandBuilder>();

        mockCommand.Setup(x=>x.Build()).Returns(new RootCommand());
        
        MockReplace(mockCommand);
        var command = Container.GetService<ICommandBuilder>().Build();

        var result = command.Parse();
        Assert.That(result.Errors.Count, Is.EqualTo(0));
    }

    [Test]
    public void StripAzureDomain()
    {
        //Assert.Pass();
        var command = Container.GetService<ICommandBuilder>().Build();

        var result = command.Parse("--instance https://dev.azure.com/MyDomain");
        Assert.That(result.GetValueForOption(command.Options.First(x=>x.Name == "instance")), Is.EqualTo("MyDomain"));
    }


    [Test]
    public async Task OptionsGetBuiltRight()
    {

        var callArgs = new List<SprintOptions>();
        var sch = new Mock<IStatsCommandHandler>(){ CallBase = true };
        sch.Setup(
            x=>x.Handler(It.IsAny<SprintOptions>(), It.IsAny<FileOptions>()))
            .Callback<SprintOptions,FileOptions>((s,f)=>callArgs.Add(s));
        
        MockReplace(sch);
        var statsBuilder = Container.GetService<ICommandBuilder>();
        var command = statsBuilder.Build();
        var result =  await command.InvokeAsync("--instance https://dev.azure.com/myDomainX --project myProject -x --pat myPAT");
        Assert.That(result, Is.EqualTo(0));
        Assert.That(callArgs.Count, Is.EqualTo(1));
        Assert.That(callArgs[0].Instance, Is.EqualTo("myDomainX"));
        sch.Verify(x=>x.Handler(It.Is<SprintOptions>(x=>x.Instance == "myDomainX"), It.IsAny<FileOptions>()), Times.Once);
    }

    [Test]
    public async Task CantOverWriteWithoutArg()
    {
        
        //Setup
        var invokeArgs = "--instance https://dev.azure.com/myDomainX --project myProject --pat myPAT";
        var callArgs = new List<FileOptions>();
        var mockSystemHelpers = new Mock<ISystemHelpers>();
        var handler = new Mock<IStatsCommandHandler>();
        MockReplace(mockSystemHelpers);
        MockReplace(handler);
        var homeVar = "/home/myUser";
        mockSystemHelpers.Setup(x=>x.GetEnvironmentVariable("HOME")).Returns(homeVar);
        handler.Setup(
            x=>x.Handler(It.IsAny<SprintOptions>(), It.IsAny<FileOptions>()))
            .Callback<SprintOptions,FileOptions>((s,f)=>callArgs.Add(f));
        
        //Mock file ALWAYS exists
        mockSystemHelpers.Setup(x=>x.Exists(It.IsAny<string>())).Returns(true);

        var statsBuilder = Container.GetService<ICommandBuilder>();
        var command = statsBuilder.Build();

        //Test without overwrite
        var parseResult1 =  command.Parse(invokeArgs);
        var invokeResult1 =  await command.InvokeAsync(invokeArgs);
        Assert.That(parseResult1.Errors.Any(x=>x.Message.Contains("already exists")));
        handler.Verify(x=>x.Handler(It.IsAny<SprintOptions>(), It.IsAny<FileOptions>()), Times.Never);

        //Test with overwrite
        var parseResult2 =  command.Parse(invokeArgs+" -x");
        var invokeResult2 =  await command.InvokeAsync(invokeArgs+" -x");
        Assert.IsFalse(parseResult2.Errors.Any(x=>x.Message.Contains("already exists")));
        Assert.That(callArgs[0].File, Is.EqualTo("/home/myUser/Desktop/velocities.csv"));
        handler.Verify(x=>x.Handler(It.IsAny<SprintOptions>(), It.Is<FileOptions>(x=>x.File == "/home/myUser/Desktop/velocities.csv")), Times.Once);
    }

    [Test]
    public void EmptyPATErrors()
    {
        var mockSystemHelpers = new Mock<ISystemHelpers>();
        mockSystemHelpers.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        Container.Replace<ISystemHelpers>(mockSystemHelpers.Object);
        var invokeArgs = "--pat";
        var statsBuilder = Container.GetService<ICommandBuilder>();
        var parseResult = statsBuilder.Build().Parse(invokeArgs);
        Assert.That(parseResult.Errors.Any(x => x.Message.Contains("Required argument missing for option: '--pat'")));
    }

    [Test]
    public void EmptyInstanceErrors()
    {
        var mockSystemHelpers = new Mock<ISystemHelpers>();
        mockSystemHelpers.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        Container.Replace<ISystemHelpers>(mockSystemHelpers.Object);
        var invokeArgs = "--instance";
        var statsBuilder = Container.GetService<ICommandBuilder>();
        var parseResult = statsBuilder.Build().Parse(invokeArgs);
        Assert.That(parseResult.Errors.Any(x => x.Message.Contains("Required argument missing for option: '--instance'")));
    }

    [Test]
    public void EmptyProjectErrors()
    {
        var mockSystemHelpers = new Mock<ISystemHelpers>();
        mockSystemHelpers.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        Container.Replace<ISystemHelpers>(mockSystemHelpers.Object);
        var invokeArgs = "--project";
        var statsBuilder = Container.GetService<ICommandBuilder>();
        var parseResult = statsBuilder.Build().Parse(invokeArgs);
        Assert.That(parseResult.Errors.Any(x => x.Message.Contains("Required argument missing for option: '--project'")));
    }


    [Test]
    public void OptionsGetBuiltRight2()
    {

        var envVar = "NotThePAT";
        var mockSystemHelpers = new Mock<ISystemHelpers>();
        mockSystemHelpers.Setup(x => x.GetEnvironmentVariable("AZURE_DEVOPS_PAT")).Returns(envVar);
        Container.Replace<ISystemHelpers>(mockSystemHelpers.Object);
        var callArgs = new List<SprintOptions>();
        var sch = new Mock<IStatsCommandHandler>() { CallBase = true };
        sch.Setup(
            x => x.Handler(It.IsAny<SprintOptions>(), It.IsAny<FileOptions>()))
            .Callback<SprintOptions, FileOptions>((s, f) => callArgs.Add(s));

        MockReplace(sch);
        var statsBuilder = Container.GetService<ICommandBuilder>();
        var command = statsBuilder.Build();
        var result = command.Invoke("--instance https://dev.azure.com/myDomainX --project myProject -x ");
        Assert.That(result, Is.EqualTo(0));
        Assert.That(callArgs.Count, Is.EqualTo(1));
        Assert.That(callArgs[0].Instance, Is.EqualTo("myDomainX"));
        sch.Verify(x => x.Handler(It.Is<SprintOptions>(x => x.Instance == "myDomainX"), It.IsAny<FileOptions>()), Times.Once);
        sch.Verify(x => x.Handler(It.Is<SprintOptions>(x => x.Pat == envVar), It.IsAny<FileOptions>()), Times.Once);
    }

    [Test]
    public void HandlerIsNotInvokedWithBadCommand()
    {
        var sch = new Mock<IStatsCommandHandler>() { CallBase = true };
        MockReplace(sch);
        var statsBuilder = Container.GetService<ICommandBuilder>();
        var command = statsBuilder.Build();
        var result = command.Invoke("--instance https://dev.azure.com/mydomain --project myProject --pat");
        sch.Verify(x => x.Handler(It.IsAny<SprintOptions>(), It.IsAny<FileOptions>()), Times.Never);
    }
}