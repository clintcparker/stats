namespace stats;

public class SystemHelpers : ISystemHelpers
{
    public string GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    public bool Exists(string path)
    {
        return File.Exists(path);
    }
}

public interface ISystemHelpers
{
    string GetEnvironmentVariable(string name);
    bool Exists(string path);
}
