using FileReader.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Core.Services
{
    public  class ConfigValues: IConfigValues
    {
        private readonly IConfiguration _config;

        public  ConfigValues(IConfiguration config)
        {
            this._config = config;
        }

        public string FolderPath => _config.GetValue<string>("FolderPath");

        public string FileFilter => _config.GetValue<string>("FileFilter");

        public string FileNameCleanerRegex => _config.GetValue<string>("FileNameCleanerRegex");

        public int SampleSize => _config.GetValue<int>("SampleSize");

        public string DefaultConnectionString => _config.GetConnectionString("Default");

        public string DefaultSchema => _config.GetConnectionString("DefaultSchema");
    }
}
