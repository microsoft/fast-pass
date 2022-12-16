using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;


[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class HomeNavigationTests : PageTest
{
    const string URL = "https://brave-mushroom-056103510.2.azurestaticapps.net/";

    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync(URL);
    }

    [Test]
    public async Task HomepageHasCorrectUrlInPath()
    {
        await Expect(Page).ToHaveURLAsync(URL);
    }
}
