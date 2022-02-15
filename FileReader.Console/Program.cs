using FileReader.Console;
using FileReader.Core.Interfaces;
using FileReader.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

IConfiguration baseConfig = new ConfigurationBuilder()
                        .AddJsonFile("appSettings.json")
                        .AddEnvironmentVariables()
                        .Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services
            .AddTransient<IConfigValues, ConfigValues>()
            .AddTransient<IFileService, FileService>()
            .AddTransient<IDataService, DataService>()
            )
    .ConfigureLogging((_, logging) =>
            {
                logging.ClearProviders();
                logging.AddEventLog();
            })
    .Build();


IConfigValues config = host.Services.GetRequiredService<IConfigValues>();
IFileService fs = host.Services.GetRequiredService<IFileService>();
IDataService ds = host.Services.GetRequiredService<IDataService>();


void PickAnOption()
{
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------");
    Console.WriteLine("What would you like to do (pick and option...)");
    Console.WriteLine("[1] Check the configured path location.");
    Console.WriteLine("[2] See files in your configured path.");
    Console.WriteLine("[3] Check connection string.");
    Console.WriteLine(">>");
    var option = Console.ReadKey();
    switch (option.KeyChar)
    {
        case ('1'):
            Option1();
            break;
        case ('2'):
            Option2();
            break;
        case ('3'):
            Option3();
            break;
        default:
            Bye();
            break;
    }
}


void Option1()
{
    Console.WriteLine("");
    Console.WriteLine($"The folder path is currently set to: >> {config.FolderPath} <<.");
    if (!string.IsNullOrWhiteSpace(config.FileFilter)) Console.WriteLine($"There is a filter of: >>  {config.FileFilter} <<.");
    if (!string.IsNullOrWhiteSpace(config.FileNameCleanerRegex)) Console.WriteLine($"There is a file cleaner regex of : >>  {config.FileNameCleanerRegex} <<.");
    PickAnOption();
}

void Option2()
{
    Console.WriteLine("");
    var files = fs.GetFiles(config.FolderPath);
    Console.WriteLine($"The folder path is currently set to: >> {config.FolderPath} <<.");
    if (!string.IsNullOrWhiteSpace(config.FileFilter)) Console.WriteLine($"There is a filter of: >>  {config.FileFilter} <<.");
    if (!string.IsNullOrWhiteSpace(config.FileNameCleanerRegex)) Console.WriteLine($"There is a file cleaner regex of : >>  {config.FileNameCleanerRegex} <<.");

    Console.Write(files.ToStringTable(new[] { "File name,", "Extension", "Size", "Table name" }, f => f.Name, f=> f.Extension, f=>f.Length, f=>fs.GetTableName(f.Name)));
    
    PickAnOption();
}

void Option3()
{
    Console.WriteLine("");
    //Console.BackgroundColor = ConsoleColor.Cyan;
    //Console.ForegroundColor = ConsoleColor.Black;
    Console.WriteLine($"The connection string is set to: >> {config.DefaultConnectionString} <<.");
    //Console.ResetColor();
    PickAnOption();
}

void Bye()
{
    Console.WriteLine("Bye...");
    Thread.Sleep(2000);
    Console.WriteLine("");
}

PickAnOption();


//var folderPath = config.GetValue<string>("FolderPath");
//Console.WriteLine($"The folder path is currently set to: {folderPath}");
//Console.WriteLine("If this is correct, please type [y] to confirm or [n] to cancel.");
//var nextKey = Console.ReadKey();
//if (nextKey.KeyChar != 'y') Environment.Exit(0);
//var constr = config.GetConnectionString("Default");
//Console.WriteLine("");
//Console.WriteLine($"The database is set to:");
//Console.WriteLine(constr);
//Console.WriteLine("");
//Console.WriteLine("If this is correct, please type [y] to confirm or [n] to cancel.");
//nextKey = Console.ReadKey();
//if (nextKey.KeyChar != 'y') Environment.Exit(0);
//var fs = new FileService(config);
//var files = fs.GetFiles(folderPath);
//Console.WriteLine("");
//Console.WriteLine("The following files will be processed. ");

//foreach (var file in files)
//{
//    Console.WriteLine(file);
//}

//Console.WriteLine("If this is correct, please type [y] to confirm or [n] to cancel.");
//nextKey = Console.ReadKey();
//if (nextKey.KeyChar != 'y') Environment.Exit(0);

//Console.WriteLine("");
//Console.WriteLine("checking db... ");
//var ds = new DataService(config);

//foreach (var file in files)
//{
//    var tableName = Path.GetFileNameWithoutExtension(file.Name);
//    Console.WriteLine($"Creating table ... {tableName}");
//    ds.CreateEmptyTable(tableName, fs.GetColumsMetaData(file.FullName, config.GetValue<int>("SampleSize")));

//    Console.WriteLine($"Populating table ... {tableName}");
//    ds.PopulateTable(tableName, file.FullName);
//}

