﻿using FileReader.Core.Interfaces;
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

        public string DefaultSchema => _config.GetValue<string>("DefaultSchema");

        public string SqlPreface => _config.GetValue<string>("SqlPreface");

        public decimal StringLengthPaddingMultiplyer => _config.GetValue<decimal>("StringLengthPaddingMultiplyer");

        public string Codepage => _config.GetSection("BulkInsertOptions").GetValue<string>("Codepage");
        public string DataFileType => _config.GetSection("BulkInsertOptions").GetValue<string>("DataFileType");
        public string FieldTerminator => _config.GetSection("BulkInsertOptions").GetValue<string>("FieldTerminator");
        public string RowTerminator => _config.GetSection("BulkInsertOptions").GetValue<string>("RowTerminator");


    }
}
