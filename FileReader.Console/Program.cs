using FileReader.Console;
using FileReader.Core.Interfaces;
using FileReader.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

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
            .AddTransient<ITypeProcessingService, TypeProcessingService>()
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

//int mainMenuSelectedIndex = -1;
//int profileMenuSelectedIndex = -1;
int selectedMenuIndex = 0;
bool initialLoad = true;

List<Option> mainOptions = new List<Option>();
List<Option> profileOptions = new List<Option>();

mainOptions.Add(new Option(0, "[0] Create and use profile settings", Option0 ));
mainOptions.Add(new Option(1, "[1] Check the configured path location", Option1));
mainOptions.Add(new Option(2, "[2] See files in your configured path.", Option2));
mainOptions.Add(new Option(3, "[3] Check connection string.", Option3));
mainOptions.Add(new Option(4, "[4] Get the SQL script to generate tables. This just generates the script, and does not alter the db.", Option4));
mainOptions.Add(new Option(5, "[5] Run table schema script. This alters the db, so be careful.", Option5) );
mainOptions.Add(new Option(6, "[6] Generate script to populate table data. This just generates the script, and does not alter the db.", Option6));
mainOptions.Add(new Option(7, "[7] Populate table data. This alters the db, so be careful.", Option7));
mainOptions.Add(new Option(8, "[8] Create the schema and populate the data (5+7). This alters the db, so be careful.", Option8));
mainOptions.Add(new Option(9, "[9] Quit", Bye));

profileOptions.Add(new Option(0, "[0] Back to main menu", LoadMainMenu));
profileOptions.Add(new Option(1, "[1] Create new Profile", CreateProfile));


void InitialIntro()
{
    if (initialLoad)
    {
        Console.WriteLine("------------------------------------------------");
        Console.WriteLine();
        Console.WriteLine("This is a really basic program to help create sql script and import data. This was done quickly, and has not been tested exensively. ");
        Console.WriteLine("It would be good to get better type parsing. In particular, I'm unhappy with float/decimal/money option. Although I also don't look at varchar vs nvarchar. I don't look at tinyint.");
        Console.WriteLine("There are parts of the script that I've made ugly changes as the flat files that I'm working on have are just nasty.");
        Console.WriteLine();
        Console.WriteLine("------------------------------------------------");
    }
    else
    {
        Console.WriteLine("------------------------------------------------");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"loaded profile: {config.ProfileName}");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("------------------------------------------------");
    }

}

void CreateProfile()
{
    Console.Clear();
    InitialIntro();
    Console.WriteLine("What would you like to call the new profile?");
    var newProfileName = Console.ReadLine();
    if (string.IsNullOrEmpty(newProfileName))
    {
        Console.WriteLine("You need to enter the name for a new profile. try again.... ");
        Thread.Sleep(2000);
        CreateProfile();
    }
    else
    {
        config.CopySettingsProfileTo(newProfileName);
        Console.WriteLine("new profile has been created with a copy of the current settings");
        // clear current profile options otherwise these will duplication each time this is run.
        WriteProfileMenu();
    }
}

void WriteProfileMenu()
{
    profileOptions.RemoveAll(e => e.Id >1);
    var profiles = config.GetProfiles();
    for (var i = 2; i < profiles.Count()+2; i++)
    {
        string profileName = profiles[i-2];
        profileOptions.Add(new Option(i, $"[{i}] load profile {profileName}", () => config.LoadSettingsProfile(name: profileName)));
    }
    WriteMenu(profileOptions, profileOptions[selectedMenuIndex]);
    GetMainInteraction(profileOptions);
}

void Option0()
{
    selectedMenuIndex = 0;
    WriteProfileMenu();
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

    if (files.Count() == 0)
    {
        Console.WriteLine("There are no files in this path that match the filters");
    }
    else
    { 
        Console.Write(files.ToStringTable(new[] { "File name,", "Extension", "Size", "Table name" }, f => f.Name, f=> f.Extension, f=>f.Length, f=>fs.GetTableName(f.Name)));
    }

    ClickToContinue();
}

void Option3()
{
    Console.Clear();
    Console.WriteLine($"The connection string is set to: >>");
    Console.WriteLine($"{config.DefaultConnectionString}");
    ClickToContinue();
}

void Option4()
{
    Console.Clear();
    StringBuilder sb = new StringBuilder();
    var files = fs.GetFiles(config.FolderPath);
    foreach (var file in files)
    {
        WriteSeparater();
        var fn = file.Name;
        var tn = fs.GetTableName(fn);
        var cols = fs.GetColumsMetaData(file.FullName, config.SampleSize);
        if (config.SampleSize>0)
        {
            Console.WriteLine($"The sample size is configured to review >> {config.SampleSize} rows");
        }
        Console.WriteLine($"Filename >> { fn }");
        Console.WriteLine($"Tablename >> { tn }");

        Console.WriteLine("Table schema >>");
        var outputText = ds.CreateEmptyTableText(tn, cols);
        sb.AppendLine(outputText);
        Console.WriteLine( outputText);
    }
    SetText(sb.ToString());
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
        Console.WriteLine($"Filename>> { fn }");
        Console.WriteLine($"Tablename>> { tn }");

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
        Console.WriteLine("...");
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
    WriteMenu(mainOptions, mainOptions[selectedMenuIndex]);
    GetMainInteraction(mainOptions);
}

void Bye()
{
    Console.WriteLine("Bye...");
    Thread.Sleep(2000);
    Console.WriteLine("");
    Environment.Exit(0);
}




void GetMainInteraction(List<Option> options)
{
    ConsoleKeyInfo keyinfo;
    do
    {
        keyinfo = Console.ReadKey();
        if (keyinfo.Key == ConsoleKey.DownArrow)
        {
            if (selectedMenuIndex + 1 < options.Count)
            {
                selectedMenuIndex++;
                WriteMenu(options, options[selectedMenuIndex]);
            }
        }

        if (keyinfo.Key == ConsoleKey.UpArrow)
        {
            if (selectedMenuIndex - 1 >= 0)
            {
                selectedMenuIndex--;
                WriteMenu(options, options[selectedMenuIndex]);
            }
        }

        // Handle different action for the option
        if (keyinfo.Key == ConsoleKey.Enter)
        {
            options[selectedMenuIndex].Selected.Invoke();
            selectedMenuIndex = 0;
        }

        //if (Int32.TryParse(keyinfo.KeyChar.ToString(), out var number))
        //{
        //    if (options.Select(e => e.Id).Contains(number))
        //    {
        //        selectedMenuIndex = number-1;//0 based index
        //        options.First(e => e.Id == number).Selected.Invoke();
        //    }
        //}

        
    } while (keyinfo.Key != ConsoleKey.X) ;
}



void WriteMenu(List<Option> options, Option selectedOption)
{
    Console.Clear();
    InitialIntro();

    if (initialLoad)
    { 
        initialLoad = false;
    }

    Console.WriteLine("Please select ... ");

    foreach (Option option in options)
    {
        if (option == selectedOption)
        {
            Console.Write("> ");
        }
        else
        {
            Console.Write("  ");
        }

        Console.WriteLine(option.Name);
    }
}


void LoadMainMenu()
{
    selectedMenuIndex = 0;
    WriteMenu(mainOptions, mainOptions[selectedMenuIndex]);
    GetMainInteraction(mainOptions);
}

LoadMainMenu();


void SetText(string text)
{
    var powershell = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-command \"Set-Clipboard -Value \\\"{text}\\\"\""
        }
    };
    powershell.Start();
    powershell.WaitForExit();
}

readonly record struct Option(int Id, string Name, Action Selected)
{
};

