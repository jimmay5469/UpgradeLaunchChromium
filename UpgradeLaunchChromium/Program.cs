using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace UpgradeLaunchChromium
{
    class Program
    {
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            GetLatestChromium();
        }
        public static void GetLatestChromium()
        {
            HttpClient client = new HttpClient();
            var latestVerstion = client.GetStringAsync("http://commondatastorage.googleapis.com/chromium-browser-continuous/Win/LAST_CHANGE").Result;

            DownloadChromium(latestVerstion);
            TryRemoveChromium();
            InstallChromium();
            LaunchChromium();
        }
        public static void TryRemoveChromium()
        {
            if (Directory.Exists("chromium"))
            {
                Console.WriteLine(" ");
                Console.WriteLine("Uninstalling old version of Chromium...");
                try
                {
                    DeleteDirectory("chromium");
                }
                catch
                {
                    Console.WriteLine("Please close all instances of Chromium and then press the ENTER key to continue.");
                    Console.ReadLine();
                    TryRemoveChromium();
                    return;
                }
            }
        }
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }
            Directory.Delete(target_dir, false);
        }
        public static void DownloadChromium(string latestVerstion)
        {
            Console.WriteLine("Downloading latest version of Chromium...");
            WebClient filereader = new WebClient();
            filereader.DownloadProgressChanged += filereader_DownloadProgressChanged;
            filereader.DownloadFileCompleted += filereader_DownloadFileCompleted;
            filereader.DownloadFileAsync(new Uri(string.Format("http://commondatastorage.googleapis.com/chromium-browser-continuous/Win/{0}/chrome-win32.zip", latestVerstion)), "chrome-win32.zip");
            resetEvent.WaitOne();
        }
        static void filereader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write(string.Format("\r     Progress: {0}/{1} MB", Math.Round(e.BytesReceived / 1048576d, 1).ToString("N1"), Math.Round(e.TotalBytesToReceive / 1048576d, 1).ToString("N1")));
        }
        static void filereader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine(" ");
            resetEvent.Set();
        }
        public static void InstallChromium()
        {
            UnzipChromium();
            RemoveTempFiles();
        }
        public static void UnzipChromium()
        {
            Console.WriteLine("Installing latest version of Chromium...");
            ZipFile.ExtractToDirectory("chrome-win32.zip", "chromium");
        }
        public static void RemoveTempFiles()
        {
            Console.WriteLine("Removing temp files...");
            File.Delete("chrome-win32.zip");
        }
        public static void LaunchChromium()
        {
            Console.WriteLine("Launching latest version of Chromium...");
            Process.Start("chromium\\chrome-win32\\chrome.exe");
        }
    }
}
