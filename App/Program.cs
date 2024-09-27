using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.CommandLine;
using System.Linq;
using Macaron.Core;
using System.Runtime.InteropServices;

partial class Program
{
    const string catalogUrl = "https://swscan.apple.com/content/catalogs/others/index-10.15-10.14-10.13-10.12-10.11-10.10-10.9-mountainlion-lion-snowleopard-leopard.merged-1.sucatalog";
    public static AssemblyInformationalVersionAttribute InformationVersionAttribute
    {
        get {

            var assembly = Assembly.GetExecutingAssembly() ??  throw new InvalidOperationException("Could not retrieve the current executing assembly.");

            var versionAttributeType = typeof(AssemblyInformationalVersionAttribute) ?? throw new InvalidOperationException("Could not retrieve the current executing assembly.");

            var informationalVersionAttribute = (AssemblyInformationalVersionAttribute) (Attribute.GetCustomAttribute(
                    assembly,
                    versionAttributeType
                ) ?? throw new InvalidOperationException("Could not retrieve the current assembly attribute."));

            return informationalVersionAttribute;
        }
    }

    static async System.Threading.Tasks.Task<int> Main(string[] args)
    {
        // コマンドラインオプションの定義

        var modelOption = new Option<string>(
            new[] { "-m", "--model" },
            "System model identifier to use (otherwise this machine's model is used). This can be specified multiple times to download multiple models in a single run."
        );
        var getCurrentModelOption = new Option<bool>(
            new[] { "--get-currentmodel" },
            "System model identifier to use (otherwise this machine's model is used)."
        );
        var downloadCatalogOption = new Option<bool>(
            new[] { "--download-catalog" },
            "download mac eds package list."
        );

        // 自動化したければ、windows imageから実行が王道。silent installもそこからできる。
        var installOption = new Option<bool>(
            new[] { "-i", "--install" },
            "After the installer is downloaded, perform the install automatically. Can be used on Windows only."
        );

        var downloadOption = new Option<bool>(
            new[] {"--download-eds" },
            "download EDS package only."
        );

        var catalogUrlOption = new Option<string>(
            new[] { "-c", "--catalog-Url" },
            () => catalogUrl, // デフォルトはapple official site.
            "Catalog Download Url."
        );

        var outputDirOption = new Option<string>(
            new[] { "-o", "--output-dir" },
            () => Directory.GetCurrentDirectory(), // デフォルトはカレントディレクトリ
            "Base path where the installer files will be extracted into a folder named after the product, ie. 'BootCamp-041-1234'. Uses the current directory if this option is omitted."
        );

        var keepFilesOption = new Option<bool>(
            new[] { "-k", "--keep-files" },
            "Keep the files that were downloaded/extracted. Useful only with the '--install' option on Windows."
        );

        var productIdOption = new Option<string>(
            new[] { "-p", "--product-id" },
            "Specify an exact product ID to download (ie. '031-0787'), currently useful only for cases where a model has multiple BootCamp ESDs available and is not downloading the desired version according to the post date."
        );

        // var versionOption = new Option<bool>(
        //     new[] { "-V", "--version" },
        //     "Output the version of the application."
        // );

        // ルートコマンドの定義
        var rootCommand = new RootCommand
        {
            modelOption,
            getCurrentModelOption,
            downloadCatalogOption,
            catalogUrlOption,
            installOption,
            downloadOption,
            outputDirOption,
            // keepFilesOption,
            // productIdOption,
            // versionOption
        };

        var platform = "dotnet tools";

#if !DOTNETTOOLS
        platform = $"{RuntimeInformation.OSDescription} ({RuntimeInformation.ProcessArchitecture})";
#endif

        rootCommand.Description = $"BootCamp installer downloader and extractor for {platform}.";

        // コマンドが実行されたときの処理を設定
        // ここでパースされた引数を受け取る。
        rootCommand.SetHandler(
            // (string model, bool getCurrentModel, bool downloadCataglog, string catalogUrl,bool install, bool download, string outputDir, bool keepFiles, string productId)
            (string model, bool getCurrentModel, bool downloadCataglog, string catalogUrl,bool install, bool download, string outputDir)
            =>
        {
            if (getCurrentModel)
            {
                GetCurrentModelHandler();
            } else if (downloadCataglog)
            {
                DownloadCatalogHandler(outputDir, catalogUrl);
            } else if (download)
            {
    #if DEBUG
            Console.WriteLine("call DownloadHandler");
    #endif
                DownloadHandler(model, outputDir);
            }
            // MainProjectのロジックを呼び出す
            // Run(model, outputDir, install, keepFiles, productId);
        // }, modelOption, getCurrentModelOption, downloadCatalogOption, catalogUrlOption, installOption, downloadOption,  outputDirOption, keepFilesOption, productIdOption);
        }, modelOption, getCurrentModelOption, downloadCatalogOption, catalogUrlOption, installOption, downloadOption,  outputDirOption);

        // コマンドの実行
        return await rootCommand.InvokeAsync(args);
    }

}
