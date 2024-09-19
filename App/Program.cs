using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO.Compression;

#if WINDOWS
using Microsoft.Management.Infrastructure;
using Microsoft.Win32.TaskScheduler;
#endif

class Program
{
    /// <summary>
    /// Main method to download, extract, and optionally install Boot Camp ESDs for specified Mac models.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when 7-zip version could not be obtain.</exception>
    static async Task Main(string[] args)
    {
        // Model identifier to use, defaults to the current machine's model.
        string? model = GetCurrentModel();

        if (model is null)
        {
            throw new AccessViolationException("Mac Model identifier cloud not be obtain.");
        }
        
        // After the installer is downloaded, perform the install automatically if -Install flag is present.
        bool install = args.Contains("-Install");
        
        // Directory to extract installer files to. Defaults to the current directory.
        string outputDir = Directory.GetCurrentDirectory();
        
        // URL to download 7-Zip from, if not installed.
        // string sevenZipURL = "https://www.7-zip.org/a/7z1900-x64.exe";
        
        // URL for the software update catalog to use.
        string catalogURL = "https://swscan.apple.com/content/catalogs/others/index-10.15-10.14-10.13-10.12-10.11-10.10-10.9-mountainlion-lion-snowleopard-leopard.merged-1.sucatalog";

        // Ensure the output directory exists or create it.
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // // Check if 7-Zip is installed, and if not, download and install it.
        // string sevenZipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe");
        // if (!File.Exists(sevenZipPath) || !IsSevenZipVersionOk(sevenZipPath))
        // {
        //     Console.WriteLine("7-Zip not installed, downloading and installing...");
        //     string downloaded7z = await DownloadFile(sevenZipURL, Path.GetTempPath());
        //     InstallSevenZip(downloaded7z);
        // }

        Console.WriteLine($"Using model: {model}");

        // Fetch BootCamp ESDs by reading data from the software update catalog.
        var bootcampList = await FetchBootCampDrivers(catalogURL, model);

        if (bootcampList.Length != 0)
        {
            // Found a supported ESD, proceed with download and extraction.
            Console.WriteLine($"Found supported ESD: {bootcampList.First().Version}");
            string landingDir = Path.Combine(outputDir, $"BootCamp-{bootcampList.First().Version}");

            // Check if the destination folder already exists.
            if (Directory.Exists(landingDir))
            {
                Console.WriteLine($"Final destination folder {landingDir} already exists, please remove it to redownload.");
                return;
            }

            string workingDir = Path.Combine(Path.GetTempPath(), $"BootCamp-unpack-{bootcampList.First().Version}");
            if (!Directory.Exists(workingDir))
            {
                Directory.CreateDirectory(workingDir);
            }

            string packagePath = Path.Combine(workingDir, "BootCampESD.pkg");

            // Download the BootCamp ESD.
            await DownloadFile(bootcampList.First().URL, packagePath);

            // Extract the downloaded BootCamp package.
            Console.WriteLine("Extracting...");
            ZipFile.ExtractToDirectory(packagePath, workingDir);

            // ExtractWith7Zip(packagePath, workingDir);

            // Install BootCamp if the install flag is set.
#if WINDOWS
            if (install)
            {
                InstallBootcamp(landingDir);
            }
#endif

            // Clean up the working directory after extraction or installation.
            Console.WriteLine("Cleaning up working directory...");
            Directory.Delete(workingDir, true);
        }
        else
        {
            // No BootCamp ESD found for the specified model.
            Console.WriteLine("No Boot Camp ESD found for the specified model.");
        }
    }

    /// <summary>
    /// Gets the current machine's model.
    /// </summary>
    /// <returns>Model identifier string.</returns>
    static string? GetCurrentModel()
    {
        string? modelIdentifier = null;
#if WINDOWS
        // Get-ComputerInfo -Property
        // CsModel
        // Example implementation for getting system model on Windows using WMI equivalent.
        // model = new Microsoft.VisualBasic.Devices.ComputerInfo().OSFullName;
        modelIdentifier = GetCurrentModelForWindows();
#elif OSX
        modelIdentifier = GetCurrentModelForOSX();
#elif LINUX

        modelIdentifier = GetCurrentModelForLinux();
#endif
        return modelIdentifier;
    }

#if WINDOWS
    private static string? GetCurrentModelForWindows()
    {
        string? modelIdentifier = null;
        try
        {
            // using ステートメントでセッションを作成
            using var session = CimSession.Create(null)
            
            // 必要なプロパティだけを指定してクエリを実行 (Modelのみ)
            var instances = session.QueryInstances(@"root\cimv2", "WQL", "SELECT Model FROM Win32_ComputerSystem");

            // クエリ結果を列挙してModelプロパティを取得
            foreach (var instance in instances)
            {
                modelIdentifier = instance.CimInstanceProperties["Model"].Value;
                // Console.WriteLine($"Model: {instance.CimInstanceProperties["Model"].Value}");
            }
        }
        catch (CimException e)
        {
            // Console.WriteLine($"An error occurred: {e.Message}");
        }
        return modelIdentifier;
    }
#elif OSX
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
#elif LINUX
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

    /// <summary>
    /// Checks if the installed version of 7-Zip is 15.14 or newer.
    /// </summary>
    /// <param name="sevenZipPath">Path to the 7-Zip executable.</param>
    /// <exception cref="ArgumentNullException">Thrown when 7-zip version could not be obtain.</exception>
    /// <returns>True if the version is acceptable, false otherwise.</returns>
    static bool IsSevenZipVersionOk(string sevenZipPath)
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
    static async Task<string> DownloadFile(string url, string destination)
    {
        using HttpClient client = new();
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
    static void InstallSevenZip(string installerPath)
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
    static void ExtractWith7Zip(string source, string destination)
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
    static void InstallBootcamp(string landingDir)
    {
        using (TaskService ts = new TaskService())
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
    static async Task<(string Version, string URL)[]> FetchBootCampDrivers(string catalogUrl, string model)
    {
        using HttpClient client = new();
        var response = await client.GetStringAsync(catalogUrl);
        var xml = XDocument.Parse(response);

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
}
