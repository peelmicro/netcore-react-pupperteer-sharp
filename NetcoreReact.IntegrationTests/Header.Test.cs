
using Xunit;
using Xunit.Abstractions;

namespace NetcoreReact.IntegrationTests
{
    [Collection("Loader collection")]
    public class HeaderTest:  BaseTest
    {
        public HeaderTest(LoaderFixture fixture, ITestOutputHelper output) : base(fixture, output)
        {
        }

        [Fact]
        public async void TheHeaderHasTheCorrectText() {
            var text = await GetContentOf("#root h1");
            Assert.Equal("Hello, world!", text);
        }

    }
}
