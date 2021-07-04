using System;
using Leaf.xNet;
using Youtube_Viewers_Re.Core.Objects;

namespace Youtube_Viewers_Re.CLI.Utils
{
    public static class XConsole
    {
        public const string Logo = @"/_/\/_/\   /________/\ /_______/\     /_____/\     /________/\ 
\ \ \ \ \  \__.::.__\/ \::: _  \ \    \:::_ \ \    \__.::.__\/ 
 \:\_\ \ \    \::\ \    \::(_)  \/_    \:\ \ \ \      \::\ \   
  \::::_\/     \::\ \    \::  _  \ \    \:\ \ \ \      \::\ \  
    \::\ \      \::\ \    \::(_)  \ \    \:\_\ \ \      \::\ \ 
     \__\/       \__\/     \_______\/     \_____\/       \__\/ 
";

        public const string GitRepo = "https://github.com/Airkek/Youtube-Viewers";
        public static int LogoPos = 0;

        /// <summary>
        /// Устанавливает Title программы на YTBot | {GitRepo}
        /// </summary>
        public static void SetTitle() => Console.Title = $"YTBot | {GitRepo}";

        public static string Ask(string question)
        {
            PrintLine(question);
            return InlineAsk("Your choice");
        }

        public static string InlineAsk(string question)
        {
            Print($"{question}: ");
            
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            
            var val = Console.ReadLine() ?? string.Empty;

            Console.ForegroundColor = color;

            return val;
        }

        public static int InlineAskInt(string question)
        {
            int i;
            
            while (!int.TryParse(InlineAsk(question), out i)) 
                PrintLine("Please, enter a number!", ConsoleColor.Red);

            return i;
        }
        
        public static int AskInt(string question)
        {
            int i;
            
            while (!int.TryParse(Ask(question), out i)) 
                PrintLine("Please, enter a number!", ConsoleColor.Red);

            return i;
        }
        
        public static bool AskBool(string question)
        {
            while (true)
            {
                var i = AskInt($"{question}\r\n1. Yes\r\n2. No");

                if (i < 1 || i > 2)
                {
                    PrintLine("Number must be 1 or 2", ConsoleColor.Red);
                    continue;
                }

                return i == 1;
            }
        }
        
        public static ProxyType SelectProxy()
        {
            while (true)
            {
                var i = AskInt("Please, select Proxy Type\r\n" +
                               "1. Http/s\r\n" +
                               "2. Socks4\r\n" +
                               "3. Socks5");

                switch (i)
                {
                    case 1:
                        return ProxyType.HTTP;
                    case 2:
                        return ProxyType.Socks4;
                    case 3:
                        return ProxyType.Socks5;
                    default:
                        PrintLine("Unknown proxy type!");
                        break;
                }
            }
        }

        public static void Print(string text, ConsoleColor color = ConsoleColor.White)
        {
            var prevColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            
            Console.Write(text);
            
            Console.ForegroundColor = prevColor;
        }

        public static void PrintLine(string text, ConsoleColor color = ConsoleColor.White)
        {
            Print(text, color);
            Console.WriteLine();
        }
        
        public static void PrintInfo(string key, string value, 
            ConsoleColor keyColor = ConsoleColor.White, ConsoleColor valueColor = ConsoleColor.Cyan)
        {
            Print($"{key}: ", keyColor);

            var width = Console.WindowWidth - value.Length - key.Length - 3;

            var toPrint = width > 0 ? value + new string(' ', width) : value;
            
            PrintLine(toPrint, valueColor);
        }

        public static void PrintInfo(string key, int value,
            ConsoleColor keyColor = ConsoleColor.White, ConsoleColor valueColor = ConsoleColor.Cyan) =>
            PrintInfo(key, value.ToString(), keyColor, valueColor);

        /// <summary>
        /// Перемещается под логотип и выводит статистику
        /// </summary>
        /// <param name="stats">VideoStats</param>
        /// <param name="botted">кол-во возвратов WorkerResponseStatus.Botted</param>
        /// <param name="errors">кол-во возвратов WorkerResponseStatus.Failed</param>
        /// <param name="proxies">кол-во загруженных прокси</param>
        public static void PrintStats(VideoStats stats, int botted, int errors, int proxies)
        {
            Console.SetCursorPosition(0, LogoPos);
            
            PrintInfo("Botted", botted);
            PrintInfo("Errors", errors);
            PrintInfo("Proxies", proxies);
            PrintInfo("Title", stats.Title);
            PrintInfo("Viewers", stats.Viewers);
        }
        
        /// <summary>
        /// Очищает консоль и выводит логотип
        /// </summary>
        /// <param name="color"></param>
        public static void PrintLogo(ConsoleColor color)
        {
            Console.Clear();
            
            PrintLine($"{Logo}\r\n", color);
            Print("Github: ");
            PrintLine(GitRepo, color);
            
            Console.WriteLine();

            LogoPos = Console.CursorTop;
        }
    }
}