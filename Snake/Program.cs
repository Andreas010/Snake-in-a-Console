using System;
using System.Threading;

namespace Snake
{
    class Program
    {
        static readonly int width = Console.WindowWidth;
        static readonly int height = Console.WindowHeight;
        static int reactionTime = 50;

        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();

                if (arg == "--help")
                {
                    Console.Write(
                        "SNAKE IN A CONSOLE\n" +
                        "------------------\n" +
                        "--help => This text\n" +
                        "--speed [value] => change the time waited between each step in ms (def=50)\n"
                        );
                    return;
                }
                else if (arg == "--speed" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int _reactionTime))
                        reactionTime = _reactionTime;
                }
            }

            while (true)
            {
                Game();
            }          
        }

        static void Game()
        {
            Console.Clear();
            byte[,] stepMatrix = new byte[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    stepMatrix[i, j] = 0xFF;
                }
            }

            int headX = width / 2;
            int headY = height / 2;
            stepMatrix[headX, headY] = 0;

            int tailX = headX;
            int tailY = headY + 1;
            stepMatrix[tailX, tailY] = 0;
            byte direction = 0;
            byte dir = 0;

            int foodX = headX;
            int foodY = headY - 1;
            int score = -1;

            Console.CursorVisible = false;

            Random r = new();

            while (true)
            {
                DateTime past = DateTime.Now;
                Console.SetCursorPosition(headX, headY);
                Console.Write('#');
                Console.SetCursorPosition(tailX, tailY);
                Console.Write(' ');
                Console.SetCursorPosition(foodX, foodY);
                Console.Write('+');

                if (headX == foodX && headY == foodY)
                {
                    Console.SetCursorPosition(foodX, foodY);
                    Console.Write('█');
                    foodX = r.Next(0, width - 1);
                    foodY = r.Next(0, height - 1);
                    score++;
                }
                else
                {
                    dir = stepMatrix[tailX, tailY];
                    stepMatrix[tailX, tailY] = 0xFF;
                    if (dir == 0)
                        tailY--;
                    else if (dir == 1)
                        tailX++;
                    else if (dir == 2)
                        tailY++;
                    else if (dir == 3)
                        tailX--;
                }

                tailX = Wrap(tailX, width - 1);
                tailY = Wrap(tailY, height - 1);

                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.UpArrow && direction != 2)
                        direction = 0;
                    else if (key == ConsoleKey.RightArrow && direction != 3)
                        direction = 1;
                    else if (key == ConsoleKey.DownArrow && direction != 0)
                        direction = 2;
                    else if (key == ConsoleKey.LeftArrow && direction != 1)
                        direction = 3;
                }

                dir = direction;
                stepMatrix[headX, headY] = dir;
                if (dir == 0)
                    headY--;
                else if (dir == 1)
                    headX++;
                else if (dir == 2)
                    headY++;
                else if (dir == 3)
                    headX--;

                headX = Wrap(headX, width - 1);
                headY = Wrap(headY, height - 1);

                if (stepMatrix[headX, headY] != 0xFF)
                {
                    Console.Clear();
                    while (Console.KeyAvailable)
                        Console.ReadKey(true);
                    Console.SetCursorPosition(width / 2 - 5, height / 2);
                    Console.Write("GAME  OVER");
                    Console.SetCursorPosition(width / 2 - 5, height / 2 + 2);
                    Console.Write($"SCORE: {score}");
                    Console.ReadKey(true);
                }

                TimeSpan span = DateTime.Now - past;
                if (span.TotalMilliseconds < reactionTime)
                    Thread.Sleep(reactionTime - (int)span.TotalMilliseconds);
            }
        }

        static int Wrap(int value, int maxValue)
        {
            if (value < 0)
                value = maxValue;
            else if (value >= maxValue)
                value = 0;
            return value;
        }
    }
}