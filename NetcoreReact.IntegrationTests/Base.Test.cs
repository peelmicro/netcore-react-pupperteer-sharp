using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using Xunit;

namespace NetcoreReact.IntegrationTests
{
    [Collection("Loader collection")]
    public class BaseTest:  IDisposable
    {
        protected Browser Browser { get; set; }
        protected Page Page { get; private set; }
        protected TestServer TestServer { get; private set; }
        protected HttpClient HttpClient { get; private set; }
        LoaderFixture _fixture;

        public BaseTest(LoaderFixture fixture)
        {
            _fixture = fixture;
            Browser = _fixture.Browser;
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
            Page = await Browser.NewPageAsync();
            await Page.GoToAsync(_fixture.Url);   
        }

        public async Task DisposeAsync()
        {
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