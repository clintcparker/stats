
global using System.Net.Http;
namespace stats;

public class SprintServiceFactory : ISprintServiceFactory {

    private readonly HttpClient _httpClient;
    public SprintServiceFactory(HttpClient httpClient)
    {
        _httpClient= httpClient;
    }
    public ISprintService createSprintService(SprintOptions options)
    {
        return new SprintService(options, _httpClient);
    }
}