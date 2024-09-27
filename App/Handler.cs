using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.CommandLine;
using System.Linq;
using Macaron.Core;
using System.Runtime.InteropServices;
using System.Net.Http;

partial class Program
{
    public static void DownloadCatalogHandler(string outputDir, string catalogUrl)
    {

#if DEBUG
        Console.WriteLine("call DownloadCatalogHandler");
#endif
        using var client = new HttpClient();

        // change absolutely path.
        var fullOutputdir = Path.IsPathFullyQualified(outputDir) 
            ? outputDir
            : Path.GetFullPath(outputDir);

#if DEBUG
        Console.WriteLine("call if BootCamp");
#endif
        if (!Directory.Exists(fullOutputdir))
        {
            Directory.CreateDirectory(fullOutputdir);
        }
        EDS.DownloadCatalog(client, catalogUrl, Path.Join(outputDir, "abc.plist"));
    }

    public static async void DownloadHandler(string model, string outputDir)
    {

    #if DEBUG
            Console.WriteLine("call DownloadHandler");
    #endif
        // Fetch BootCamp ESDs by reading data from the software update catalog.
        // var bootcampList = await EDS.FetchBootCampDrivers(catalogURL, model);
        var bootcampList = EDS.FetchBootCampDrivers(catalogUrl, model);

        // change absolutely path.
        var fullOutputdir = Path.IsPathFullyQualified(outputDir) 
            ? outputDir
            : Path.GetFullPath(outputDir);

    #if DEBUG
            Console.WriteLine("call if BootCamp");
    #endif
        if (!Directory.Exists(fullOutputdir))
        {
            Directory.CreateDirectory(fullOutputdir);
        }

        if (bootcampList.Length == 0)
        {
            // No BootCamp ESD found for the specified model.
            Console.WriteLine("No Boot Camp ESD found for the specified model.");
            return;
        }

    #if DEBUG
            Console.WriteLine("call if BootCamp");
    #endif
        // Found a supported ESD, proceed with download and extraction.
        Console.WriteLine($"Found supported ESD: {bootcampList[0].Version}");
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
        await EDS.DownloadFile(bootcampList.First().URL, packagePath);

        // Extract the downloaded BootCamp package.
        Console.WriteLine("Extracting...");
        ZipFile.ExtractToDirectory(packagePath, workingDir);

        // ExtractWith7Zip(packagePath, workingDir);

        // Install BootCamp if the install flag is set.
// #if WINDOWS
//             if (install)
//             {
//                 EDS.InstallBootcamp(landingDir);
//             }
// #endif

        // Clean up the working directory after extraction or installation.
        Console.WriteLine("Cleaning up working directory...");
        Directory.Delete(workingDir, true);
        // await EDS.DownloadFile(catalogURLから撮ったやつ。, fullOutputdir);
    }

    static void GetCurrentModelHandler()
    {
        var model = EDS.GetCurrentModel();

        if (model is null)
        {
            Console.WriteLine("current model is unkown.");
        }
        // if --local
        Console.WriteLine($"current model is {model}.");
    }

    static void ShowVersionHandler()
    {
        Console.WriteLine($"macaron-cli version {InformationVersionAttribute}");
    }

    /// <summary>
    /// Main method to download, extract, and optionally install Boot Camp ESDs for specified Mac models.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when 7-zip version could not be obtain.</exception>
    static async System.Threading.Tasks.Task Run(string[] args)
    {
        // Run(args);

        // Model identifier to use, defaults to the current machine's model.
        string? model = EDS.GetCurrentModel();

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
        // string catalogURL = "https://swscan.apple.com/content/catalogs/others/index-10.15-10.14-10.13-10.12-10.11-10.10-10.9-mountainlion-lion-snowleopard-leopard.merged-1.sucatalog";

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
        // var bootcampList = await EDS.FetchBootCampDrivers(catalogURL, model);
        var bootcampList = EDS.FetchBootCampDrivers(catalogUrl, model);

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
            await EDS.DownloadFile(bootcampList.First().URL, packagePath);

            // Extract the downloaded BootCamp package.
            Console.WriteLine("Extracting...");
            ZipFile.ExtractToDirectory(packagePath, workingDir);

            // ExtractWith7Zip(packagePath, workingDir);

            // Install BootCamp if the install flag is set.
#if WINDOWS
            if (install)
            {
                EDS.InstallBootcamp(landingDir);
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
    /// Main method to download, extract, and optionally install Boot Camp ESDs for specified Mac models.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when 7-zip version could not be obtain.</exception>
    static async System.Threading.Tasks.Task Runas(string[] args)
    {
        // Run(args);

        // Model identifier to use, defaults to the current machine's model.
        string? model = EDS.GetCurrentModel();

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
        // string catalogURL = "https://swscan.apple.com/content/catalogs/others/index-10.15-10.14-10.13-10.12-10.11-10.10-10.9-mountainlion-lion-snowleopard-leopard.merged-1.sucatalog";

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
        // var bootcampList = await EDS.FetchBootCampDrivers(catalogURL, model);
        var bootcampList = EDS.FetchBootCampDrivers(catalogUrl, model);

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
            await EDS.DownloadFile(bootcampList.First().URL, packagePath);

            // Extract the downloaded BootCamp package.
            Console.WriteLine("Extracting...");
            ZipFile.ExtractToDirectory(packagePath, workingDir);

            // ExtractWith7Zip(packagePath, workingDir);

            // Install BootCamp if the install flag is set.
#if WINDOWS
            if (install)
            {
                EDS.InstallBootcamp(landingDir);
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
}