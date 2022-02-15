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



void InitialIntro()
{
    Console.WriteLine("------------------------------------------------");
    Console.WriteLine();
    Console.WriteLine("This is a really basic program to help create sql script and import data. This was done quickly, and has not been tested exensively. ");
    Console.WriteLine("It would be good to get better type parsing. In particular, I'm unhappy with float/decimal/money option. Although I also don't look at varchar vs nvarchar. I don't look at tinyint.");
    Console.WriteLine("There are parts of the script that I've made ugly changes as the flat files that I'm working on have are just nasty.");
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------");

}



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
    Console.WriteLine("[4] Get the SQL script to generate tables. This just generates the script, and does not alter the db.");
    Console.WriteLine("[5] Run table schema script. This alters the db, so be careful.");
    Console.WriteLine("[6] Generate script to populate table data. This just generates the script, and does not alter the db.");
    Console.WriteLine("[7] Populate table data. This alters the db, so be careful.");
    Console.WriteLine("[8] Create the schema and populate the data (5+7). This alters the db, so be careful.");
    Console.WriteLine("Press enter a couple of times to get out of here.");
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
        case ('4'):
            Option4();
            break;
        case ('5'):
            Option5();
            break;
        case ('6'):
            Option6();
            break;
        case ('7'):
            Option7();
            break;
        case ('8'):
            Option8();
            break;
        default:
            Bye();
            break;
    }
}


void Option1()
{
    Console.Clear();
    Console.WriteLine($"The folder path is currently set to: >> {config.FolderPath} <<.");
    if (!string.IsNullOrWhiteSpace(config.FileFilter)) Console.WriteLine($"There is a filter of: >>  {config.FileFilter} <<.");
    if (!string.IsNullOrWhiteSpace(config.FileNameCleanerRegex)) Console.WriteLine($"There is a file cleaner regex of : >>  {config.FileNameCleanerRegex} <<.");
    ClickToContinue();
}

void Option2()
{
    Console.Clear();
    var files = fs.GetFiles(config.FolderPath);
    Console.WriteLine($"The folder path is currently set to: >> {config.FolderPath} <<.");
    if (!string.IsNullOrWhiteSpace(config.FileFilter)) Console.WriteLine($"There is a filter of: >>  {config.FileFilter} <<.");
    if (!string.IsNullOrWhiteSpace(config.FileNameCleanerRegex)) Console.WriteLine($"There is a file cleaner regex of : >>  {config.FileNameCleanerRegex} <<.");

    Console.Write(files.ToStringTable(new[] { "File name,", "Extension", "Size", "Table name" }, f => f.Name, f=> f.Extension, f=>f.Length, f=>fs.GetTableName(f.Name)));
 
    ClickToContinue();
}

void Option3()
{
    Console.Clear();
    //Console.BackgroundColor = ConsoleColor.Cyan;
    //Console.ForegroundColor = ConsoleColor.Black;
    Console.WriteLine($"The connection string is set to: >> {config.DefaultConnectionString} <<.");
    //Console.ResetColor();
    ClickToContinue();
}

void Option4()
{
    Console.Clear();
    var files = fs.GetFiles(config.FolderPath);
    foreach (var file in files)
    {
        WriteSeparater();
        var fn = file.Name;
        var tn = fs.GetTableName(fn);
        var cols = fs.GetColumsMetaData(file.FullName, config.SampleSize);
        if (config.SampleSize>0)
        {
            Console.WriteLine($"The sample size is configured to review {config.SampleSize} rows");
        }
        Console.WriteLine($"Filename: { fn }");
        Console.WriteLine($"Tablename: { tn }");

        Console.WriteLine($"Table schema: { ds.CreateEmptyTableText(tn, cols) }");
        Console.WriteLine( ds.CreateEmptyTableText(tn, cols) );
    }
    ClickToContinue();
}


void Option5()
{
    Console.Clear();
    var files = fs.GetFiles(config.FolderPath);
    foreach (var file in files)
    {
        var fn = file.Name;
        var tn = fs.GetTableName(fn);
        var cols = fs.GetColumsMetaData(file.FullName, config.SampleSize);
        WriteSeparater();
        Console.WriteLine($"Filename: { fn }");
        Console.WriteLine($"Tablename: { tn }");

        ds.CreateEmptyTable(tn, cols);

        Console.WriteLine("Finished creating table.");
    }
    WriteSeparater();
    Console.WriteLine($"Finished creating { files.Count() } tables. The data has not been populated for the tables. ");

    ClickToContinue();
}

void Option6()
{
    Console.Clear();
    var files = fs.GetFiles(config.FolderPath);
    foreach (var file in files)
    {
        var fn = file.Name;
        var tn = fs.GetTableName(fn);
        
        var script = ds.PopulateTableString(tn, file.FullName);
        WriteSeparater();
        Console.WriteLine($"Bulk insert script for { tn }.");
        Console.WriteLine(script);

    }
    ClickToContinue();
}


void Option7()
{
    Console.Clear();
    Console.WriteLine("Populating data... ");
    var files = fs.GetFiles(config.FolderPath);
    foreach (var file in files)
    {
        WriteSeparater();
        var fn = file.Name;
        var tn = fs.GetTableName(fn);
        
        ds.PopulateTable(tn, file.FullName);

        Console.WriteLine($"Bulk insert for { tn } complete.");
    }
    WriteSeparater();

    Console.WriteLine("All tables have been populated.");
    ClickToContinue();
}


void Option8()
{
    Console.Clear();
    Console.WriteLine("Creating tables and Populating data... ");
    var files = fs.GetFiles(config.FolderPath);
    foreach (var file in files)
    {
        WriteSeparater();
        var fn = file.Name;
        var tn = fs.GetTableName(fn);
        var cols = fs.GetColumsMetaData(file.FullName, config.SampleSize);

        ds.CreateEmptyTable(tn, cols);
        ds.PopulateTable(tn, file.FullName);

        Console.WriteLine($"Bulk insert for { tn } complete.");
    }

    Console.WriteLine("All tables have been created and  populated. Please carefully check everything now");
    ClickToContinue();
}

void WriteSeparater()
{
    Console.WriteLine();
    Console.WriteLine("...");
    Console.WriteLine();
}

void ClickToContinue()
{
    WriteSeparater();
    Console.WriteLine("Press enter to return to the menu.");
    Console.ReadLine();
    Console.Clear();
    PickAnOption();
}

void Bye()
{
    Console.WriteLine("Bye...");
    Thread.Sleep(2000);
    Console.WriteLine("");
}

InitialIntro();
PickAnOption();

