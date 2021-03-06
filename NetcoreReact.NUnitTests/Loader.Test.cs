using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;
using NUnit.Framework;

namespace NetcoreReact.NUnitTests
{
    public class LoaderTest 
    {
        IWebHost _webHost = null;
        public string Url { get; set; } = "http://localhost:5000/";
        public Browser Browser { get; set; }
        public TestServer TestServer { get; private set; }
        public HttpClient HttpClient { get; private set; }

        [SetUp]
        public async Task LoaderSetup()
        {
           var mainApp = Assembly.GetExecutingAssembly().FullName.Split(',').First().Replace(".NUnitTests", "");
            var mainPath = Path.GetFullPath($"../../../../{mainApp}");
            Console.WriteLine("Before creating WebHostBuilder");
            _webHost = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .UseEnvironment("Staging")
                .UseUrls(Url)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(mainPath);
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddEnvironmentVariables();
                })
                .UseContentRoot(mainPath)
                .Build();
            await _webHost.StartAsync();
            Console.WriteLine("Before creating TestServer");

            TestServer = new TestServer(
                new WebHostBuilder()
                    .UseStartup<Startup>()
                    .UseContentRoot(mainPath)
                    .UseEnvironment("Staging")
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.SetBasePath(mainPath);
                        config.AddJsonFile("appsettings.json", optional: true);
                        config.AddEnvironmentVariables();
                    })
                );
            Console.WriteLine("Before creating HttpClient");

            HttpClient = TestServer.CreateClient();
            Console.WriteLine("Before Downloading Browser");

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            Console.WriteLine("Before Launching Browser");

            Browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        } 
        [TearDown]
        public async Task LoaderTearDown() 
        { 
            TestServer.Dispose();
            await Browser.CloseAsync();
            await _webHost.StopAsync();
        }        
    }
}