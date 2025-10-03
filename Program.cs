using System.Diagnostics;
using System.Text;

class Program
{
    static ConsoleColor[] _consoleColors = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
    static int _consoleWidth;
    static int _consoleHeight;
    static char[,] _charMap;
    static int[,] _historyMap;
    static int _fps = 30;
    static long _frequency = Stopwatch.Frequency;
    static long _ticksPerFrame = _frequency / _fps;
    static readonly object _locker = new object();

    static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Thread handleKeys = new Thread(KeyHandler);
        handleKeys.Start();

        const string charSet = "1234567890-=qwertyuiop[]\\';lkjhgfdsazxcvbnm,./QAZWSXEDCRFVTGBYHNUJMIK<OL>P:?{}|";
        const int dropSize = 13;

        Random rnd = new Random();
        int[] rndColumns;
        int _0rowColsCount = 0;

        Stopwatch sw = new Stopwatch();
        string realFps;

        while (true)
        {
            sw.Start();
            CheckResize();

            rndColumns = new int[1 + (_consoleWidth / dropSize) / 10];
            for (int i = 0; i < rndColumns.Length; i++)
            {
                rndColumns[i] = rnd.Next(_consoleWidth);
            }

            for (int i = 0; i < _consoleHeight; i++)
            {
                for (int j = 0; j < _consoleWidth; j++)
                {
                    if (_historyMap[i, j] != 0 && _historyMap[i, j] < dropSize)
                        _historyMap[i, j]++;

                    if (i == 0)
                    {
                        if (rndColumns.Contains(j))
                        {
                            _historyMap[i, j] = 1;
                            _0rowColsCount++;
                            _charMap[i, j] = charSet[rnd.Next(charSet.Length)];
                        }
                    }
                    else if (_historyMap[i - 1, j] == 2)
                    {
                        _historyMap[i, j] = 1;
                        _charMap[i, j] = charSet[rnd.Next(charSet.Length)];
                    }

                    if (_historyMap[i, j] == dropSize)
                    {
                        _historyMap[i, j] = 0;
                        _charMap[i, j] = ' ';
                    }
                }
            }

            while(sw.ElapsedTicks < _ticksPerFrame) { }
            realFps = (_frequency / sw.ElapsedTicks).ToString("D4");
            for (int i = 0; i < realFps.Length; i++)
                _charMap[0, i] = realFps[i];

            Print();
            sw.Reset();
        }
    }

    static void InitCharMap()
    {
        for (int i = 0; i < _consoleHeight; i++)
            for (int j = 0; j < _consoleWidth; j++)
                _charMap[i, j] = ' ';
    }

    static void Print()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _consoleHeight; i++)
            for (int j = 0; j < _consoleWidth; j++)
                sb.Append(_charMap[i, j]);

        try
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(sb);
        }
        catch { }
    }

    static void CheckResize()
    {
        int height = Console.WindowHeight;
        int width = Console.WindowWidth;

        if (_consoleHeight != height
            || _consoleWidth != width)
        {
            try
            {
                Console.Clear();
                Console.SetBufferSize(width, height);

                _consoleHeight = height;
                _consoleWidth = width;

                _charMap = new char[_consoleHeight, _consoleWidth];
                InitCharMap();
                _historyMap = new int[_consoleHeight, _consoleWidth];
            }
            catch { }
        }
    }

    static void KeyHandler()
    {
        ConsoleKey key;
        int ccIndex = 1;
        while (true)
        {
            lock (_locker)
            {
                key = Console.ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.RightArrow:
                        ccIndex = ccIndex < _consoleColors.Length - 1 ? ++ccIndex : 1;
                        Console.ForegroundColor = _consoleColors[ccIndex];
                        break;
                    case ConsoleKey.LeftArrow:
                        ccIndex = ccIndex > 1 ? --ccIndex : _consoleColors.Length - 1;
                        Console.ForegroundColor = _consoleColors[ccIndex];
                        break;
                    case ConsoleKey.UpArrow:
                        _fps++;
                        _ticksPerFrame = _frequency / _fps;
                        break;
                    case ConsoleKey.DownArrow:
                        _fps = _fps > 1 ? --_fps : 1;
                        _ticksPerFrame = _frequency / _fps;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
