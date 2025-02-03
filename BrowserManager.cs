using PuppeteerSharp;

namespace HeadlessPreloader
{
    public class BrowserManager : IDisposable
    {
        private IBrowser browser;

        public async Task Init(bool displayBrowser)
        {
            if (!browser?.IsClosed ?? false)
                await browser.CloseAsync();

            var revisionInfo = await new BrowserFetcher().DownloadAsync(BrowserTag.Stable);
            browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = !displayBrowser, ExecutablePath = revisionInfo.GetExecutablePath(), Timeout = 0, ProtocolTimeout = 0 });
            browser.DefaultWaitForTimeout = 0;
            await Task.Delay(1000);
        }

        public async Task<IPage> NewPageAsync(int timeout)
        {
            var page = await browser.NewPageAsync();
            page.DefaultNavigationTimeout = timeout;
            page.DefaultTimeout = timeout;
            return page;
        }

        public void Dispose()
        {
            if (browser != null)
            {
                if (!browser.IsClosed)
                    browser.CloseAsync().GetAwaiter().GetResult();
                browser.Dispose();
                browser = null;
            }
        }
    }
}
