using FileReader.Core.Enums;
using FileReader.Core.Interfaces;
using FileReader.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace FileReader.Core.Services
{
   
    public class FileService :  IFileService
    {

        List<Column> columns = null!;
        private readonly ILogger<FileService> _logger = null!;
        private readonly IConfigValues _config;
        private readonly ITypeProcessingService _typeProcessingService;

        public FileService(ITypeProcessingService typeProcessingService, IConfigValues config, ILogger<FileService> logger)
        {
            _config = config;
            _logger = logger;
            _typeProcessingService = typeProcessingService;
        }

        public List<Column> GetColumsMetaData(string path, int sampleSize = 50)
        {
            columns =  new List<Column>();

            using StreamReader readingFile = new StreamReader(path);

            string readingLine = readingFile.ReadLine();

            var columnsHeaders = readingLine.Split('|');

            var stringLengthPaddingMultiplyer = _config.StringLengthPaddingMultiplyer;


            for (var i = 0; i < columnsHeaders.Count(); i++)
            {
                var c = new Column { OrdinalPosition = i, Title = columnsHeaders[i] };
                columns.Add(c);
            }

            for (var i = 0; i < sampleSize; i++)
            {
                ProcessLine(readingFile);
            }

            foreach (var col in columns)
            {
                if (col.HeadingDataType == HeadingDataType.String)
                {

                    col.MaxLength = (int) (Math.Round(col.MaxLength * stringLengthPaddingMultiplyer));
                }
            }

            readingFile.Dispose();
            
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
                _typeProcessingService.UpdateColumn(p, col);
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
            string tableName="";
            if (Path.HasExtension(filename))
                tableName = Path.GetFileNameWithoutExtension(filename);

            var multiFileToTableRegex = "";

            if (!string.IsNullOrWhiteSpace(multiFileToTableRegex))
            {
                var re = new Regex(multiFileToTableRegex);
                tableName = re.Replace(tableName, "");
            }

            var cleanerExpression = _config.FileNameCleanerRegex;

            if (!string.IsNullOrWhiteSpace(cleanerExpression))
            {
                var re = new Regex(cleanerExpression);
                tableName = re.Replace(tableName, "");
            }
            return tableName;
        }
    }
}
