using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;
using static System.MathF;

namespace Consode
{
    public class Player
    {
        public float X = 1;
        public float Y = 1;
        public float A = 0;
        public float Speed = 5;
        private float sinA = 0, cosA = 1;

        public const float FOV = 60 * MathF.PI / 180;//0.7853975f;
        public const float RotateSpeed = 1.3f;

        public void MoveForward()
        {
            X += sinA * Speed * Consode.DeltaTime;
            Y += cosA * Speed * Consode.DeltaTime;
            while (Consode.Map[(int)X, (int)Y])
            {
                X -= sinA * Speed * Consode.DeltaTime;
                Y -= cosA * Speed * Consode.DeltaTime;
            }
        }

        public void MoveBackward()
        {
            X -= sinA * Speed * Consode.DeltaTime;
            Y -= cosA * Speed * Consode.DeltaTime;
            while (Consode.Map[(int)X, (int)Y])
            {
                X += sinA * Speed * Consode.DeltaTime;
                Y += cosA * Speed * Consode.DeltaTime;
            }
        }

        public void TurnLeft()
        {
            A -= RotateSpeed * Consode.DeltaTime;
            sinA = Sin(A);
            cosA = Cos(A);
        }

        public void TurnRight()
        {
            A += RotateSpeed * Consode.DeltaTime;
            sinA = Sin(A);
            cosA = Cos(A);
        }
    }

    public static class Consode
    {
        #region Consts
        public static int ScreenWidth = 120;
        public static int ScreenHeight = 40;
        public static int MapWidth = 40;
        public static int MapHeight = 40;

        public const float ViewDepth = 40.0f;

        public const string Logo =
        " ###########     ###########    ###       ###    ############    ###########    ########        #############\n" +
        "#############   #############   ######    ###   #############   #############   ###    ###      #############\n" +
        "###             ###       ###   ######    ###   ###             ###       ###   ###      ###    ###		  \n" +
        "###             ###       ###   ###  ###  ###   ############    ###       ###   ###       ###   #############\n" +
        "###             ###       ###   ###  ###  ###   #############   ###       ###   ###       ###   #############\n" +
        "###             ###       ###   ###    ######             ###   ###       ###   ###      ###    ###		  \n" +
        "#############   #############   ###    ######   #############   #############   ###    ###      #############\n" +
        " ###########     ###########    ###       ###   ############     ###########    ########        #############";
        public const string SettingsLogo =
                " ############   #############   #############   #############   #############   ###       ###    ############    ############\n" +
                "#############   #############   #############   #############   #############   ######    ###   #############   #############\n" +
                "###             ###                  ###             ###             ###        ######    ###   ###             ###          \n" +
                "############    #############        ###             ###             ###        ###  ###  ###   ###    ######   ############ \n" +
                "#############   #############        ###             ###             ###        ###  ###  ###   ###    ######   #############\n" +
                "          ###   ###                  ###             ###             ###        ###    ######   ###       ###             ###\n" +
                "#############   #############        ###             ###        #############   ###    ######   #############   #############\n" +
                "############    #############        ###             ###        #############   ###       ###    ###########    ############ ";

        #endregion

        public static float DeltaTime { get; private set; }
        public static char[,] CharMap { get; private set; }
        public static bool[,] Map { get; private set; }
        public static readonly string[] Maps = new string[]
        {
            "empty.txt",
            "2x2.txt",
            "2x2 pt.2.txt",
            "map.txt",
        };

        private static bool drawMap, showDebug = false, running = true, errorQuit = false, noCap = false, disableFlash = false;
        private static int targetFramerate = 40;
        private static TimeSpan targetFrameTime = TimeSpan.FromSeconds(1 / 40.0);
        private static Player player;
        private static char[] screen;

