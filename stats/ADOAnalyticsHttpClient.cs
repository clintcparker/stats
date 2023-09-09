using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using System.Collections.Specialized;
    
    public class ADOAnalyticsHttpClient : HttpClient
    {
        public ADOAnalyticsHttpClient(string PAT) : base(new CacheHandler(new LoggingHandler(new HttpClientHandler())))
        {
            this.BaseAddress = new Uri("https://analytics.dev.azure.com/");
            this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", PAT))));
            this.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.DefaultRequestHeaders.Host = "analytics.dev.azure.com";
        }   
    }