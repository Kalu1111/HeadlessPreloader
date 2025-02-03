namespace HeadlessPreloader.Printing
{
    public class Spinner
    {
        public bool IsActive { get; private set; }
        private const string spinnerChars = "|/-\\";
        private int currentIndex;
        private string text;
        private int delay;
        private Thread thread;
        private ConsolePrinter printer;

        public Spinner(ConsolePrinter printer)
        {
            this.printer = printer;
        }
        public void Start(string text, int delay = 100)
        {
            try
            {
                this.text = text;
                this.delay = delay;
                if (thread?.IsAlive ?? false)
                    thread.Abort();

                if (!printer.IsSilent)
                {
                    printer.ImmediatePrintLine("", true, true, ConsoleColor.White);
                    thread = new Thread(Draw);
                    thread.Start();
                }
            }
            catch (Exception e)
            {
                Program.Nlog.Error(e);
            }
        }
        public void Stop(bool success)
        {
            IsActive = false;
            if (printer.IsSilent)
            {
                if (thread?.IsAlive ?? false)
                    thread.Join();
                return;
            }

            thread.Join();
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            printer.ImmediatePrintLine((success ? "LOADED  - " : "FAILED  - ") + text, true, true, success ? ConsoleColor.Green : ConsoleColor.Red);
        }

        private void Draw()
        {
            IsActive = true;
            while (IsActive)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                printer.ImmediatePrintLine($"LOADING {spinnerChars[currentIndex]} {text}", true, true, ConsoleColor.Yellow);
                Thread.Sleep(delay);
                currentIndex = (currentIndex + 1) % spinnerChars.Length;
            }
        }
    }
}
