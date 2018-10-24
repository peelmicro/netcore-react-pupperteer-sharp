
using Xunit;

namespace NetcoreReact.IntegrationTests
{
    [Collection("Loader collection")]
    public class HeaderTest:  BaseTest
    {
        public HeaderTest(LoaderFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void TheHeaderHasTheCorrectText() {
            var text = await GetContentOf("#root h1");
            Assert.Equal("Hello, world!", text);
        }

    }
}
