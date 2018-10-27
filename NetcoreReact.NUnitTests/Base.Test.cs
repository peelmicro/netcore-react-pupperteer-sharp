using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using PuppeteerSharp;

namespace NetcoreReact.NUnitTests
{
    public class BaseTest : LoaderTest
    {
        protected Page Page { get; private set; }

        [SetUp]
        public async Task BaseSetup()
        {
            // Page = await Browser.NewPageAsync();
            // await Page.GoToAsync(Url);  
        } 
        [TearDown]
        public async Task BaseTearDown() 
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