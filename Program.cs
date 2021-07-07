using System;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;

namespace HorizonPackage
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args[0] == "install")
                {
                    Console.WriteLine("  Installing your package...");
                    Directory.CreateDirectory(".temp");
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (var client = new WebClient())
                    {
                        string url = "https://openlight.me/cdn.openlight.me/horizon/" + args[1] + "/bundle.zip";
                        string file = ".temp/bundle.zip";
                        client.DownloadFile(url, file);
                        ExtractZipFileToDirectory(file, "./", true);
                        File.Delete(".temp/bundle.zip");
                        Console.WriteLine("  Package "+ args[1]+" has been installed successfully!", ConsoleColor.Green);
                    }

                    Console.WriteLine(" ");
                }
                else if (args[0] == "update")
                {
                    Console.Clear();
                    Console.WriteLine(" ");
                    Console.WriteLine("  Package.horizon can't update 32-bit (x86) or ARM versions of the OS.");
                    Console.WriteLine("  If you are running a 32-bit (x86) or ARM version of the OS, please close this window and update manually.");
                    Console.WriteLine(" ");
                    Console.WriteLine("  If you are running a 64-bit (x64) version of the OS, press [Enter] to continue.");
                    Console.ReadLine();

                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" ");
                    Console.WriteLine("  Updating HorizonOS...");
                    Console.ForegroundColor = ConsoleColor.White;

                    // kill the OS
                    foreach (Process proc in Process.GetProcessesByName("Horizon64"))
                    {
                        proc.Kill();
                    }

                    Directory.CreateDirectory(".temp");
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (var client = new WebClient())
                    {
                        string url = "https://github.com/openlight-team/HorizonOS/releases/latest/download/update.package";
                        string file = ".temp/bundle.zip";
                        client.DownloadFile(url, file);
                        ExtractZipFileToDirectory(file, "./", true);
                        File.Delete(".temp/bundle.zip");
                        Console.WriteLine("  HorizonOS has been updated successfully!");
                        Console.WriteLine(" ");
                        Thread.Sleep(1000);
                    }

                    // restart the OS
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = "Horizon64.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                }
                else
                {
                    Console.WriteLine("  Use package.horizon:install [package] to install a package.");
                    Console.WriteLine(" ");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("  SOMETHING WENT WRONG :/");
                Console.WriteLine("  Use package.horizon:install [package] to install a package.");
                Console.WriteLine("  ---------- technical info ----------");
                Console.WriteLine(ex);
                Console.WriteLine(" ");
            }
        }

        static void ExtractZipFileToDirectory(string sourceZipFilePath, string destinationDirectoryName, bool overwrite)
        {
            using (var archive = ZipFile.Open(sourceZipFilePath, ZipArchiveMode.Read))
            {
                if (!overwrite)
                {
                    archive.ExtractToDirectory(destinationDirectoryName);
                    return;
                }

                DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
                string destinationDirectoryFullPath = di.FullName;

                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                    if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                    }

                    if (file.Name == "")
                    {// Assuming Empty for Directory
                        Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                        continue;
                    }
                    file.ExtractToFile(completeFileName, true);
                }
            }
        }
    }
}
