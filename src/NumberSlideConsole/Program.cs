using System.Text;
using NumberSlide.Core;

namespace NumberSlideConsole;

internal static class Program
{
    private const byte MinWidth = 3;
    private const byte MaxWidth = 10;

    private static Game game = null!;
    
    private static void Main()
    {
        Console.CursorVisible = false;
        
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "数字华容道";

        byte width = ReadBoardWidth();
        game = new Game(width);
        string message = string.Empty;

        while (true)
        {
            ShowGameScreen(message);
            ConsoleKey key = Console.ReadKey(true).Key;
            message = string.Empty;

            switch (key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    game.Move(Direction.Left);
                    break;
                
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    game.Move(Direction.Up);
                    break;
                
                case ConsoleKey.RightArrow: 
                case ConsoleKey.D:
                    game.Move(Direction.Right);
                    break;
                
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    game.Move(Direction.Down);
                    break;
                
                case ConsoleKey.Z:
                    game.Undo();
                    break;
                
                case ConsoleKey.R:
                    game.Restart();
                    break;

                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    ClearScreen();
                    Console.WriteLine("已退出数字华容道。");
                    return;
            }
            
            if (game.IsComplete()) break;
        }

        ShowVictoryScreen();
    }

    private static byte ReadBoardWidth()
    {
        while (true)
        {
            ClearScreen();
            Console.WriteLine("========================================");
            Console.WriteLine("              数字华容道");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("  请选择棋盘大小：");
            Console.WriteLine("    3 - 3 × 3 入门模式");
            Console.WriteLine("    4 - 4 × 4 经典模式");
            Console.WriteLine("    5 ~ 10 - 自定义难度");
            Console.WriteLine();
            Console.Write($"  请输入 {MinWidth} 到 {MaxWidth}：");

            string? input = Console.ReadLine();
            if (byte.TryParse(input?.Trim(), out byte width) && width is >= MinWidth and <= MaxWidth)
            {
                return width;
            }

            Console.WriteLine();
            Console.WriteLine($"  输入无效，请输入 {MinWidth} 到 {MaxWidth} 之间的整数。");
            Console.Write("  按任意键重新选择...");
            Console.ReadKey(true);
        }
    }

    private static void ShowGameScreen(string message)
    {
        ClearScreen();
        Console.WriteLine("========================================");
        Console.WriteLine($"              数字华容道 {game.board.Width} × {game.board.Width}");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("  目标：按顺序排列数字，并将空格留在右下角。");
        Console.WriteLine();

        ShowMap();

        Console.WriteLine();
        Console.WriteLine("  方向键 / WASD  移动空格");
        Console.WriteLine("  Z              撤销");
        Console.WriteLine("  R              重新开始");
        Console.WriteLine("  Q / Esc        退出");

        if (!string.IsNullOrEmpty(message))
        {
            Console.WriteLine();
            Console.WriteLine($"  {message}");
        }
    }

    private static void ShowVictoryScreen()
    {
        ClearScreen();
        Console.WriteLine("========================================");
        Console.WriteLine("              恭喜完成！");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine($"  数字华容道 {game.board.Width} × {game.board.Width}");
        Console.WriteLine("  所有数字已按顺序排列，空格位于右下角。");
        Console.WriteLine();

        ShowMap();

        Console.WriteLine();
        Console.WriteLine("  本局挑战完成！");
        Console.Write("  按任意键退出...");

        if (!Console.IsInputRedirected)
            Console.ReadKey(true);

        Console.WriteLine();
    }

    private static void ClearScreen()
    {
        if (!Console.IsOutputRedirected)
            Console.Clear();
    }

    private static void ShowMap()
    {
        byte[,] map = game.board.BoardMap;
        byte width = game.board.Width;
        int cellWidth = (width * width - 1).ToString().Length + 2;

        WriteBorder(width, cellWidth, '┌', '┬', '┐');

        for (byte row = 0; row < width; row++)
        {
            Console.Write("│");
            for (byte column = 0; column < width; column++)
            {
                string text = map[row, column] == 0
                    ? " "
                    : map[row, column].ToString();

                Console.Write(CenterText(text, cellWidth));
                Console.Write("│");
            }

            Console.WriteLine();

            WriteBorder(
                width,
                cellWidth,
                row == width - 1 ? '└' : '├',
                row == width - 1 ? '┴' : '┼',
                row == width - 1 ? '┘' : '┤');
        }
    }

    private static string CenterText(string text, int width)
    {
        int remaining = width - text.Length;
        int left = remaining / 2;
        int right = remaining - left;
        return new string(' ', left) + text + new string(' ', right);
    }

    private static void WriteBorder(
        byte width,
        int cellWidth,
        char left,
        char middle,
        char right)
    {
        Console.Write(left);

        for (byte column = 0; column < width; column++)
        {
            Console.Write(new string('─', cellWidth));
            Console.Write(column == width - 1 ? right : middle);
        }

        Console.WriteLine();
    }
}
