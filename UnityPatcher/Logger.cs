using System;

namespace UnityPatcher
{
    internal sealed class Logger
    {
        public static readonly Logger Debug = new Logger(ConsoleColor.DarkGray);
        public static readonly Logger Info = new Logger(ConsoleColor.Gray);
        public static readonly Logger Message = new Logger(ConsoleColor.Cyan);
        public static readonly Logger Success = new Logger(ConsoleColor.Green);
        public static readonly Logger Warning = new Logger(ConsoleColor.Yellow);
        public static readonly Logger Error = new Logger(ConsoleColor.Red);

        private readonly ConsoleColor _foregroundColor;

        private Logger(ConsoleColor foregroundColor)
        {
            _foregroundColor = foregroundColor;
        }

        public void Write(string text)
        {
            Console.ForegroundColor = _foregroundColor;
            Console.Write(text);
            Console.ResetColor();
        }

        public void WriteLines(params string[] lines)
        {
            Console.ForegroundColor = _foregroundColor;
            foreach (string line in lines)
                Console.WriteLine(line);
            Console.ResetColor();
        }
    }
}
