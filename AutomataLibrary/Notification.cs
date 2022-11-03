using System;

namespace AutomataLibrary2D
{
    public static class Notification
    {
        public static string TimeStamp(int input)
        {
            int miliseconds = input % 1000;
            int seconds = ((input - miliseconds) / 1000) % 60;
            int mins = ((input - miliseconds) / 1000) / 60;
            return " - elapsed time " + mins + "m:" + seconds + "s:" + miliseconds + "ms (or " + input + "ms)";
        }
        public static void CompletionNotification(int inf, int sup, ref bool written, string tabs)
        {
            if (Math.Round(inf / (double)sup * 100, 2) % 10 == 0)
            {
                if (written) Console.SetCursorPosition(0, Console.CursorTop);
                else written = true;
                Console.Write(tabs + "..." + Math.Round(inf / (double)sup * 100, 2) + "%");
            }
        }
        public static void FinalCompletionNotification(string tabs)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(tabs + "...100%\n");
        }
        public static void PrintProgramLabel()
        {
            Console.WriteLine("NextGen2D Apps - Cellular Automata Programs");
            Console.WriteLine("Copyright (C) 2019 Darien Viera Barredo \n");
        }
    }
}
