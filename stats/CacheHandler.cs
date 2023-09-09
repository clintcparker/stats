
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using System.Collections.Specialized;
using System.Net;
using System.IO;
public class CacheHandler : DelegatingHandler
{

    public CacheHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
    }

    //this hanlder caches requests and responses to a temp file and returns them if they are in the cache
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cacheFileName = await getCacheFileName(request);
        Console.WriteLine($"cacheFileName: {cacheFileName}");
        if (File.Exists(cacheFileName))
        {
            var response = await File.ReadAllTextAsync(cacheFileName);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response)
            };
        }
        HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken);
        var responseString = await responseMessage.Content.ReadAsStringAsync();
        await File.WriteAllTextAsync(cacheFileName, responseString);
        return responseMessage;
    }

    public async Task<string> getCacheFileName(HttpRequestMessage request)
    {
        var cacheFileName = request.RequestUri.AbsolutePath.Replace("/", "_");
        var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
        foreach (var key in query.AllKeys)
        {
            cacheFileName += $"_{key}_{query[key]}";
        }
        cacheFileName = HashString(cacheFileName);
        //var tempPath = Path.GetTempPath();
        var tempPath = Path.GetFullPath("./cache");
        cacheFileName = Path.Combine(tempPath, cacheFileName);
        return cacheFileName;
    }

    static string HashString(string text, string salt = "")
    {
        if (String.IsNullOrEmpty(text))
        {
            return String.Empty;
        }

        // Uses SHA256 to create the hash
        using (var sha = new System.Security.Cryptography.SHA256Managed())
        {
            // Convert the string to a byte array first, to be processed
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text + salt);
            byte[] hashBytes = sha.ComputeHash(textBytes);

            // Convert back to a string, removing the '-' that BitConverter adds
            string hash = BitConverter
                .ToString(hashBytes)
                .Replace("-", String.Empty);

            return hash;
        }
    }
}