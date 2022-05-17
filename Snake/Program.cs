using System;
using System.Threading;
using System.IO;
using System.Linq;

namespace Snake
{
    class Program
    {
        static readonly string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Snake\\";
        static int width = Console.WindowWidth;
        static int height = Console.WindowHeight;
        static int reactionTime = 50;
        static bool walls = false;

        static void Main()
        {
            Console.Clear();

            Write("SNAKE", width / 2 - 2, height / 5);
            Write("-----", width / 2 - 2, height / 5 + 1);
            Write("1. Play", width / 2 - 2, height / 5 + 3);
            Write("2. Settings", width / 2 - 2, height / 5 + 4);
            Write("3. Quit", width / 2 - 2, height / 5 + 5);

            while (true)
            {
                ConsoleKey k = Console.ReadKey(true).Key;

                if (k == ConsoleKey.D1)
                    Game();

                if (k == ConsoleKey.D2)
                {
                    Settings();
                    width = Console.WindowWidth;
                    height = Console.WindowHeight;
                    Main();
                    return;
                }

                if (k == ConsoleKey.D3)
                    Environment.Exit(0);
            }
        }

        static void Game()
        {
            Console.Clear();
            sbyte[,] stepMatrix = new sbyte[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    stepMatrix[i, j] = -1;
                }
            }

            int headX = width / 2;
            int headY = height / 2;
            stepMatrix[headX, headY] = 0;

            int tailX = headX;
            int tailY = headY + 1;
            stepMatrix[tailX, tailY] = 0;
            sbyte direction = 0;
            sbyte dir = 0;

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
                    stepMatrix[tailX, tailY] = -1;
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

                headX = WrapHead(headX, width - 1, score);
                headY = WrapHead(headY, height - 1, score);

                if (stepMatrix[headX, headY] != -1)
                {
                    Die(score);
                    return;
                }

