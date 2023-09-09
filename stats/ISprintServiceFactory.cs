namespace stats;

public interface ISprintServiceFactory {
    public SprintService createSprintService(SprintOptions options);
}