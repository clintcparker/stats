


public class ADOResponse<T>
{
    public int count { get; set; }
    public List<T> value { get; set; }

    public List<T> GetValue()
    {
        return value ?? new List<T>();
    }
}