                TimeSpan span = DateTime.Now - past;
                if (span.TotalMilliseconds < reactionTime)
                    Thread.Sleep(reactionTime - (int)span.TotalMilliseconds);
            }
        }

        static void Die(int score)
        {
            Console.Clear();

            Write("You died!", width / 2 - 4, height / 5);
            Write("---------", width / 2 - 4, height / 5 + 1);
            Write($"Score: {score}", width / 2 - 4, height / 5 + 3);

            bool highScore = false;

            if(!Directory.Exists(appdata))
                Directory.CreateDirectory(appdata);
            if (!File.Exists(appdata + "stats"))
            {
                File.WriteAllLines(appdata + "stats", new string[] { $"{walls}{score}" });
                highScore = true;
            }
            else
            {
                string[] text = File.ReadAllLines(appdata + "stats");

                bool executed = false;

                for(int i = 0; i < text.Length; i++)
                {
                    if (text[i].StartsWith(walls.ToString()))
                    {
                        executed = true;

                        if(int.Parse(text[i].Substring(4)) < score)
                        {
                            highScore = true;
                            text[i] = $"{walls}{score}";

                            File.WriteAllLines(appdata + "stats", text);
                        }

                        break;
                    }
                }

                if (!executed)
                {
                    File.AppendAllText(appdata + "stats", $"{walls}{score}");
                    highScore = true;
                }
            }

            if (highScore)
                Write("New high score!", width / 2 - 4, height / 5 + 4);
            else
            {
                foreach(string s in File.ReadAllLines(appdata + "stats"))
                {
                    if (s.StartsWith(walls.ToString()))
                        Write($"High score: {s.Substring(4)}", width / 2 - 4, height / 5 + 4);
                }
            }

            int input = CreateMenu(new string[] {
                "         ",
                "Retry",
                "Back to menu"
            }, false, 5);

            if (input == 1)
                Game();

            else if (input == 2)
                Main();
        }

        static int WrapHead(int value, int maxValue, int score)
        {
            if (value < 0)
            {
                if (walls)
                {
                    Die(score);
                    Environment.Exit(0);
                }
                value = maxValue;
            }
            else if (value >= maxValue)
            {
                if (walls)
                {
                    Die(score);
                    Environment.Exit(0);
                }
                value = 0;
            }
            return value;
        }

        static int Wrap(int value, int maxValue)
        {
            if (value < 0)
                value = maxValue;
            else if (value >= maxValue)
                value = 0;
            return value;
        }

        static void Write(string text, int posX, int posY, bool reset = false)
        {
            int oldX = 0;
            int oldY = 0;

            if (reset)
            {
                oldX = Console.CursorLeft;
                oldY = Console.CursorTop;
            }

            Console.SetCursorPosition(posX, posY);
            Console.Write(text);

            if (reset)
            {
                Console.SetCursorPosition(oldX, oldY);
            }
        }

        static int CreateMenu(string[] text, bool cleared = true, int offY = 0)
        {
            if(cleared)
                Console.Clear();

            for(int i = 0; i < text.Length; i++)
            {

                if(i == 0)
                {
                    Write(text[0], width / 2 - text[0].Length / 2, height / 5 + offY);

                    string underline = null;

                    for (int j = 0; j < text[0].Length; j++)
                        underline += "-";

                    Write(underline, width / 2 - text[0].Length / 2, height / 5 + 1 + offY);
                }

                else
                {
                    Write($"{i}. {text[i]}", width / 2 - text[0].Length / 2, height / 5 + i + 2 + offY);
                }
            }

            int output = 0;

            while (true)
            {
                string k = Console.ReadKey(true).KeyChar.ToString();

                int kNum = 0;

                if(int.TryParse(k, out kNum))
                {
                    if(kNum > 0 && kNum < text.Length)
                    {
                        output = kNum;
                        break;
                    }
                }
            }

            return output;
        }

        static void Settings()
        {
            int uInput = CreateMenu(new string[]
            {
                "Settings",
                "Difficulty",
                "Size",
                "Toggle Border Portals",
                "Back to menu"
            });

            if (uInput == 4)
                return;

            if (uInput == 1)
            {
                uInput = CreateMenu(new string[]
                {
                    "Difficulty",
                    "Easy",
                    "Medium",
                    "Hard",
                    "Apocalypse",
                    "Custom",
                    "Back to settings"
                });

                if(uInput == 6)
                {
                    Settings();
                    return;
                }

                if (uInput == 5)
                {
                    while (true)
                    {
                        Console.Clear();
                        Write("Custom Difficulty", width / 2 - 6, height / 5);
                        Write("-----------------", width / 2 - 6, height / 5 + 1);
                        Write("Set Reaction Time (ms): ", width / 2 - 6, height / 5 + 3);
                        Write("Leave empty to exit", width / 2 - 6, height / 5 + 5, true);

                        string input = Console.ReadLine();

                        if (input == "")
                            break;
                        if (int.TryParse(input, out reactionTime))
                        {
                            Settings();
                            return;
                        }
                    }
                }

                else
                {
                    switch (uInput)
                    {
                        case 1:
                            reactionTime = 200;
                            break;
                        case 2:
                            reactionTime = 50;
                            break;
                        case 3:
                            reactionTime = 30;
                            break;
                        case 4:
                            reactionTime = 10;
                            break;
                        default:
                            break;
                    }
                }
            }

            else if (uInput == 2)
            {
                uInput = CreateMenu(new string[] {
                    "Size",
                    "Small (60x60)",
                    "Medium (90x90),",
                    "Large (120x120)",
                    "Custom",
                    "Back to settings"
                });

                if (uInput == 5)
                {
                    Settings();
                    return;
                }

                else if (uInput == 4)
                {
                    Console.Clear();

                    Write("Custom Size", width / 2 - 6, height / 5);
                    Write("-----------", width / 2 - 6, height / 5 + 1);
                    Write("Width: ", width / 2 - 6, height / 5 + 3);
                    Write("Height: ", width / 2 - 6, height / 5 + 4, true);
                    Write("Leave empty to exit", width / 2 - 6, height / 5 + 5, true);

                    int newW, newH;

                    while (true)
                    {
                        string input = Console.ReadLine();

                        if (input == "")
                        {
                            Settings();
                            return;
                        }

                        if (int.TryParse(input, out newW))
                        {
                            break;
                        }
                    }

                    Console.SetCursorPosition(width / 2 + 2, height / 5 + 4);

                    while (true)
                    {
                        string input = Console.ReadLine();

                        if (input == "")
                        {
                            Settings();
                            return;
                        }

                        if (int.TryParse(input, out newH))
                        {
                            newH /= 2;

                            Console.SetWindowSize(newW, newH);
                            width = Console.WindowWidth;
                            height = Console.WindowHeight;
                            Settings();
                            return;
                        }
                    }
                }

                else
                {
                    switch (uInput)
                    {
                        case 1:
                            Console.SetWindowSize(60, 30);
                            Console.SetBufferSize(60, 30);
                            break;
                        case 2:
                            Console.SetWindowSize(90, 45);
                            Console.SetBufferSize(90, 45);
                            break;
                        case 3:
                            Console.SetWindowSize(120, 60);
                            Console.SetBufferSize(120, 60);
                            break;
                        default:
                            break;
                    }
                }
                
            }

            else if(uInput == 3)
            {
                walls = !walls;
            }
        }
    }
}