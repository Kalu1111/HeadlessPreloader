using HeadlessPreloader.Printing;
using NLog;
using System.CommandLine;
using System.Diagnostics;

namespace HeadlessPreloader
{
    public class Program
    {
        private static Configurations config;
        private static BrowserManager browserMan { get; } = new BrowserManager();
        private static ConsolePrinter printer { get; } = new ConsolePrinter();

        public static Logger Nlog { get; private set; } = LogManager.GetCurrentClassLogger();
        public static bool IsSilent
        {
            get { return printer.IsSilent; }
            private set { printer.IsSilent = value; }
        }


        public static void PrintLine(string txt, bool forceSingleLine = false, bool flush = false, ConsoleColor color = ConsoleColor.White)
            => printer.PrintLine(txt, forceSingleLine, flush, color);
        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (object s, EventArgs e) => { browserMan.Dispose(); };
            await ParseCommandLineArgs(args);
        }
        private static async Task<bool> InitialLoadAndValidations()
        {
            if (config == null)
                config = Configurations.Load();
            if (config == null)
            {
                PrintLine("Configuration file could not be loaded", color: ConsoleColor.Red);
                return false;
            }

            PrintLine("Checking for Chromium browser updates and downloading the latest version...");
            await browserMan.Init(config.DisplayWebBrowser);
            if (config.LoginPage != null)
            {
                PrintLine("Testing login configurations...");
                printer.StartSpinner("LOGIN TEST");
                var success = await config.LoginPage.Load(browserMan, config);
                printer.StopSpinner(success);
                if (!success)
                    return false;
            }

            return true;
        }
        private static async Task Run()
        {
            if (await InitialLoadAndValidations())
            {
                var count = 0;
                var globalFailCount = 0;
                var timer = new Stopwatch();
                var siteListCount = config.Websites.Count();

                PrintLine("Loading webpages...");
                timer.Start();
                foreach (var site in config.Websites)
                {
                    printer.StartSpinner($"{++count}/{siteListCount} - {site.GetFullUrl(config.WebsiteDomain)}");
                    var success = await site.Load(browserMan, config);
                    if (!success)
                        globalFailCount++;
                    printer.StopSpinner(success);
                }
                timer.Stop();
                browserMan.Dispose();

                PrintLine("\n\nJob's done!");
                PrintLine($"The entire job took ~{(int)timer.Elapsed.TotalMinutes} minute(s), and on average each page took ~{(int)(timer.Elapsed.TotalSeconds / siteListCount)} second(s)\n");
                PrintLine(globalFailCount == 0 ? "All webpages appear to have loaded correctly :)" : $"{globalFailCount} page(s) failed to load.\nThese are displayed above, in red. You might want to take a look at them.", color: globalFailCount == 0 ? ConsoleColor.Green : ConsoleColor.Red);
            }
        }
        private static async Task<int> ParseCommandLineArgs(string[] args)
        {
            var cmd = new RootCommand(
                 @"After you deploy your website, I preload its pages so that they'll be nice and snappy for your end user. For information on how to configure me please check my README.txt file."
                );

            var configOption = new Option<string>(
                "--configFile",
                "Full path to the config file, including extension");

            var runOption = new Option<bool>(
                "--run",
                "Starts processing the webpages defined in the config file");

            var silentOption = new Option<bool>(
                "--silent",
                "Stops all console messages");

            var editConfigOption = new Option<bool>(
                "--editConfig",
                "Opens a text editor with the default config file");

            var validateConfigOption = new Option<bool>(
                "--validateConfig",
                "Runs a config file validation test, letting you know if you've configured it correctly");

            var readmeOption = new Option<bool>(
                "--readme",
                "Opens a text editor with the README.txt file so you can learn how to configure me");

            cmd.AddOption(configOption);
            cmd.AddOption(runOption);
            cmd.AddOption(silentOption);
            cmd.AddOption(editConfigOption);
            cmd.AddOption(validateConfigOption);
            cmd.AddOption(readmeOption);
            cmd.SetHandler(async (string configFile, bool run, bool silent, bool editConfig, bool validateConfig, bool readme) =>
            {
                try
                {
                    if (editConfig)
                    {
                        Configurations.OpenEditor();
                        return;
                    }
                    if (readme)
                    {
                        Configurations.OpenReadme();
                        return;
                    }
                    if (validateConfig)
                    {
                        await InitialLoadAndValidations();
                        return;
                    }

                    IsSilent = silent;

                    if (!string.IsNullOrEmpty(configFile))
                        config = Configurations.Load(configFile);
                    if (run)
                        await Run();
                }
                catch (Exception e)
                {
                    Nlog.Error($"GENERAL - Execution failed with the following exception:\n{e}");
                }
            }, configOption, runOption, silentOption, editConfigOption, validateConfigOption, readmeOption);

            return await cmd.InvokeAsync(args.Length == 0 ? new[] { "--help" } : args);
        }
    }
}
