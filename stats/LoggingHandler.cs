
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using System.Collections.Specialized;
public class LoggingHandler : DelegatingHandler
{

    private readonly bool _logRequestAndResponse = false;
    public LoggingHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
        _logRequestAndResponse = true;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_logRequestAndResponse){
            Console.WriteLine("Request:");
            Console.WriteLine(request.ToString());
            if (request.Content != null)
            {
                Console.WriteLine(await request.Content.ReadAsStringAsync());
            }
            Console.WriteLine();
        }
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        if (_logRequestAndResponse){
            Console.WriteLine("Response:");
            Console.WriteLine(response.ToString());
            if (response.Content != null)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            Console.WriteLine();
        }

        return response;
    }
}