namespace stats.Tests;
public class CommandHandlerTests
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
    public void Test1()
    {
        Assert.Pass();
    }

    [Test]
    public void SprintServiceCreated()
    {
        var ssf = new Mock<ISprintServiceFactory>();
        var ss = new Mock<ISprintService>();
        ssf.Setup(x => x.createSprintService(It.IsAny<SprintOptions>())).Returns(ss.Object);
        MockReplace(ssf);
        MockReplace(ss);
        var commandHandler = Container.GetService<IStatsCommandHandler>();
        commandHandler.Handler(new SprintOptions(), new stats.FileOptions());
        ssf.Verify(x=>x.createSprintService(It.IsAny<SprintOptions>()), Times.Once);
    }

    [Test]
    public async Task Test2()
    {
        var so = new SprintOptions();
        so.Instance = "https://dev.azure.com/clockshark";
        so.Project = "ClockShark";
        so.Pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        so.Count = 1;
        so.Teams = new string[] { "Cuckoo" };
        var commandHandler = Container.GetService<IStatsCommandHandler>();
        commandHandler.Handler(so, new stats.FileOptions());
    }
}
