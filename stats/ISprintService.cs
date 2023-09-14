namespace stats;

public interface ISprintService
{
    public Task<List<TeamIteration>> getSprints(int count);
}