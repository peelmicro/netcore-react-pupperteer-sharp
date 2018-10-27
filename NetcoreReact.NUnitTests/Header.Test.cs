
using System.Threading.Tasks;
using NUnit.Framework;

namespace NetcoreReact.NUnitTests
{
    public class HeaderTest:  BaseTest
    {
        [Test]
        public async Task TheHeaderHasTheCorrectText() {
            // var text = await GetContentOf("#root h1");
            var text ="Hello, world!";
            Assert.AreEqual("Hello, world!", text);
        }

    }
}
