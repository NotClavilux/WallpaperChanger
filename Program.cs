using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetConsoleWindow();

    private const int SW_HIDE = 0;

    static async Task Main()
    {
        IntPtr consoleHandle = GetConsoleWindow();
        ShowWindow(consoleHandle, SW_HIDE);

        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string imageFolder = Path.Combine(baseDirectory, "Wallpapers");

        if (!Directory.Exists(imageFolder))
        {
            Directory.CreateDirectory(imageFolder);
        }

        var imageUrls = new Dictionary<string, string>
        {
            { "morning", "https://w.wallhaven.cc/full/5g/wallhaven-5gpv25.jpg" },
            { "afternoon", "https://w.wallhaven.cc/full/kx/wallhaven-kxj3l1.jpg" },
            { "evening", "https://w.wallhaven.cc/full/1p/wallhaven-1poo61.jpg" },
            { "night", "https://w.wallhaven.cc/full/yj/wallhaven-yjog2l.jpg" }
        };

        while (true)
        {
            string timeOfDay = GetTimeOfDay();
            string wallpaperUrl = imageUrls.ContainsKey(timeOfDay) ? imageUrls[timeOfDay] : "https://w.wallhaven.cc/full/1p/wallhaven-1pd1o9.jpg";
            string wallpaperPath = Path.Combine(imageFolder, $"{timeOfDay}_wallpaper.jpg");

            if (!File.Exists(wallpaperPath))
            {
                await DownloadImageAsync(wallpaperUrl, wallpaperPath);
            }

            SetWallpaper(wallpaperPath);

            await Task.Delay(300000);
        }
    }

    static string GetTimeOfDay()
    {
        int currentHour = DateTime.Now.Hour;

        if (currentHour >= 5 && currentHour < 12)
        {
            return "morning";
        }
        else if (currentHour >= 12 && currentHour < 17)
        {
            return "afternoon";
        }
        else if (currentHour >= 17 && currentHour < 20)
        {
            return "evening";
        }
        else
        {
            return "night";
        }
    }

    static void SetWallpaper(string imagePath)
    {
        bool result = SystemParametersInfo(0x0014, 0, imagePath, 0x0001 | 0x0002);
        if (!result)
        {
            Console.WriteLine("Failed to set wallpaper.");
        }
    }

    static async Task DownloadImageAsync(string url, string savePath)
    {
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(savePath, response);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
}
