using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.Extensions.Options;
using VSWebApp.Configuration;

namespace VSWebApp.Controllers
{
    [Produces("application/json")]
    [Route("api/Search")]

    public class SearchController : Controller
    {
        private readonly BingImageSearchConfiguration _bingImageSearchConfiguration;

        public SearchController(IOptions<BingImageSearchConfiguration> bingImageSearchConfiguration)
        {
            _bingImageSearchConfiguration = bingImageSearchConfiguration.Value;
        }

        [HttpPost]
        public async Task Post(string market = null)
        {
            var baseUri = market == null ? _bingImageSearchConfiguration.ApiUrl : $"{ _bingImageSearchConfiguration.ApiUrl}?mkt={market}";

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, baseUri))
            {
                var subscriptionkey = _bingImageSearchConfiguration.Subscriptionkey;
                HttpContent content = null;
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionkey);
                if (Request.HasFormContentType)
                {
                    MultipartFormDataContent mfdc = new MultipartFormDataContent();
                    var form = await Request.ReadFormAsync();
                    foreach (var kvp in form)
                    {
                        var k = kvp.Key;
                        var v = kvp.Value;
                        mfdc.Add(new StringContent(v.ToString()), k);
                    }

                    foreach (var file in form.Files)
                    {
                        mfdc.Add(new StreamContent(file.OpenReadStream()), file.Name, file.FileName);
                    }

                    content = mfdc;
                }
                request.Content = content;
                
                using (var response = await client.SendAsync(request))
                {
                    Response.ContentType = response.Content.Headers.ContentType.MediaType;

                    var stream = await response.Content.ReadAsStreamAsync();
                    stream.CopyTo(Response.Body);

                    Response.ContentLength = stream.Length;
                }
            }
        }
    }
}
