namespace HeadlessPreloader.Printing
{
    public class ConsolePrinter
    {
        public bool IsSilent { get; set; } = false;

        private Spinner spinner;
        private Queue<(string, bool, bool, ConsoleColor)> printQueue;

        public ConsolePrinter()
        {
            spinner = new Spinner(this);
            printQueue = new Queue<(string, bool, bool, ConsoleColor)>();
        }
        public void PrintLine(string txt, bool forceSingleLine, bool flush, ConsoleColor color)
        {
            if (IsSilent)
                return;

            if (spinner.IsActive)
                printQueue.Enqueue((txt, forceSingleLine, flush, color));
            else
                ImmediatePrintLine(txt, forceSingleLine, flush, color);
        }
        public void StartSpinner(string text, int delay = 200) => spinner.Start(text, delay);
        public void StopSpinner(bool success)
        {
            spinner.Stop(success);
            while (printQueue.Count > 0)
            {
                var param = printQueue.Dequeue();
                ImmediatePrintLine(param.Item1, param.Item2, param.Item3, param.Item4);
            }
        }
        internal void ImmediatePrintLine(string txt, bool forceSingleLine, bool flush, ConsoleColor color)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = color;
                int maxTextLength = Console.WindowWidth - 4;
                if (forceSingleLine && txt.Length > maxTextLength)
                    Console.WriteLine(txt.Substring(0, maxTextLength) + "...");
                else
                    Console.WriteLine(txt);

                Console.ResetColor();
                if (flush)
                    Console.Out.Flush();
            }
        }
    }
}
