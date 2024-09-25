﻿
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO.Compression;
using System.Runtime.InteropServices;

#if (WINDOWS || DOTNETTOOLS)
using Microsoft.Management.Infrastructure;
using Microsoft.Win32.TaskScheduler;
#endif

namespace Macaron.Core
{

    public class EDS
    {

        /// Gets the current machine's model.
        /// </summary>
        /// <returns>Model identifier string.</returns>
        public static string? GetCurrentModel()
        {
            string? modelIdentifier = null;
#if WINDOWS
            // Get-ComputerInfo -Property
            // CsModel
            // Example implementation for getting system model on Windows using WMI equivalent.
            // model = new Microsoft.VisualBasic.Devices.ComputerInfo().OSFullName;
            modelIdentifier = GetCurrentModelForWindows();
#elif OSX
    #if DEBUG
            Console.WriteLine("call GetCurrentModelForOSX");
    #endif
            modelIdentifier = GetCurrentModelForOSX();
#elif LINUX
            modelIdentifier = GetCurrentModelForLinux();
#elif IOS

#elif ANDROID

#elif DOTNETTOOLS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                modelIdentifier = GetCurrentModelForWindows();
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                modelIdentifier = GetCurrentModelForOSX();
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                modelIdentifier = GetCurrentModelForLinux();
            }
#endif
            return modelIdentifier;
        }

#if (WINDOWS || DOTNETTOOLS)
        private static string? GetCurrentModelForWindows()
        {
            string? modelIdentifier = null;
            try
            {
                // using ステートメントでセッションを作成
                using var session = CimSession.Create(null);
                
                // 必要なプロパティだけを指定してクエリを実行 (Modelのみ)
                var instances = session.QueryInstances(@"root\cimv2", "WQL", "SELECT Model FROM Win32_ComputerSystem");

                // クエリ結果を列挙してModelプロパティを取得
                foreach (var instance in instances)
                {
                    modelIdentifier = (string?) instance.CimInstanceProperties["Model"].Value;
                    // Console.WriteLine($"Model: {instance.CimInstanceProperties["Model"].Value}");
                }
            }
            catch (CimException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return modelIdentifier;
        }
#endif

#if (OSX || DOTNETTOOLS)
        private static string? GetCurrentModelForOSX()
        {
            string? modelIdentifier = null;
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"system_profiler SPHardwareDataType | grep 'Model Identifier'\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // プロセスを開始して結果を読み取る
                using var process = Process.Start(processInfo)
                    ?? throw new InvalidOperationException("Failed to start process.");

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // 出力結果から "Model Identifier: " 部分を削除
                modelIdentifier = output.Split(':')[1].Trim();

                Console.WriteLine($"Model Identifier: {modelIdentifier}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
            return modelIdentifier;
        }
#endif

#if (LINUX || DOTNETTOOLS)
        private static string? GetCurrentModelForLinux()
        {
            string? modelIdentifier = null;
            try
            {
                // ファイルパス
                string filePath = "/sys/class/dmi/id/product_name";

                // ファイルが存在するか確認して、内容を読み取る
                if (File.Exists(filePath))
                {
                    modelIdentifier = File.ReadAllText(filePath).Trim();
                    Console.WriteLine($"Model Identifier: {modelIdentifier}");
                }
                else
                {
                    Console.WriteLine("Model identifier file not found.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
            return modelIdentifier;
        }
#endif

#if IOS

#endif

#if ANDROID

#endif

        /// <summary>
        /// Checks if the installed version of 7-Zip is 15.14 or newer.
        /// </summary>
        /// <param name="sevenZipPath">Path to the 7-Zip executable.</param>
        /// <exception cref="ArgumentNullException">Thrown when 7-zip version could not be obtain.</exception>
        /// <returns>True if the version is acceptable, false otherwise.</returns>
        public static bool IsSevenZipVersionOk(string sevenZipPath)
        {
            var fileVersion = FileVersionInfo.GetVersionInfo(sevenZipPath).FileVersion
                ?? throw new ArgumentNullException("7-zip version could not be obtain.");
            
            return new Version(fileVersion) >= new Version(15, 14);
        }

        /// <summary>
        /// Downloads a file from a URL to the specified destination.
        /// </summary>
        /// <param name="url">The URL to download from.</param>
        /// <param name="destination">The destination folder for the downloaded file.</param>
        /// <returns>The path to the downloaded file.</returns>
        public static async Task<string> DownloadFile(string url, string destination)
        {
            using var client = new HttpClient();
            string filePath = Path.Combine(destination, Path.GetFileName(url));
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var fs = new FileStream(filePath, FileMode.Create);
            await response.Content.CopyToAsync(fs);
            return filePath;
        }

        /// <summary>
        /// Installs 7-Zip by running its installer.
        /// </summary>
        /// <param name="installerPath">Path to the 7-Zip installer file.</param>
        /// <exception cref="InvalidOperationException">Thrown when the process cannot be started. This may occur if the installer path is invalid or if the necessary permissions are not granted.</exception>
        public static void InstallSevenZip(string installerPath)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                Arguments = $"/i {installerPath} /qb- /norestart",
                UseShellExecute = true,
                Verb = "runas" // Ensures the process runs with elevated privileges.
            }) ?? throw new InvalidOperationException("Failed to start the process. Please check the installer path and permissions.");
            process.WaitForExit();
        }

        /// <summary>
        /// Extracts a package using 7-Zip.
        /// </summary>
        /// <param name="source">The source file to extract.</param>
        /// <param name="destination">The destination directory to extract to.</param>
        /// <exception cref="InvalidOperationException">Thrown when the process cannot be started. This may occur if the source path is invalid or if 7-Zip is not installed.</exception>
        public static void ExtractWith7Zip(string source, string destination)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = @"C:\Program Files\7-Zip\7z.exe",
                Arguments = $"x {source} -o{destination} -y",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }) ?? throw new InvalidOperationException("Failed to start the process. Please check the source path and ensure that 7-Zip is installed.");