        public static void Main()
        {
            try
            {
                Console.CursorVisible = false;
                Console.Title = "Consode.NET";
                Console.Clear();
                LoadSettings();
                int msel = -1, csel = -1;
                while (msel != 0)
                {
                    msel = SelectionMenu(16, 8, Logo, "START", "SETTINGS");
                    if (msel == 1)
                    {
                        while (csel != 0)
                        {
                            csel = SelectionMenu(16, 8, SettingsLogo, "BACK", "TARGET FRAMERATE", "SHOW DEBUG INFO", "DISABLE FLASH");
                            if (csel == 0) break;
                            switch (csel)
                            {
                                case 1: targetFramerate = SettingsMenu(16, 8, SettingsLogo, "TARGET FRAMERATE(NO FRAMERATE CAP IF -1)", -1, 200, targetFramerate); break;
                                case 2: showDebug = SettingsMenu(16, 8, SettingsLogo, "SHOW DEBUG INFO", 0, 1, showDebug ? 1 : 0) == 1; break;
                                case 3: disableFlash = SettingsMenu(16, 8, SettingsLogo, "DISABLE FLASH", 0, 1, disableFlash ? 1 : 0) == 1; break;
                            }
                            SaveSettings();
                        }
                    }
                }

                int mapId = SelectionMenu(16, 8, Logo + "\n\nSelect map:", "\t1. Empty map", "\t2. 2x2 map by gstroin", "\t3. 2x2 pt.2 map by gstroin", "\t4. Load your own map (map.txt)");
                Console.WriteLine();

                LoadMap(mapId);

                Thread.Sleep(640);
                if (!disableFlash)
                {
                    var r = new Random();
                    for (int i = 0; i < 8; i++)
                    {
                        Console.Clear();
                        switch (i)
                        {
                            case 0: Console.ForegroundColor = ConsoleColor.Blue; break;
                            case 2: Console.ForegroundColor = ConsoleColor.Red; break;
                            case 4: Console.ForegroundColor = ConsoleColor.DarkRed; break;
                            case 6: Console.ForegroundColor = ConsoleColor.DarkBlue; break;
                            default: Console.ForegroundColor = ConsoleColor.White; break;
                        }
                        int x = (int)(16 + (r.NextDouble() * 2 - 1) * (7 - i)), y = (int)(8 + (r.NextDouble() * 2 - 1) * (7 - i));
                        foreach (var line in Logo.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                        {
                            Console.SetCursorPosition(x, y++);
                            Console.Write(line);
                        }
                        Thread.Sleep(100);
                    }
                }
                Thread.Sleep(200);

                player = new Player();
                var upd = DateTime.UtcNow;
                int pwwidth = 0, pwheight = 0;

                while (running)
                {
                    int wwidth = Console.WindowWidth, wheight = Console.WindowHeight;
                    if (pwwidth != wwidth || pwheight != wheight)
                    {
                        Console.Clear();
                        ScreenWidth = wwidth;
                        ScreenHeight = wheight;
                        screen = new char[ScreenWidth * ScreenHeight];
                    }
                    if (wwidth < 20 || wheight < 20)
                    {
                        Console.Write("Too small!");
                        Thread.Sleep(100);
                        continue;
                    }
                    pwwidth = wwidth; pwheight = wheight;

                    DeltaTime = (float)(DateTime.UtcNow - upd).TotalSeconds;
                    upd = DateTime.UtcNow;

                    CheckInputs();

                    RayCasting();

                    Draw();

                    if (!noCap)
                    {
                        var sleep = targetFrameTime - (DateTime.UtcNow - upd);
                        Thread.Sleep(sleep > TimeSpan.Zero ? sleep : TimeSpan.Zero);
                    }
                }
            }
            catch (Exception e)
            {
                errorQuit = true;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Well, unknown error occurred. Please, create issue(https://github.com/Nebulaxin/Consode.NET/issues/new) and attach errorLog.txt to it.");
                Console.ResetColor();

                const string sep = "\n================================\n";
                File.AppendAllText("errorLog.txt", File.Exists("errorlog.txt") ? $"{sep}{e}\n" : $"Consode.NET Error Log\n{sep}{e}\n");
            }
            finally
            {
                if (!errorQuit) Console.Clear();
                Console.ResetColor();
                Console.CursorVisible = true;
            }
        }

        private static int Snap(int val, int max, int min = 0)// min incl, max excl
        {
            return val < min ? max - 1 : (val >= max ? min : val);
        }

        private static int SelectionMenu(int left, int top, string header, params string[] items)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.SetCursorPosition(left, top);
            var splitHeader = header.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitHeader.Length; i++)
            {
                Console.CursorLeft = left;
                Console.WriteLine(splitHeader[i]);
                Thread.Sleep(40);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            int sel = 0, max = items.Length, nTop = top + splitHeader.Length + 2;
            while (true)
            {
                Console.SetCursorPosition(left, nTop);
                for (int i = 0; i < max; i++)
                {
                    Console.CursorLeft = left;
                    Console.WriteLine((sel == i ? "   >" : "    ") + items[i]);
                    Thread.Sleep(40);
                }
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.DownArrow: sel = Snap(sel + 1, max); break;
                    case ConsoleKey.UpArrow: sel = Snap(sel - 1, max); break;
                    case ConsoleKey.Enter: return sel;
                }
            }
        }

