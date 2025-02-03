using PuppeteerSharp;
using System.Text.Json;
using System.Diagnostics;
using System;


namespace HeadlessPreloader
{
    public class Configurations
    {
        public bool DisplayWebBrowser { get; set; }
        public SharedConfigurations GlobalSettings { get; set; }
        public string WebsiteDomain { get; set; }
        public LoginWebsite LoginPage { get; set; }
        public List<Website> Websites { get; set; }

        public bool IsLoginPage(string url) => LoginPage?.DoesUrlMatch(WebsiteDomain, url) ?? false;
        public static Configurations Load(string path = null)
        {
            Configurations config = null;
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    path = DefaultConfig.TITLE;
                else if (!File.Exists(path))
                    return null;

                if (!CreateDefaultConfigFileIfDoesntExit())
                {
                    Program.PrintLine($"Attempting to read configuration file at {path}");
                    config = JsonSerializer.Deserialize<Configurations>(File.ReadAllText(path));
                    Program.PrintLine($"Configurations parsed successfully\nValidating imported values...");
                    config.ValidateImport();
                }
            }
            catch (JsonException ex0)
            {
                Program.PrintLine("Configuration file is either empty or its content it malformed. Please check the log file for more details", color: ConsoleColor.Red);
                Program.Nlog.Error($"CONFIG - {path} - failed to load configuration file with the following exception:\n{ex0}");
            }
            catch (Exception ex1)
            {
                Program.PrintLine("Configuration file has failed to load. Check the log files for more details. Please check the log file for more details", color: ConsoleColor.Red);
                Program.Nlog.Error($"CONFIG - {path} - failed to load configuration file with the following exception:\n{ex1}");
            }

            return config;
        }
        public static void OpenEditor()
        {
            CreateDefaultConfigFileIfDoesntExit();
            Process.Start(new ProcessStartInfo { FileName = DefaultConfig.TITLE, UseShellExecute = true });
        }
        public static void OpenReadme()
        {
            try
            {
                Program.PrintLine(File.ReadAllText(DefaultConfig.README), color: ConsoleColor.Blue);
                Process.Start(new ProcessStartInfo { FileName = DefaultConfig.README, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Program.Nlog.Error(ex.ToString());
            }
        }

        public bool ValidateImport()
        {
            var allValid = true;

            if (!(string.IsNullOrEmpty(WebsiteDomain) || Uri.TryCreate(WebsiteDomain, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)) || (WebsiteDomain?.Trim().EndsWith('/') ?? false))
            {
                Program.PrintLine(
                    "WARNING: WebsiteDomain - field should be an http/https url and not end in '/' (relative urls should all start with '/')",
                    color: ConsoleColor.Yellow);
                allValid = false;
            }

            allValid = GlobalSettings.ValidateImport() && allValid;
            allValid = (LoginPage?.ValidateImport() ?? true) && allValid;
            foreach (var site in Websites)
                allValid = site.ValidateImport() && allValid;

            return allValid;
        }
        private static bool CreateDefaultConfigFileIfDoesntExit()
        {
            if (!File.Exists(DefaultConfig.TITLE))
            {
                File.Create(DefaultConfig.TITLE).Dispose();
                File.WriteAllText(DefaultConfig.TITLE, JsonSerializer.Serialize(DefaultConfig.DEFAULT_CONTENT, new JsonSerializerOptions { WriteIndented = true }));
                Program.PrintLine($"Could not find a configuration file.\nCreating a default one at {Path.GetFullPath(DefaultConfig.TITLE)}", color: ConsoleColor.Yellow);
                Program.PrintLine($"This config will run as default if no other gets specified by '--configFile'.\nYou'll want to edit it before running me again to make sure the correct websites are listed.", color: ConsoleColor.Yellow);

                return true;
            }
            return false;
        }
    }

    public class SharedConfigurations
    {
        public int AwaitMode { get; set; }
        public int AdditionalDelaySeconds { get; set; }
        public int TimeoutSeconds { get; set; }
        public int MaxAttemptsBeforeSkip { get; set; }

        public WaitUntilNavigation GetAwaitMode() => (WaitUntilNavigation)AwaitMode;
        public bool ValidateImport()
        {
            var allValid = true;
            if (AwaitMode < 0 || AwaitMode > 3)
            {
                Program.PrintLine("WARNING: AwaitMode - field can only be a value between 0 and 3. Defaulting to 0.", color: ConsoleColor.Yellow);
                AwaitMode = 0;
                allValid = false;
            }
            if (AdditionalDelaySeconds < 0)
            {
                Program.PrintLine("WARNING: AdditionalDelaySeconds - value cannot be negative. Defaulting to 3.", color: ConsoleColor.Yellow);
                AdditionalDelaySeconds = 3;
                allValid = false;
            }
            if (AdditionalDelaySeconds < 0)
            {
                Program.PrintLine("WARNING: TimeoutSeconds - value cannot be negative. Defaulting to 0.", color: ConsoleColor.Yellow);
                AdditionalDelaySeconds = 0;
                allValid = false;
            }
            if (MaxAttemptsBeforeSkip < 1)
            {
                Program.PrintLine("WARNING: MaxAttemptsBeforeSkip - value cannot be lower than 1. Defaulting to 10.", color: ConsoleColor.Yellow);
                MaxAttemptsBeforeSkip = 10;
                allValid = false;
            }

            return allValid;
        }
    }

    public static class DefaultConfig
    {
        public const string TITLE = "config.json";
        public const string README = "README.txt";

        public static readonly Configurations DEFAULT_CONTENT = new Configurations()
        {
            DisplayWebBrowser = false,
            GlobalSettings = new SharedConfigurations
            {
                AwaitMode = 0,
                AdditionalDelaySeconds = 2,
                TimeoutSeconds = 0,
                MaxAttemptsBeforeSkip = 10
            },
            WebsiteDomain = "https://www.youtube.com",
            LoginPage = new LoginWebsite()
            {
                Url = "https://www.sketchuptextureclub.com/login",
                Username = "homelessc.raft.8.600@gmail.com",
                Password = "homelessc.raft.8.600@gmail.com",
                UsernameFieldId = "#mail",
                PasswordFieldId = "#password",
                OptionalSettings = new SharedConfigurations
                {
                    MaxAttemptsBeforeSkip = 2
                }
            },
            Websites = new List<Website>()
            {
                new Website()
                {
                    Url = "https://one.outsystems.com/log-in"
                },
                new Website()
                {
                    Url = "/watch?v=dQw4w9WgXcQ"
                },
                new Website()
                {
                    Url = "/watch?v=dQw4w9WgXcQ"
                }
            }
        };
    }
}
