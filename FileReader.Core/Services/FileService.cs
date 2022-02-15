using FileReader.Core.Enums;
using FileReader.Core.Interfaces;
using FileReader.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileReader.Core.Services
{
   
    public class FileService :  IFileService
    {

        List<Column> columns = null!;
        private readonly ILogger<FileService> _logger = null!;
        private readonly IConfigValues _config;

        public FileService(IConfigValues config, ILogger<FileService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public List<Column> GetColumsMetaData(string path, int sampleSize = 50)
        {
            columns =  new List<Column>();

            using System.IO.StreamReader readingFile = new System.IO.StreamReader(path);

            string readingLine = readingFile.ReadLine();

            var columnsHeaders = readingLine.Split('|');

            for (var i = 0; i < columnsHeaders.Count(); i++)
            {
                var c = new Column { OrdinalPosition = i, Title = columnsHeaders[i] };
                columns.Add(c);
            }

            for (var i = 0; i< sampleSize; i++)
            {
                ProcessLine(readingFile);
            }

            foreach (var col in columns)
            {
                if (col.HeadingDataType == HeadingDataType.String)
                {
                    col.MaxLength = col.MaxLength * 3;
                }
            }

            return columns;


        }

        private void ProcessLine(StreamReader sr)
        {
            string? v = null!;
            if (!sr.EndOfStream)
                v = sr.ReadLine();

            if (string.IsNullOrWhiteSpace(v)) return;

            var line = v.Split('|');

            for (var i = 0; i < columns.Count; i++)
            {
                var col = columns.Single(e => e.OrdinalPosition ==i);

                var p = line[i];


                var isBool = bool.TryParse(p, out var isb);
                if (isBool && p!="True" &&p!="False") continue;

                var isDate = DateTime.TryParse(p, out var isda);
                if (isDate)
                {
                    if (col.HeadingDataType >= HeadingDataType.DateTime) continue;

                    col.HeadingDataType = HeadingDataType.DateTime;
                    //col.MaxLength = 20;
                    continue;
                }

                var isInt = int.TryParse(p, out var isi);
                if (isInt)
                {
                    if (col.HeadingDataType >= HeadingDataType.Int) continue;

                    col.HeadingDataType = HeadingDataType.Int;
                    continue;
                }

                var isBigInt = Int64.TryParse(p, out var isBig);
                if (isBigInt)
                {
                    if (col.HeadingDataType >= HeadingDataType.BigInt) continue;

                    col.HeadingDataType = HeadingDataType.BigInt;
                    continue;
                }

                var isDecimal = decimal.TryParse(p, out var isdc);
                if (isDecimal)
                {
                    if (col.HeadingDataType >= HeadingDataType.Decimal) continue;
                    col.HeadingDataType = HeadingDataType.Decimal;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(p))
                {
                    if (!col.IsNullable) col.IsNullable = true;
                    continue;
                }


                var stringLen = p.Length + 5;
                if (stringLen > col.MaxLength)
                {
                    col.HeadingDataType = HeadingDataType.String;
                    col.MaxLength = stringLen;
                }
            }

        }

        public IEnumerable<FileInfo> GetFiles(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            var filter = _config.FileFilter;

            return di.GetFiles(filter);
        }

        public string GetTableName(string filename)
        {

            if (Path.HasExtension(filename))
                filename = Path.GetFileNameWithoutExtension(filename);

            var cleanerExpression = _config.FileNameCleanerRegex;

            if (!string.IsNullOrWhiteSpace(cleanerExpression))
            {
                var re = new Regex(cleanerExpression);
                filename = re.Replace(filename, "");
            }
            return filename;
        }
    }
}
