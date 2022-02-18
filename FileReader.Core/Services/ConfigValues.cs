using FileReader.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FileReader.Core.Services
{
    public  class ConfigValues: IConfigValues
    {
        private readonly IConfiguration _config;

        public  ConfigValues(IConfiguration config)
        {
            _config = config;
            UpdateBase(config);
        }

        public void UpdateBase(IConfiguration config)
        {
            ProfileName = _config.GetValue<string>("ProfileName");
            FolderPath = _config.GetValue<string>("FolderPath");
            FileFilter = _config.GetValue<string>("FileFilter");
            FileNameCleanerRegex = _config.GetValue<string>("FileNameCleanerRegex");
            SampleSize = _config.GetValue<int>("SampleSize");
            DefaultConnectionString = _config.GetConnectionString("Default");
            DefaultSchema = _config.GetValue<string>("DefaultSchema");
            SqlPreface = _config.GetValue<string>("SqlPreface");
            StringLengthPaddingMultiplyer = _config.GetValue<decimal>("StringLengthPaddingMultiplyer");
            Codepage = _config.GetSection("BulkInsertOptions").GetValue<string>("Codepage");
            DataFileType  = _config.GetSection("BulkInsertOptions").GetValue<string>("DataFileType");
            FieldTerminator = _config.GetSection("BulkInsertOptions").GetValue<string>("FieldTerminator");
            RowTerminator = _config.GetSection("BulkInsertOptions").GetValue<string>("RowTerminator");
            MultiFileToTableRegex = _config.GetValue<string>("MultiFileToTableRegex");
        }

        public ConfigValues()
        { }

        public void UpdateValues(ConfigValues values)
        {
            this.ProfileName = values.ProfileName;
            this.FolderPath = values.FolderPath;
            this.FileFilter = values.FileFilter;
            this.FileNameCleanerRegex = values.FileNameCleanerRegex;
            this.SampleSize = values.SampleSize;
            this.DefaultConnectionString = values.DefaultConnectionString;
            this.DefaultSchema = values.DefaultSchema;
            this.SqlPreface = values.SqlPreface;
            this.StringLengthPaddingMultiplyer = values.StringLengthPaddingMultiplyer;
            this.Codepage = values.Codepage;
            this.DataFileType = values.DataFileType;
            this.FieldTerminator = values.FieldTerminator;
            this.RowTerminator = values.RowTerminator;
            this.MultiFileToTableRegex = values.MultiFileToTableRegex;
        }

        public string ProfileName { get; set; }
        public string FolderPath {get; set; } 
        public string FileFilter {get; set; } 
        public string FileNameCleanerRegex { get; set; }
        public int SampleSize { get; set; }
        public string DefaultConnectionString { get; set; }
        public string DefaultSchema { get; set; }
        public string SqlPreface { get; set; }
        public decimal StringLengthPaddingMultiplyer { get; set; }
        public string Codepage { get; set; }
        public string DataFileType { get; set; }
        public string FieldTerminator { get; set; }
        public string RowTerminator { get; set; }
        public string MultiFileToTableRegex { get; set; }

        public void LoadSettingsProfile(string name)
        {
            checkProfileDirectoryExists();
            var fn = getFileName(name);
            var contents = File.ReadAllText(fn);
            var tmp = JsonConvert.DeserializeObject<ConfigValues>(contents);
            if (tmp!=null)
            { 
                UpdateValues(tmp);
            }
        }

        public void LoadDefaultConfig(IConfiguration baseconfig)
        {
            UpdateBase(baseconfig);
        }
        public void CopySettingsProfileTo(string name)
        {
            checkProfileDirectoryExists();
            var fn = getFileName(name);
            ProfileName = name;
            File.WriteAllText(fn, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public List<string> GetProfiles()
        {
            checkProfileDirectoryExists();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Profiles");
            var files = new DirectoryInfo(path).GetFiles();
            return files.Select(e=> Path.GetFileNameWithoutExtension(e.Name)).ToList();

        }
        private string getFileName(string name)
        {
            return $"{Directory.GetCurrentDirectory()}\\profiles\\{ name }.json";
        }

        private void checkProfileDirectoryExists()
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Profiles")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Profiles"));
            }
        
        }
               

        public Dictionary<string, object> GetAllSettingsProfiles()
        {
            checkProfileDirectoryExists();

            var configs =  new Dictionary<string, object>();
            configs.Add("FolderPath", FolderPath);
            configs.Add("FileFilter", FileFilter);
            configs.Add("FileNameCleanerRegex", FileNameCleanerRegex);
            configs.Add("SampleSize", SampleSize);
            configs.Add("DefaultConnectionString", DefaultConnectionString);
            configs.Add("DefaultSchema", DefaultSchema);
            configs.Add("SqlPreface", SqlPreface);
            configs.Add("StringLengthPaddingMultiplyer", StringLengthPaddingMultiplyer);
            configs.Add("Codepage", Codepage);
            configs.Add("DataFileType", DataFileType);
            configs.Add("FieldTerminator", FieldTerminator);
            configs.Add("RowTerminator", RowTerminator);
            configs.Add("MultiFileToTableRegex", MultiFileToTableRegex);

            return configs;
        }

       
        public void UpdateSetting(string name, string newValue)
        {
            switch (name)
            {
                case "FolderPath":
                    FolderPath = newValue;
                    break;
                case "FileFilter":
                    FileFilter = newValue;
                    break;
                case "FileNameCleanerRegex":
                    FileNameCleanerRegex = newValue;
                    break;
                case "SampleSize":
                    SampleSize = int.Parse(newValue);
                    break;
                case "DefaultConnectionString":
                    DefaultConnectionString = newValue;
                    break;
                case "DefaultSchema":
                    DefaultSchema = newValue;
                    break;
                case "SqlPreface":
                    SqlPreface = newValue;
                    break;
                case "StringLengthPaddingMultiplyer":
                    StringLengthPaddingMultiplyer = decimal.Parse(newValue);
                    break;
                case "Codepage":
                    Codepage = newValue;
                    break;
                case "DataFileType":
                    DataFileType = newValue;
                    break;
                case "FieldTerminator":
                    FieldTerminator = newValue;
                    break;
                case "RowTerminator":
                    RowTerminator = newValue;
                    break;
                case "MultiFileToTableRegex":
                    MultiFileToTableRegex = newValue;
                    break;

            }
            
            var fn = getFileName(ProfileName);
            File.WriteAllText(fn, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
