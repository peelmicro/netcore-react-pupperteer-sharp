using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppeteerSharp;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace NetcoreReact.IntegrationTests
{
    [Collection("Loader collection")]
    public class BaseTest:  IDisposable
    {
        protected Browser Browser { get; set; }
        protected Page Page { get; private set; }
        protected TestServer TestServer { get; private set; }
        protected HttpClient HttpClient { get; private set; }
        private readonly LoaderFixture _fixture;
        private readonly ITestOutputHelper _output;

        public BaseTest(LoaderFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            // Browser = _fixture.Browser;
            TestServer = _fixture.TestServer;
            HttpClient = _fixture.HttpClient;            
            Task.Run(InitializeAsync).Wait();
        }

        public void Dispose()
        {
            Task.Run(DisposeAsync).Wait();
        }
        public async Task InitializeAsync()
        {
            _output.WriteLine("Before downloading Browser with DownloadAsync");
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            _output.WriteLine("After downloading Browser with DownloadAsync");

            Browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            _output.WriteLine("launching Browser");
            Page = await Browser.NewPageAsync();
            await Page.GoToAsync(_fixture.Url);   
        }

        public async Task DisposeAsync()
        {
            await Browser.CloseAsync();
            await Page.CloseAsync();
        }

		#region protected help methods
        protected async Task ClickAndWaitForNavigation(string selector) 
        {
            await Task.WhenAll(
                Page.ClickAsync(selector),
                Page.WaitForNavigationAsync()); 
        }
        protected async Task<string> GetContentOf(string selector)
        {
            var attempts = 0;
            while (attempts < 3)
            {
                attempts++;
                try
                {
                    var text = await Page.QuerySelectorAsync(selector)
                        .EvaluateFunctionAsync<string>("el => el.innerHTML");
                    return text;
                }
                catch (Exception)
                {
                    await Task.Delay(1000);
                }
            }
            return null;
        }

        protected async Task<dynamic> Get(params object[] args)
        {
            var result = await Page.EvaluateFunctionAsync(@"
              (_path) => {
                return fetch(_path, {
                  method: 'GET',
                  credentials: 'same-origin',
                  headers: {
                    'Content-Type': 'application/json'
                  }
                })
                  .then(res => res.json());
              }
            ", args);
            return result;
        }

        protected async Task<dynamic> Post(params object[] args)
        {
            var result = await Page.EvaluateFunctionAsync(@"
              (_path, _data) => {
                return fetch(_path, {
                  method: 'POST',
                  credentials: 'same-origin',
                  headers: {
                    'Content-Type': 'application/json'
                  },
                  body: JSON.stringify(_data)
                })
                  .then(res => res.json());
              }
            ", args);
            return result;
        }

        private delegate dynamic Request(params object[] args);

        protected async Task<List<dynamic>> ExecRequests(IEnumerable<object> requests)
        {
            var results = new List<dynamic>();
            foreach (dynamic request in requests)
            {
                Request rq = null;
                switch (request.method)
                {
                    case "get":
                        rq = Get;
                        break;
                    case "post":
                        rq = Post;
                        break;
                }

                if (rq == null) continue;
                var result = await rq(request.path, request.data);
                results.Add(result);
            }

            return results;
        }
        #endregion

 
    }
}