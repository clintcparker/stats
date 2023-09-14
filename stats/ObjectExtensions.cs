namespace stats;

public static class ObjectExtensions
{
    public static void IsNotNull<T>(this T obj, string message = "")
    {
        if (obj == null)
        {
            throw new ArgumentNullException(typeof(T).Name, message);
        }
    }
}