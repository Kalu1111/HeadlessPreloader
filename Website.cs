using PuppeteerSharp.Input;
using PuppeteerSharp;
using System.Text.RegularExpressions;

namespace HeadlessPreloader
{
    public class Website
    {
        public string Url { get; set; }
        public SharedConfigurations OptionalSettings { get; set; }

        public override string ToString() => Url;
        public string GetFullUrl(string domain) => Url.StartsWith("http") ? Url.Trim() : domain.Trim() + Url.Trim();
        public bool DoesUrlMatch(string domain, string url) => url?.StartsWith(GetFullUrl(domain)) ?? false;
        public SharedConfigurations GetEffectiveSettings(SharedConfigurations globalSettings)
        {
            return new SharedConfigurations
            {
                AwaitMode = getValidSetting(OptionalSettings?.AwaitMode, globalSettings.AwaitMode),
                AdditionalDelaySeconds = getValidSetting(OptionalSettings?.AdditionalDelaySeconds, globalSettings.AdditionalDelaySeconds),
                TimeoutSeconds = getValidSetting(OptionalSettings?.TimeoutSeconds, globalSettings.TimeoutSeconds),
                MaxAttemptsBeforeSkip = getValidSetting(OptionalSettings?.MaxAttemptsBeforeSkip, globalSettings.MaxAttemptsBeforeSkip)
            };
        }
        public async Task<bool> Load(BrowserManager browserMan, Configurations config)
        {
            var failCount = 0;
            var urlAfter = "";
            var url = GetFullUrl(config.WebsiteDomain);
            var settings = GetEffectiveSettings(config.GlobalSettings);
            var timeout = settings.TimeoutSeconds * 1000;
            var additionalDelay = settings.AdditionalDelaySeconds * 1000;
            var awaitMode = settings.GetAwaitMode();
            var maxAtempts = settings.MaxAttemptsBeforeSkip;

            while (failCount < maxAtempts)
                try
                {
                    var page = await browserMan.NewPageAsync(timeout);
                    if (page != null)
                    {
                        await page.GoToAsync(url, new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { awaitMode }, Timeout = timeout });
                        await Task.Delay(additionalDelay);
                        urlAfter = page.Url;

                        if (config.IsLoginPage(urlAfter))
                            await config.LoginPage.Login(page);
                        await page.CloseAsync();

                        //fail if the requested page isn't the login page and we got redirected, and fail if we didn't get redirected if the requested page was a login page (probably meaning our login attempt failed)
                        if ((url != urlAfter && !config.IsLoginPage(url)) || (url == urlAfter && config.IsLoginPage(url)))
                            failCount++;
                        else
                            break;
                    }
                    else
                        failCount++;
                }
                catch (Exception e)
                {
                    Program.PrintLine("Load process threw an exception.\nFor further information please check my log files.", color: ConsoleColor.Red);
                    Program.Nlog.Error($"WEBSITE - {url} - failed with the following exception:\n{e}");
                    failCount++;
                    await browserMan.Init(config.DisplayWebBrowser); //restart the browser to get a clean start
                }

            var success = failCount < maxAtempts;
            if (!success)
            {
                if (!config.IsLoginPage(url))
                    Program.PrintLine("Website exceeded the configured amount of retries and failed to load", color: ConsoleColor.Red);
                else
                    Program.PrintLine("All login attempts have failed.", color: ConsoleColor.Red);

                Program.Nlog.Warn($"WEBSITE - {url} - Exceeded the configured amount of retries and failed to load.");
            }

            return success;
        }
        public virtual bool ValidateImport()
        {
            var allValid = true;
            if (string.IsNullOrWhiteSpace(Url))
            {
                Program.PrintLine($"WARNING: Url - field cannot be empty", color: ConsoleColor.Yellow);
                allValid = false;
            }
            else
            if (!(Uri.TryCreate(Url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                && !Regex.IsMatch(Url, @"^(\/[^\/\s]+\/?)*$"))
            {
                Program.PrintLine($"WARNING: Url ({Url}) - field could not be parsed as either a relative or absolute url", color: ConsoleColor.Yellow);
                allValid = false;
            }

            return (OptionalSettings?.ValidateImport() ?? true) && allValid;
        }

        private int getValidSetting(int? optionalSetting, int globalSetting)
        {
            if (optionalSetting.HasValue && optionalSetting.Value > 0)
                return optionalSetting.Value;
            return globalSetting < 0 ? 0 : globalSetting;
        }
    }

    public class LoginWebsite : Website
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string UsernameFieldId { get; set; }
        public string PasswordFieldId { get; set; }

        public async Task<bool> Login(IPage page)
        {
            string field = null;
            if (!string.IsNullOrEmpty(UsernameFieldId) && await page.QuerySelectorAsync(UsernameFieldId) == null)
                field = "Username";
            else if (!string.IsNullOrEmpty(PasswordFieldId) && await page.QuerySelectorAsync(PasswordFieldId) == null)
                field = "Password";

            if (field != null)
            {
                var errorMsg = $"{field} field could not be found!";
                Program.PrintLine(errorMsg, color: ConsoleColor.Red);
                Program.Nlog.Warn(errorMsg);
                return false;
            }

            if (!string.IsNullOrEmpty(UsernameFieldId))
                await page.TypeAsync(UsernameFieldId, Username, new TypeOptions() { Delay = 133 });
            if (!string.IsNullOrEmpty(PasswordFieldId))
                await page.TypeAsync(PasswordFieldId, Password, new TypeOptions() { Delay = 133 });
            await page.Keyboard.PressAsync("Enter");
            await Task.Delay(500);
            await page.Keyboard.PressAsync("Enter");
            await Task.Delay(10000);

            return true;
        }

        public override bool ValidateImport()
        {
            var allValid = true;
            if (string.IsNullOrWhiteSpace(Username))
            {
                Program.PrintLine($"WARNING: Username - field should probably not be empty", color: ConsoleColor.Yellow);
                allValid = false;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                Program.PrintLine($"WARNING: Password - field should probably not be empty", color: ConsoleColor.Yellow);
                allValid = false;
            }
            if (string.IsNullOrWhiteSpace(UsernameFieldId))
            {
                Program.PrintLine($"WARNING: UsernameFieldId - field should probably not be empty", color: ConsoleColor.Yellow);
                allValid = false;
            }
            if (string.IsNullOrWhiteSpace(PasswordFieldId))
            {
                Program.PrintLine($"WARNING: PasswordFieldId - field should probably not be empty", color: ConsoleColor.Yellow);
                allValid = false;
            }

            return base.ValidateImport() && allValid;
        }

    }
}
