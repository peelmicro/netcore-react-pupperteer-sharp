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
using Xunit.Abstractions;
using Xunit.Sdk;

namespace NetcoreReact.IntegrationTests
{
    public class LoaderFixture : IDisposable
    {
        IWebHost _webHost = null;
        private readonly IMessageSink _messageSink;

        public string Url { get; set; } = "http://localhost:5000/";
        // public Browser Browser { get; set; }
        public TestServer TestServer { get; private set; }
        public HttpClient HttpClient { get; private set; }

        public LoaderFixture(IMessageSink messageSink)
        {
            _messageSink = messageSink;
            SetupAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Task.Run(DisposeAsync).Wait();
        }
        public async Task DisposeAsync()
        {
            TestServer.Dispose();
            // await Browser.CloseAsync();
            await _webHost.StopAsync();
        }
        private async Task SetupAsync()
        {
            await StartWebServerAsync();
        }

        private async Task StartWebServerAsync()
        {
            var mainApp = Assembly.GetExecutingAssembly().FullName.Split(',').First().Replace(".IntegrationTests", "");
            var mainPath = Path.GetFullPath($"../../../../{mainApp}");
            _messageSink.OnMessage(new DiagnosticMessage("Before creating WebHostBuilder"));
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
            _messageSink.OnMessage(new Xunit.Sdk.DiagnosticMessage("After creating WebHostBuilder"));

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
            _messageSink.OnMessage(new Xunit.Sdk.DiagnosticMessage("After creating TestServer"));

            HttpClient = TestServer.CreateClient();
            _messageSink.OnMessage(new Xunit.Sdk.DiagnosticMessage("After creating HttpClient"));

            // await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            // _messageSink.OnMessage(new Xunit.Sdk.DiagnosticMessage("After downloading Browser with DownloadAsync"));

            // Browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            // _messageSink.OnMessage(new Xunit.Sdk.DiagnosticMessage("launching Browser"));

        }


    }
}