        private static int SettingsMenu(int left, int top, string header, string setting, int min, int max, int def)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(left, top);
            var splitHeader = header.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitHeader.Length; i++)
            {
                Console.CursorLeft = left;
                Console.WriteLine(splitHeader[i]);
                Thread.Sleep(40);
            }
            Console.ForegroundColor = ConsoleColor.Magenta;
            int val = def, nTop = top + splitHeader.Length + 2;
            while (true)
            {
                Console.SetCursorPosition(left, nTop);
                Console.WriteLine((val == min ? "    " : " <  ") + setting + (val == max ? "    " : "  > ") + "   : " + val + "   ");
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.RightArrow: val = Math.Clamp(val + 1, min, max); break;
                    case ConsoleKey.LeftArrow: val = Math.Clamp(val - 1, min, max); break;
                    case ConsoleKey.Enter: return val;
                }
            }
        }

        private static void LoadSettings()
        {
            if (File.Exists("settings"))
            {
                using (var sr = File.OpenText("settings"))
                {
                    if(!sr.EndOfStream) targetFramerate = int.Parse(sr.ReadLine());
                    if (!(noCap = targetFramerate == -1)) targetFrameTime = TimeSpan.FromSeconds(1.0 / targetFramerate);
                    if(!sr.EndOfStream) showDebug = bool.Parse(sr.ReadLine());
                    if(!sr.EndOfStream) disableFlash = bool.Parse(sr.ReadLine());
                }
            }
            else SaveSettings();
        }

        private static void SaveSettings()
        {
            using (var sw = File.CreateText("settings"))
            {
                sw.WriteLine(targetFramerate);
                sw.WriteLine(showDebug);
                sw.WriteLine(disableFlash);
            }
        }

        private static void LoadMap(int id)
        {
            try
            {
                Console.WriteLine("Loading...");
                var map = File.ReadAllLines("maps/" + Maps[id - 1]);
                MapWidth = map[0].Length;
                MapHeight = map.Length;
                CharMap = new char[MapWidth, MapHeight];
                Map = new bool[MapWidth, MapHeight];
                for (int i = 0; i < map.Length; i++)
                {
                    var line = map[i];
                    for (int j = 0; j < MapWidth; j++)
                        Map[j, i] = (CharMap[j, i] = line[j]) == '#';
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Can't load map, using new one(8x8)");
                MapWidth = MapHeight = 8;
                CharMap = new char[8, 8]
                {
                { '#', '#', '#', '#', '#', '#', '#', '#' },
                { '#', ' ', ' ', ' ', ' ', ' ', ' ', '#' },
                { '#', ' ', ' ', ' ', ' ', ' ', ' ', '#' },
                { '#', ' ', ' ', ' ', ' ', ' ', ' ', '#' },
                { '#', ' ', ' ', ' ', ' ', ' ', ' ', '#' },
                { '#', ' ', ' ', ' ', ' ', ' ', ' ', '#' },
                { '#', ' ', ' ', ' ', ' ', ' ', ' ', '#' },
                { '#', '#', '#', '#', '#', '#', '#', '#' }
                };
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                        Map[j, i] = CharMap[j, i] == '#';
            }

        }

        private static void CheckInputs()
        {
            if (Console.KeyAvailable)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.W: player.MoveForward(); break;
                    case ConsoleKey.S: player.MoveBackward(); break;
                    case ConsoleKey.A: player.TurnLeft(); break;
                    case ConsoleKey.D: player.TurnRight(); break;
                    case ConsoleKey.M: drawMap = !drawMap; break;
                    case ConsoleKey.Escape: running = false; break;
                }
            }
        }

        private static List<(float, float)> p = new List<(float, float)>();
        private static void RayCasting()
        {
            for (int x = 0; x < ScreenWidth; x++)
            {
                float rayAngle = (player.A - Player.FOV / 2.0f) + ((float)x / (float)ScreenWidth) * Player.FOV;

                float distanceToWall = 0, stepSize = 0.005f;

                bool hitWall = false, boundary = false;

                float eyeX = Sin(rayAngle);
                float eyeY = Cos(rayAngle);

                while (!hitWall && distanceToWall < ViewDepth)
                {
                    distanceToWall += stepSize;
                    int testX = (int)(player.X + eyeX * distanceToWall);
                    int testY = (int)(player.Y + eyeY * distanceToWall);

                    if (testX < 0 || testX >= MapWidth || testY < 0 || testY >= MapHeight)
                    {
                        hitWall = true;
                        distanceToWall = ViewDepth;
                    }
                    else if (Map[testX, testY])
                    {
                        hitWall = true;
                        p.Clear();

                        for (int tx = 0; tx < 2; tx++)
                        {
                            for (int ty = 0; ty < 2; ty++)
                            {
                                float vx = (float)testX + tx - player.X;
                                float vy = (float)testY + ty - player.Y;
                                float d = Sqrt(vx * vx + vy * vy);
                                float dot = (eyeX * vx / d) + (eyeY * vy / d);

                                p.Add((d, dot));
                            }
                        }
                        p.Sort((a, b) => Sign(a.Item1 - b.Item1));

                        float bound = 0.005f;
                        if (Acos(p[0].Item2) < bound) boundary = true;
                        if (Acos(p[1].Item2) < bound) boundary = true;
                    }

                }

                int ceiling = (int)(ScreenHeight / 2f - ScreenHeight / distanceToWall);
                int floor = ScreenHeight - ceiling;

                char nShade = ' ';
                if (boundary) nShade = '|';
                else if (distanceToWall <= ViewDepth * 0.25f) nShade = '\x2588';
                else if (distanceToWall < ViewDepth * 0.5f) nShade = '\x2593';
                else if (distanceToWall < ViewDepth * 0.75f) nShade = '\x2592';
                else if (distanceToWall < ViewDepth) nShade = '\x2591';
                else nShade = ' ';


                for (int y = 0; y < ScreenHeight; y++)
                {
                    if (y <= ceiling)
                        screen[x + ScreenWidth * y] = ' ';
                    else if (y > ceiling && y <= floor)
                        screen[x + ScreenWidth * y] = nShade;
                    else
                    {
                        float b = 1.0f - (((float)y - ScreenHeight / 2.0f) / ((float)ScreenHeight / 2.0f));

                        if (b < 0.25) nShade = '#';
                        else if (b < 0.5) nShade = 'x';
                        else if (b < 0.75) nShade = '~';
                        else if (b < 0.9) nShade = '.';
                        else nShade = ' ';
                        screen[x + ScreenWidth * y] = nShade;
                    }
                }
            }
        }

        private static void Draw()
        {
            if (drawMap)
            {
                for (int y = 0; y < Math.Min(MapHeight, ScreenHeight); y++)
                {
                    for (int x = 0; x < Math.Min(MapWidth, ScreenWidth); x++)
                    {
                        screen[y * ScreenWidth + x] = CharMap[x, y];
                    }
                }
                int px = (int)player.X + 1, py = (int)player.Y;
                if (px >= 0 && px < ScreenWidth && py >= 0 && py < ScreenHeight)
                    screen[px + py * ScreenWidth] = 'P';
            }

            screen[(ScreenWidth / 2) + ScreenWidth * (ScreenHeight / 2 - 1)] = 'H';
            screen[(ScreenWidth / 2 - 2) + ScreenWidth * (ScreenHeight / 2)] = '=';
            screen[(ScreenWidth / 2 - 1) + ScreenWidth * (ScreenHeight / 2)] = '=';
            screen[(ScreenWidth / 2) + ScreenWidth * (ScreenHeight / 2)] = '+';
            screen[(ScreenWidth / 2 + 1) + ScreenWidth * (ScreenHeight / 2)] = '=';
            screen[(ScreenWidth / 2 + 2) + ScreenWidth * (ScreenHeight / 2)] = '=';
            screen[(ScreenWidth / 2) + ScreenWidth * (ScreenHeight / 2 + 1)] = 'H';



            Console.SetCursorPosition(0, 0);
            Console.Write(screen);
            Console.SetCursorPosition(0, ScreenHeight - 1);
            if (showDebug)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($"X:{player.X:0.00}; Y:{player.Y:0.00}; A:{((player.A * 180 / MathF.PI + 180) % 360 - 180):0.00}; FPS:{(int)(1 / DeltaTime):000}(target: {targetFramerate})  ----- WASD - Move Player; M - Open Map");
                Console.ResetColor();
            }
        }
    }
}
