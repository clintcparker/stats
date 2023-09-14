
public class SprintServiceTests
{
    [SetUp]
    public void Setup()
    {
        Container.Flush();
    }

    [Test]
    public void Test1(){
        var options = new SprintOptions();
        var ss= Container.GetService<ISprintServiceFactory>().createSprintService(options);
    }
}