            process.WaitForExit();
        }

#if WINDOWS
        /// <summary>
        /// Schedules a task to install BootCamp using Task Scheduler.
        /// </summary>
        /// <param name="landingDir">Directory where BootCamp files are located.</param>
        public static void InstallBootcamp(string landingDir)
        {
            using (var ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Install Bootcamp Drivers";

                td.Principal.LogonType = TaskLogonType.ServiceAccount;
                td.Principal.UserId = "SYSTEM";

                // Schedule the task to run once, 15 seconds after creation.
                td.Triggers.Add(new TimeTrigger(DateTime.Now.AddSeconds(15)));

                // Set the action to run the BootCamp MSI installer silently.
                td.Actions.Add(new ExecAction("msiexec.exe", $"/i {Path.Combine(landingDir, "Bootcamp\\Drivers\\Apple\\BootCamp.msi")} /qn /norestart"));

                // Register the task.
                ts.RootFolder.RegisterTaskDefinition("InstallBootcamp", td);
            }

            Console.WriteLine("Scheduled Bootcamp installation task.");
        }
#endif

        /// <summary>
        /// Fetches BootCamp drivers from the software update catalog.
        /// </summary>
        /// <param name="catalogUrl">URL of the software update catalog.</param>
        /// <param name="model">The model identifier to look for.</param>
        /// <returns>An array of BootCamp drivers with version and URL.</returns>
        public static async Task<(string Version, string URL)[]> FetchBootCampDrivers(string catalogUrl, string model)
        {
    #if DEBUG
            Console.WriteLine("call FetchBootCampDrivers");
    #endif
            using var client = new HttpClient();

            // var response = await client.GetStringAsync(catalogUrl);

            // var destinationPath = "C:\\Users\\katsutoshi\\src\\Macaron\\file.txt";
            var destinationPath = "/Users/katsutoshi/src/Macaron/file.txt";

            // await DownloadCatalog(client, catalogUrl, destinationPath);
            // try
            // {
            //     System.Threading.Tasks.Task.Run(async () =>
            //     {
            //     }).Wait();
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine($"例外が発生しました: {ex.Message}");
            // }
            await DownloadCatalog(client, catalogUrl, destinationPath);

#if DEBUG
            Console.WriteLine("call FetchBootCampDrivers");
    #endif
            // var xml = XDocument.Parse(response);

            XDocument xml = XDocument.Load(destinationPath);

            // Parse the XML to find BootCamp ESDs that support the specified model.
            var bootcampESDs = from dict in xml.Descendants("dict")
                            let version = dict.Element("Version")?.Value
                            let url = dict.Element("URL")?.Value
                            let supportedModels = dict.Elements("SupportedModels")
                                                        .Where(x => x.Value.Contains(model))
                            where supportedModels.Any()
                            select (Version: version, URL: url);

            return bootcampESDs.ToArray();
        }

        // dotnet8.0のHTTPClientのバグが治るまではこれ。
        private static System.Threading.Tasks.Task DownloadCatalogCurl(string fileUrl,  string destinationPath)
        {
    #if DEBUG
            Console.WriteLine("call DownloadCatalogCurl");
    #endif
            return System.Threading.Tasks.Task.Run(() =>{
                try
                {
    #if WINDOWS
    // windowsはwget実装よりもcurl.exeの方が実装が早かったためcurl
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Windows\System32\curl.exe",
                        Arguments = $"-o {destinationPath} {fileUrl}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
    #elif OSX
    #if DEBUG
            Console.WriteLine("call Macos curl");
    #endif
                    var processInfo = new ProcessStartInfo
                    {
    // macはcurlをデフォルトインストール。
                        FileName = "/usr/bin/curl",
                        Arguments = $"-o {destinationPath} {fileUrl}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
    #elif LINUX
    #if DEBUG
            Console.WriteLine("call Macos linux");
    #endif
                    // Linuxはディストリビューションによる。コマンドの存在確認以後にやる。
                    string commandPath = "/usr/bin/wget";


                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/wget",
                        Arguments = $"-O {destinationPath} {fileUrl}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    if (File.Exists(commandPath))
                    {
                        processInfo = new ProcessStartInfo
                        {
                            FileName = "/usr/bin/curl",
                            Arguments = $"-o {destinationPath} {fileUrl}",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                    }
    #elif DOTNETTOOLS
                    // defaultはwindowsにする。
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Windows\System32\curl.exe",
                        Arguments = $"-o {destinationPath} {fileUrl}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        processInfo = new ProcessStartInfo
                        {
        // macはcurlをデフォルトインストール。
                            FileName = "/usr/bin/curl",
                            Arguments = $"-o {destinationPath} {fileUrl}",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                    } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        string commandPath = "/usr/bin/wget";

                        processInfo = new ProcessStartInfo
                        {
                            FileName = "/usr/bin/wget",
                            Arguments = $"-O {destinationPath} {fileUrl}",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        if (File.Exists(commandPath))
                        {
                            processInfo = new ProcessStartInfo
                            {
                                FileName = "/usr/bin/curl",
                                Arguments = $"-o {destinationPath} {fileUrl}",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                        }
                    }
    #else 
    // Androidなど
    #endif
                    // プロセスを開始して結果を読み取る
                    using var process = Process.Start(processInfo)
                        ?? throw new InvalidOperationException("Failed to start process.");

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();


                    Console.WriteLine(output);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred: {e.Message}");
                }
            });
        }

        private static async System.Threading.Tasks.Task DownloadCatalogHttpClient(HttpClient client, string fileUrl, string destinationPath)
        {
            try
            {
                // 応答メッセージを使って詳細なエラーチェックを行う
                using (HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode(); // 応答が成功しているか確認
                    Console.WriteLine("ファイルのダウンロードが開始されました。");

                    // ストリームでファイルを書き込み
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                   fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }

                    Console.WriteLine("ファイルが正常にダウンロードされ、保存されました。");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HTTPリクエストエラー: {e.Message}");
                if (e.InnerException != null)
                {
                    Console.WriteLine($"詳細なエラー情報: {e.InnerException.Message}");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine($"ファイル書き込みエラー: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"予期しないエラー: {ex.Message}");
            }
            //try
            //{
            //    // ファイルを一度にダウンロードしてローカルに保存
            //    byte[] fileBytes = await client.GetByteArrayAsync(fileUrl);
            //    await File.WriteAllBytesAsync(destinationPath, fileBytes);
                
            //    #if DEBUG
            //    Console.WriteLine("ファイルが正常にダウンロードされました。");
            //    #endif
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"エラーが発生しました: {ex.Message}");
            //}
        }
        internal static async System.Threading.Tasks.Task DownloadCatalog(HttpClient client, string fileUrl, string destinationPath)
        {
            // dotnet8.0 has httpclient bug https://github.com/dotnet/runtime/issues/97966.
// #if NET8.0
            // await DownloadCatalogHttpClient(client, fileUrl, destinationPath);
// #endif
            await DownloadCatalogCurl(fileUrl, destinationPath);
        }
    //     /// <summary>
    //     /// Fetches BootCamp drivers from the software update catalog.
    //     /// </summary>
    //     /// <param name="catalogUrl">URL of the software update catalog.</param>
    //     /// <param name="model">The model identifier to look for.</param>
    //     /// <returns>An array of BootCamp drivers with version and URL.</returns>
    //     public static async Task<(string Version, string URL)[]> FetchBootCampDrivers(string catalogUrl, string model)
    //     {
    // #if DEBUG
    //         Console.WriteLine("call FetchBootCampDrivers");
    // #endif
    //         using var client = new HttpClient();
    //         DownloadFileAsync

    //         var response = await client.GetStringAsync(catalogUrl);

    //         var xml = XDocument.Parse(response);

    //         // Parse the XML to find BootCamp ESDs that support the specified model.
    //         var bootcampESDs = from dict in xml.Descendants("dict")
    //                         let version = dict.Element("Version")?.Value
    //                         let url = dict.Element("URL")?.Value
    //                         let supportedModels = dict.Elements("SupportedModels")
    //                                                     .Where(x => x.Value.Contains(model))
    //                         where supportedModels.Any()
    //                         select (Version: version, URL: url);

    //         return bootcampESDs.ToArray();
    //     }
    }
}
