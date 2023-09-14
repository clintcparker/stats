namespace stats;

public interface ISprintServiceFactory {
    public ISprintService createSprintService(SprintOptions options);
}